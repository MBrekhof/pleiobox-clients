using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.App;
using Android.Webkit;
using Android.Util;
using Android.Database;
using Android.Preferences;
using Android.Views.InputMethods;

using LocalBox_Common;
using LocalBox_Common.Remote;

namespace LocalBox_Droid
{
	[Activity (Label = "LocalBox", WindowSoftInputMode = SoftInput.AdjustPan, ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class HomeActivity : FragmentActivity
	{
		public static List<ExplorerFragment> openedExplorerFragments;
		public static string colorOfSelectedLocalBox;
		public static bool shouldLockApp = false;
		public ImageButton buttonFullscreenDocument;
		public MenuFragment menuFragment;
		public Android.App.DialogFragment dialogFragmentShare;
		public Android.App.DialogFragment dialogFragmentMoveFile;
		private ImageButton buttonBackExplorer;
		private ImageButton buttonAddFolderExplorer;
		private ImageButton buttonUploadFileExplorer;
		private ImageButton buttonRefreshExplorer;
		private ImageButton buttonCloseDocument;
		private TextView textviewFilename;
		private RelativeLayout fragmentContainerExplorerBottom;
		private View shadowContainerExplorer;
		private CustomProgressDialog progressDialog =  new CustomProgressDialog();
		private DialogHelperForHomeActivity dialogHelper;

		protected override async void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.activity_home);

			openedExplorerFragments = new List<ExplorerFragment>();
			dialogHelper = new DialogHelperForHomeActivity (this);

			this.ActionBar.Hide ();

			//////////////////////////////////////////////////////
			//Menu part of layout
			//////////////////////////////////////////////////////

			//Initialize menu fragment
			menuFragment = new MenuFragment ();
			SupportFragmentManager.BeginTransaction ().Add (Resource.Id.fragment_container_menu, menuFragment).Commit ();

			//Initialize bottom menu fragment
			BottomMenuFragment bottomMenuFragment = new BottomMenuFragment ();
			SupportFragmentManager.BeginTransaction ().Add (Resource.Id.fragment_container_menu_bottom, bottomMenuFragment).Commit ();


			//Controleer of introduction dialog getoond moet worden (alleen wanneer geen local boxen geregistreerd zijn)
			List<LocalBox> registeredLocalBoxes = await DataLayer.Instance.GetLocalBoxes ();
			if (registeredLocalBoxes.Count == 0) {
				if (SplashActivity.intentData == null) {
					ShowIntroductionDialog ();
				}
			}


			//////////////////////////////////////////////////////
			//Explorer part of layout
			//////////////////////////////////////////////////////
			buttonBackExplorer = FindViewById<ImageButton> (Resource.Id.button_back_explorer);
			buttonAddFolderExplorer = FindViewById<ImageButton> (Resource.Id.button_add_folder_explorer);
			buttonUploadFileExplorer = FindViewById<ImageButton> (Resource.Id.button_upload_file_explorer);
			buttonRefreshExplorer = FindViewById<ImageButton> (Resource.Id.button_refresh_explorer);

			fragmentContainerExplorerBottom = FindViewById<RelativeLayout> (Resource.Id.fragment_container_explorer_blank);
			fragmentContainerExplorerBottom.Visibility = ViewStates.Invisible;

			shadowContainerExplorer = FindViewById<View> (Resource.Id.shadow_container_explorer);
			shadowContainerExplorer.Visibility = ViewStates.Invisible;

			buttonBackExplorer.Visibility = ViewStates.Invisible;
			buttonBackExplorer.Click += delegate {

				//Verwijder fragment van stack
				SupportFragmentManager.PopBackStack ();

				//Remove last opened directory from opened directory list
				int numberOfDirectoriesOpened = ExplorerFragment.openedDirectories.Count;
				if (numberOfDirectoriesOpened > 0) {
					ExplorerFragment.openedDirectories.RemoveAt (numberOfDirectoriesOpened - 1);
					RemoveLastOpenedExplorerFragment();
					ShowBottomExplorerMenuItems ();
				}
				CheckToHideButtons ();
			};

			buttonAddFolderExplorer.Click += delegate {
				ShowNewFolderDialog ();
			};

			//Hide upload file button on root level
			buttonUploadFileExplorer.Visibility = ViewStates.Invisible;
			buttonUploadFileExplorer.Click += delegate {
			
				//Show menu to make a choice between new folder or upload file
				PopupMenu popupMenu = new PopupMenu (this, buttonUploadFileExplorer);
				popupMenu.Inflate (Resource.Menu.menu_add);
				popupMenu.MenuItemClick += (s1, arg1) => {

					if (arg1.Item.ItemId.Equals (Resource.Id.menu_explorer_upload_photo)) {//Upload foto of video geselecteerd
						var imageIntent = new Intent ();
						imageIntent.SetType ("image/*");
						imageIntent.SetAction (Intent.ActionGetContent);
						StartActivityForResult (Intent.CreateChooser (imageIntent, "Select photo"), 0);
					} else if (arg1.Item.ItemId.Equals (Resource.Id.menu_explorer_upload_file)) {//Upload ander bestandstype geselecteerd
						var filePickerIntent = new Intent (this, typeof(FilePickerActivity));
						StartActivity (filePickerIntent);  
					}

				};
				popupMenu.Show ();
			};

			buttonRefreshExplorer.Click += delegate {
				ExplorerFragment fragment = GetLastOpenedExplorerFragment();
				fragment.RefreshData();
			};


			//////////////////////////////////////////////////////
			//Document part of layout
			//////////////////////////////////////////////////////
			buttonFullscreenDocument = FindViewById<ImageButton> (Resource.Id.button_fullscreen_document);
			textviewFilename = FindViewById<TextView> (Resource.Id.textview_filename);
			buttonCloseDocument = FindViewById<ImageButton> (Resource.Id.button_close_document);

			buttonFullscreenDocument.Visibility = ViewStates.Invisible;
			textviewFilename.Visibility = ViewStates.Invisible;
			buttonCloseDocument.Visibility = ViewStates.Invisible;

			//Open file fullscreen in new activity
			buttonFullscreenDocument.Click += delegate {
				var documentFullscreenIntent = new Intent (this, typeof(DocumentFullscreenActivity));
				StartActivity (documentFullscreenIntent);  
			};
				
			//Determine to save PDF annotations
			if (SplashActivity.intentData != null) { 
				Android.Net.Uri data = SplashActivity.intentData; 
				String scheme = data.Scheme;
				if (scheme.Equals ("file")) { //Save annotations
					UpdatePdfFile (data.Path);
				}
			}
			else if (SplashActivity.clipData != null) 
			{
				Android.Net.Uri uri = SplashActivity.clipData.GetItemAt (0).Uri;
				UpdatePdfFile (uri.ToString ());
			}
			HideProgressDialog ();



			SslValidator.CertificateMismatchFound += (object sender, EventArgs e) => 
			{
				SslValidator.CertificateErrorRaised = true;

				//Incorrect ssl found so show message
				Console.WriteLine ("SSL mismatch!!!");
				this.RunOnUiThread (() => {
					dialogHelper.ShowCertificateMismatchDialog();
				});
			};
		}


		private void UpdatePdfFile (string pathToTempPdf)
		{
			try {
				new Thread (new ThreadStart (async delegate {

					//Get location and name of last opened file
					ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (this);         
					var fileNameLastOpenedPdf = prefs.GetString ("fileNameLastOpenedPdf", null);
					var pathLastOpenedPdf = prefs.GetString ("pathLastOpenedPdf", null);
					var temporaryFilePath = prefs.GetString ("temporaryFilePath", null);
					var isFavorite = prefs.GetBoolean ("isFavorite", false);

					//Controleer of pdf path overeenkomt met path van laatst geopende pdf file
					if (pathToTempPdf.Contains (fileNameLastOpenedPdf)) {
						ShowProgressDialog ("Eventuele aanpassingen aan het bestand opslaan. \nEen ogenblik geduld a.u.b.");
						bool updateSucceeded = await DataLayer.Instance.SavePdfAnnotations (pathLastOpenedPdf, pathToTempPdf, true, isFavorite);
						
						if (updateSucceeded) {
							if (File.Exists (temporaryFilePath)) {
								File.Delete (temporaryFilePath);
							}
							HideProgressDialog ();
							ShowToast ("PDF bestand succesvol bijgewerkt");
						} else {
							HideProgressDialog ();
							ShowToast ("Er is een fout opgetreden bij het bijwerken van het PDF bestand. \n" +
							"Probeer het a.u.b. later nogmaals");
						}
					} else {
						HideProgressDialog ();
						ShowToast ("PDF bestand niet gevonden.\n" +
						"Nieuwe bestanden kunnen alleen binnen de app ge-upload worden.");
					}
					SplashActivity.clipData = null;
				})).Start ();
			} catch {
				SplashActivity.clipData = null;
				HideProgressDialog ();
				ShowToast ("Er is een fout opgetreden. Probeer het a.u.b. later nogmaals");
			}
		}
			
		protected override void OnStop ()
		{
			HideProgressDialog ();
			base.OnStop ();
		}

		protected override void OnPause ()
		{
			base.OnPause ();
			LockHelper.SetLastActivityOpenedTime ("HomeActivity");
		}
			
		protected override void OnResume ()
		{
			base.OnResume ();

			bool shouldShowLockScreen = LockHelper.ShouldLockApp ("HomeActivity");
			if (shouldShowLockScreen || shouldLockApp) { //Lock scherm
				HomeActivity.shouldLockApp = true;
				StartActivity(typeof(PinActivity));
				DataLayer.Instance.LockDatabase ();
			} else {
				ExplorerFragment explorerFragment = GetLastOpenedExplorerFragment ();
				if (explorerFragment != null && !explorerFragment.favoriteFolderOpened && SplashActivity.intentData == null) {
					explorerFragment.RefreshData ();
				}
			}
		}

		public override void OnBackPressed ()
		{
			base.OnBackPressed ();

			CheckToHideButtons ();

			//Remove last opened directory from opened directory list
			if (ExplorerFragment.openedDirectories != null) {
				int numberOfDirectoriesOpened = ExplorerFragment.openedDirectories.Count;
				if (numberOfDirectoriesOpened > 0) {
					ExplorerFragment.openedDirectories.RemoveAt (numberOfDirectoriesOpened - 1);
					RemoveLastOpenedExplorerFragment ();
					ShowBottomExplorerMenuItems ();
				}
			}
			if (ExplorerFragment.openedDirectories.Count > 1) {
				buttonUploadFileExplorer.Visibility = ViewStates.Visible;
			} else {
				buttonUploadFileExplorer.Visibility = ViewStates.Invisible;
			}
		}
			
		private ExplorerFragment GetLastOpenedExplorerFragment()
		{
			if (openedExplorerFragments.Count > 0) {
				return openedExplorerFragments [openedExplorerFragments.Count - 1];
			} else {
				return null;
			}
		}
			
		private void RemoveLastOpenedExplorerFragment()
		{
			if (openedExplorerFragments.Count > 0) {
				openedExplorerFragments.RemoveAt (openedExplorerFragments.Count - 1);
			}
		}

		public void RefreshExplorerFragmentData()
		{
			ExplorerFragment explorerFragment = GetLastOpenedExplorerFragment();
			if(explorerFragment != null){
				explorerFragment.RefreshData ();
			}
		}

		//Resultaat van image picker
		protected override async void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);

			if (resultCode == Result.Ok) {
				Android.Net.Uri uriResult = data.Data;
				try {
					int numberOfDirectoriesOpened = ExplorerFragment.openedDirectories.Count;
					string directoryNameToUploadFileTo = ExplorerFragment.openedDirectories [numberOfDirectoriesOpened - 1];
					string pathToFile = GetPathToImage (uriResult);
					string fileName = System.IO.Path.GetFileName (pathToFile);
					string fullDestinationPath = System.IO.Path.Combine (directoryNameToUploadFileTo, fileName);
					bool uploadedSucceeded = await DataLayer.Instance.UploadFile (fullDestinationPath, pathToFile);

					HideProgressDialog ();
					if (!uploadedSucceeded) {
						ShowToast("Het uploaden is mislukt. Probeer het a.u.b. opnieuw");
					} else {
						ShowToast("Bestand succesvol geupload");
						RefreshExplorerFragmentData();
					}
				} catch {
					HideProgressDialog ();
					ShowToast ("Het uploaden is mislukt. Probeer het a.u.b. opnieuw");
				}
			}
		}

		private string GetPathToImage (Android.Net.Uri uri)
		{
			string path = null;

			string[] projection = new[] { Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data };
			using (ICursor cursor = ManagedQuery (uri, projection, null, null, null)) {
				if (cursor != null) {
					int columnIndex = cursor.GetColumnIndexOrThrow (Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
					cursor.MoveToFirst ();
					path = cursor.GetString (columnIndex);
				}
			}
			return path;
		}

		public void CheckToHideButtons ()
		{
			//Als maar 1 fragment in stack dan verberg back button
			if (SupportFragmentManager.BackStackEntryCount < 3) {
				buttonBackExplorer.Visibility = ViewStates.Invisible;

				if (SupportFragmentManager.BackStackEntryCount == 0) {
					fragmentContainerExplorerBottom.Visibility = ViewStates.Invisible;
					shadowContainerExplorer.Visibility = ViewStates.Invisible;

					ClearContentInDocumentFragment ();
				}
			}
		}

		public void ClearContentInDocumentFragment ()
		{
			textviewFilename.Visibility = ViewStates.Invisible;
			buttonFullscreenDocument.Visibility = ViewStates.Invisible;

			//Clear webview
			WebView webViewDocument = FindViewById<WebView> (Resource.Id.webview_document);
			webViewDocument.ClearView ();
		}



		private void ShowNewFolderDialog ()
		{
			dialogHelper.ShowNewFolderDialog ();
		}

		public void ShowIntroductionDialog ()
		{
			dialogHelper.ShowIntroductionDialog ();
		}


		public void ShowAboutAppDialog ()
		{
			dialogHelper.ShowAboutAppDialog ();
		}
			
		public void ShowShareDialog (string pathOfFolderToShare, bool alreadyShared)
		{
			dialogHelper.ShowShareDialog (pathOfFolderToShare, alreadyShared);
		}


		public void HideShareDialog (bool isNewShare)
		{
			dialogHelper.HideShareDialog (isNewShare);
		}


		public void ShowMoveFileDialog(TreeNode treeNodeToMove)
		{
			dialogHelper.ShowMoveFileDialog (treeNodeToMove);
		}
			
		public void HideMoveFileDialog ()
		{
			dialogHelper.HideMoveFileDialog ();
		}
			
		public void ShowShareFileDatePicker(string pathToNewFileShare)
		{
			dialogHelper.ShowShareFileDatePicker (pathToNewFileShare);
		}


	
			

		////////////////////////////////////////////////////////////////////////////////////////////////////
		///////Below you can find methods to register a new LocalBox 
		////////////////////////////////////////////////////////////////////////////////////////////////////

		//Register new LocalBox part 1
		public void ShowOpenUrlDialog ()
		{
			dialogHelper.ShowOpenUrlDialog ();
		}

		private void ShowHttpWarningDialog(string urlToOpen)
		{
			dialogHelper.ShowHttpWarningDialog (urlToOpen);
		}

		private void ShowInvalidCertificateDialog(string urlToOpen)
		{
			dialogHelper.ShowInvalidCertificateDialog (urlToOpen);
		}
			
		//Register new LocalBox part 2	
		public void ShowRegisterLocalBoxDialog (string urlToOpen, bool ignoreSslError)
		{
			dialogHelper.ShowRegisterLocalBoxDialog (urlToOpen, ignoreSslError);
		}
	
		//Register new LocalBox part 3
		public void AddLocalBox(LocalBox lbToAdd)
		{
			dialogHelper.AddLocalBox (lbToAdd);
		}

		//Register new LocalBox part 4
		public void SetUpPassphrase (LocalBox localBox)
		{
			dialogHelper.SetUpPassphrase (localBox);
		}

		//Register new LocalBox part 4
		public void EnterPassphrase (LocalBox localBox)
		{
			dialogHelper.EnterPassphrase (localBox);
		}
			




		public void HideBottomExplorerMenuItems ()
		{
			buttonAddFolderExplorer.Visibility 	= ViewStates.Invisible;
			buttonUploadFileExplorer.Visibility = ViewStates.Invisible;
			buttonRefreshExplorer.Visibility 	= ViewStates.Invisible;
		}

		public void ShowBottomExplorerMenuItems ()
		{
			buttonAddFolderExplorer.Visibility 	= ViewStates.Visible;
			buttonUploadFileExplorer.Visibility = ViewStates.Visible;
			buttonRefreshExplorer.Visibility 	= ViewStates.Visible;
		}
			
		public void ShowProgressDialog (string textToShow)
		{
			this.RunOnUiThread (new Action (() => { 
				progressDialog.Show (this, textToShow);
			}));
		}

		public void HideProgressDialog ()
		{
			try{
				if (progressDialog != null) {
					this.RunOnUiThread (new Action (() => { 
						progressDialog.Hide ();
					}));
				}
			}catch{
			}
		}

		public void ShowToast(string message)
		{
			try{
				this.RunOnUiThread (new Action (() => { 
					Toast.MakeText (this, message, ToastLength.Long).Show ();
				}));
			}catch{
			}
		}

	}
}
