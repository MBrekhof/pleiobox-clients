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

using LocalBox_Common;
using LocalBox_Common.Remote;

namespace localbox.android
{
	[Activity (Label = "LocalBox", WindowSoftInputMode = SoftInput.AdjustPan, ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape)]
	public class HomeActivity : FragmentActivity
	{
		public static List<ExplorerFragment> openedExplorerFragments;
		public static string colorOfSelectedLocalBox;
		public static bool shouldLockApp = false;
		public string pathToNewFileShare;
		public  ImageButton buttonFullscreenDocument;
		private DateTime selectedExpirationDate;
		private ImageButton buttonBackExplorer;
		private ImageButton buttonAddFolderExplorer;
		private ImageButton buttonUploadFileExplorer;
		private ImageButton buttonRefreshExplorer;
		private ImageButton buttonCloseDocument;
		private TextView textviewFilename;
		private RelativeLayout fragmentContainerExplorerBottom;
		private View shadowContainerExplorer;
		private MenuFragment menuFragment;
		private Android.App.DialogFragment dialogFragmentShare;
		private Android.App.DialogFragment dialogFragmentMoveFile;
		private CustomProgressDialog progressDialog =  new CustomProgressDialog();

		protected override async void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.activity_home);

			openedExplorerFragments = new List<ExplorerFragment>();

			//Hide action bar
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
				String scheme = data.Scheme; // "http" 

				if (scheme.Equals ("file")) //Save annotations
				{ 
					UpdatePdfFile (data.Path);
				}
			}
			else if (SplashActivity.clipData != null) 
			{
				Android.Net.Uri uri = SplashActivity.clipData.GetItemAt (0).Uri;
				UpdatePdfFile (uri.ToString ());
			}
			HideProgressDialog ();
		}


		private async void UpdatePdfFile (string pathToTempPdf)
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
			if (shouldShowLockScreen || shouldLockApp) {
				//Lock scherm
				HomeActivity.shouldLockApp = true;
				StartActivity(typeof(PinActivity));
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

		private void RefreshExplorerFragmentData()
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
			LayoutInflater factory = LayoutInflateHelper.GetLayoutInflater (this);

			View viewNewFolder = factory.Inflate (Resource.Layout.dialog_new_folder, null);

			EditText editTextFolderName = (EditText)viewNewFolder.FindViewById<EditText> (Resource.Id.editText_dialog_folder_name);          

			// Build the dialog
			var dialogBuilder = new AlertDialog.Builder (this);
			dialogBuilder.SetTitle (Resource.String.folder_new);
			dialogBuilder.SetView (viewNewFolder);

			dialogBuilder.SetPositiveButton (Resource.String.add, (EventHandler<DialogClickEventArgs>)null);
			dialogBuilder.SetNegativeButton (Resource.String.cancel, (EventHandler<DialogClickEventArgs>)null);

			var dialog = dialogBuilder.Create ();
			dialog.Show ();

			// Get the buttons.
			var buttonAddFolder = dialog.GetButton ((int)DialogButtonType.Positive);
			var buttonCancel = dialog.GetButton ((int)DialogButtonType.Negative);

			buttonAddFolder.Click += async(sender, args) => {
				if (String.IsNullOrEmpty (editTextFolderName.Text)) {
					ShowToast("Naam is niet ingevuld");
				} else {
					ShowProgressDialog("Map wordt aangemaakt. Een ogenblik geduld a.u.b.");
					try{
						int numberOfDirectoriesOpened = ExplorerFragment.openedDirectories.Count;
						string directoryNameToUploadFileTo = ExplorerFragment.openedDirectories [numberOfDirectoriesOpened - 1];
					
						bool addedSuccesfully = (await DataLayer.Instance.CreateFolder (System.IO.Path.Combine (directoryNameToUploadFileTo, (editTextFolderName.Text))));
					
						dialog.Dismiss ();
					
						if (!addedSuccesfully) {
							HideProgressDialog();
							ShowToast("Toevoegen map mislukt. Probeer het a.u.b. opnieuw");
						} else {
							HideProgressDialog();
							ShowToast("Map succesvol toegevoegd");
						
							//Refresh data
							RefreshExplorerFragmentData();
						}
					}catch{
						HideProgressDialog();
						ShowToast("Toevoegen map mislukt. Probeer het a.u.b. opnieuw");
					}
				}
			};
			buttonCancel.Click += (sender, args) => {
				dialog.Dismiss ();
			};
		}

		public void ShowIntroductionDialog ()
		{
			var alertDialogIntro = new AlertDialog.Builder (this);
			alertDialogIntro.SetView (LayoutInflater.Inflate (Resource.Layout.fragment_introduction, null));
			
			alertDialogIntro.SetPositiveButton ("Start de registratie van een nieuwe LocalBox", delegate { 
				ShowOpenUrlDialog();
				alertDialogIntro.Dispose();
			});
			alertDialogIntro.Create ().Show ();
		}


		public void ShowAboutAppDialog ()
		{
			Android.App.FragmentTransaction fragmentTransaction;
			fragmentTransaction = FragmentManager.BeginTransaction ();

			Android.App.DialogFragment dialogFragmentAboutApp;
			AboutAppFragment aboutFragment = new AboutAppFragment ();

			dialogFragmentAboutApp = aboutFragment;
			dialogFragmentAboutApp.Show (fragmentTransaction, "aboutdialog");
		}
			


		public async void ShowShareDialog (string pathOfFolderToShare, bool alreadyShared)
		{
			ShowProgressDialog (null);

			try {
				Android.App.FragmentTransaction fragmentTransaction;
				fragmentTransaction = FragmentManager.BeginTransaction ();

				List<Identity> localBoxUsers = await DataLayer.Instance.GetLocalboxUsers();
				Share shareSettings = null;

				if (alreadyShared) {//Existing share
					//Get share settings
					shareSettings = await BusinessLayer.Instance.GetShareSettings (pathOfFolderToShare);
				}

				ShareFragment shareFragment = new ShareFragment (pathOfFolderToShare, localBoxUsers, shareSettings);
				dialogFragmentShare = shareFragment;

				HideProgressDialog ();

				if (localBoxUsers.Count > 0) {
					dialogFragmentShare.Show (fragmentTransaction, "sharedialog");
				} else {
					ShowToast("Geen gebruikers gevonden om mee te kunnen delen");
				}
			} catch {
				HideProgressDialog ();
			}
		}


		public void HideShareDialog (bool isNewShare)
		{
			dialogFragmentShare.Dismiss ();

			if (isNewShare) {
				ShowToast ("Map succesvol gedeeld");
			} else {
				ShowToast ("Deel instellingen succesvol gewijzigd");
			}
			RefreshExplorerFragmentData ();
		}


		public async void ShowMoveFileDialog(TreeNode treeNodeToMove)
		{
			Console.WriteLine ("Bestand om te verplaatsen: " + treeNodeToMove.Name);
			ShowProgressDialog (null);
			try {
				Android.App.FragmentTransaction fragmentTransaction;
				fragmentTransaction = FragmentManager.BeginTransaction ();

				List<TreeNode>foundDirectoryTreeNodes 	= new List<TreeNode>();
				TreeNode rootTreeNode 			= await DataLayer.Instance.GetFolder ("/");

				foreach(TreeNode foundTreeNode in rootTreeNode.Children)
				{
					if(foundTreeNode.IsDirectory)
					{
						foundDirectoryTreeNodes.Add(foundTreeNode);
					}
				}
				MoveFileFragment moveFileFragment = new MoveFileFragment(foundDirectoryTreeNodes, treeNodeToMove, this);
				dialogFragmentMoveFile = moveFileFragment;

				HideProgressDialog ();

				if (foundDirectoryTreeNodes.Count > 0) {
					dialogFragmentMoveFile.Show (fragmentTransaction, "movefiledialog");
				} else {
					ShowToast("Geen mappen gevonden om bestand naar te verplaatsen");
				}
			} catch {
				HideProgressDialog ();
				ShowToast ("Er is iets fout gegaan bij het ophalen van mappen. \nProbeer het a.u.b. opnieuw");
			}
		}
			
		public void HideMoveFileDialog ()
		{
			dialogFragmentMoveFile.Dismiss ();
			ShowToast("Bestand succesvol verplaatst");
			RefreshExplorerFragmentData ();
		}


		//Used for file share datepicker
		protected override Dialog OnCreateDialog (int id)
		{
			HideProgressDialog ();

			selectedExpirationDate = DateTime.Now.AddDays(7);//Selectie standaard op 7 dagen na vandaag
			var picker = new DatePickerDialog (this, HandleDateSet, selectedExpirationDate.Year, selectedExpirationDate.Month - 1, selectedExpirationDate.Day);
			picker.SetTitle ("Selecteer vervaldatum:");
			return picker;
		}

		//Used for file share datepicker
		async void HandleDateSet (object sender, DatePickerDialog.DateSetEventArgs e)
		{
			selectedExpirationDate = e.Date;

			if (selectedExpirationDate.Date <= DateTime.Now.Date) {
				HideProgressDialog ();
				Toast.MakeText (Android.App.Application.Context, "Gekozen vervaldatum moet na vandaag zijn. Probeer het a.u.b. opnieuw.", ToastLength.Long).Show ();
			} else {
				PublicUrl createdPublicUrl = await DataLayer.Instance.CreatePublicFileShare (pathToNewFileShare, selectedExpirationDate);

				if (createdPublicUrl != null) {
					//Open e-mail intent
					var emailIntent = new Intent (Android.Content.Intent.ActionSend);

					string bodyText = "Mijn gedeelde bestand: \n" +
						createdPublicUrl.publicUri + "\n \n" +
						"Deze link is geldig tot: " + selectedExpirationDate.ToString ("dd-MM-yyyy");

					emailIntent.PutExtra (Android.Content.Intent.ExtraSubject, "Publieke URL naar gedeeld LocalBox bestand");
					emailIntent.PutExtra (Android.Content.Intent.ExtraText, bodyText);
					emailIntent.SetType ("message/rfc822");

					HideProgressDialog ();

					StartActivity (emailIntent);
				} else {
					HideProgressDialog ();
					Toast.MakeText (Android.App.Application.Context, "Bestand delen mislukt", ToastLength.Short).Show ();
				}
			}
		}




		////////////////////////////////////////////////////////////////////////////////////////////////////
		///////Below you can find methods to register a new LocalBox 
		////////////////////////////////////////////////////////////////////////////////////////////////////

		//Register new LocalBox part 1
		private void ShowOpenUrlDialog ()
		{
			LayoutInflater factory = LayoutInflateHelper.GetLayoutInflater (this);
			View viewNewFolder = factory.Inflate (Resource.Layout.dialog_open_url, null);

			EditText editTextUrl = (EditText)viewNewFolder.FindViewById<EditText> (Resource.Id.editText_dialog_open_url);          

			//Build the dialog
			var dialogBuilder = new AlertDialog.Builder (this);
			dialogBuilder.SetTitle ("Nieuwe LocalBox");
			dialogBuilder.SetView (viewNewFolder);

			dialogBuilder.SetPositiveButton ("Open URL", (EventHandler<DialogClickEventArgs>)null);
			dialogBuilder.SetNegativeButton (Resource.String.cancel, (EventHandler<DialogClickEventArgs>)null);

			var dialog = dialogBuilder.Create ();
			dialog.Show ();

			var buttonOpenUrl = dialog.GetButton ((int)DialogButtonType.Positive);
			var buttonCancel = dialog.GetButton ((int)DialogButtonType.Negative);

			editTextUrl.Text = "https://localbox.bigwobber.nl";

			buttonOpenUrl.Click +=(sender, args) => {
				if (String.IsNullOrEmpty (editTextUrl.Text)) {
					ShowToast("URL is niet ingevuld");
				} else {
					EnterCredential(editTextUrl.Text);
					dialog.Dismiss();
				}
			};
			buttonCancel.Click += (sender, args) => {
				dialog.Dismiss ();
			};
		}

		//Register new LocalBox part 2
		public void EnterCredential (string urlToNewLocalBox)
		{
			LayoutInflater factory = LayoutInflateHelper.GetLayoutInflater (this);
			View viewRegisterLocalBox = factory.Inflate (Resource.Layout.dialog_register_localbox, null);

			EditText editTextUsername = (EditText)viewRegisterLocalBox.FindViewById<EditText> (Resource.Id.editText_dialog_register_username);          
			EditText editTextPassword = (EditText)viewRegisterLocalBox.FindViewById<EditText> (Resource.Id.editText_dialog_register_password); 

			// Build the dialog.
			var dialogBuilder = new AlertDialog.Builder (this);
			dialogBuilder.SetTitle ("Inloggegevens voor betreffende LocalBox");
			dialogBuilder.SetView (viewRegisterLocalBox);

			dialogBuilder.SetPositiveButton (Resource.String.login, (EventHandler<DialogClickEventArgs>)null);
			dialogBuilder.SetNegativeButton (Resource.String.cancel, (EventHandler<DialogClickEventArgs>)null);

			var dialog = dialogBuilder.Create ();
			dialog.Show ();

			// Get the buttons
			var buttonLogin = dialog.GetButton ((int)DialogButtonType.Positive);
			var buttonCancelRegistration = dialog.GetButton ((int)DialogButtonType.Negative);

			buttonLogin.Click += (sender, args) => {
				if (string.IsNullOrEmpty (editTextUsername.Text)) {
					ShowToast("Gebruikersnaam is niet ingevuld");
				} else if (string.IsNullOrEmpty (editTextPassword.Text)) {
					ShowToast("Wachtwoord is niet ingevuld");
				} else {
					ShowProgressDialog("Laden...");

					try{
						HideProgressDialog();
						ShowRegisterLocalBoxDialog(urlToNewLocalBox, editTextUsername.Text, editTextPassword.Text);
						dialog.Dismiss();
					}
					catch{
						HideProgressDialog();
						ShowToast("Openen van opgegeven URL is mislukt.");
					}
				}
			};
			buttonCancelRegistration.Click += (sender, args) => {
				//DataLayer.Instance.DeleteLocalBox (localBox.Id);
				menuFragment.UpdateLocalBoxes ();
				dialog.Dismiss ();
			};
		}
			
		//Register new LocalBox part 3
		private void ShowRegisterLocalBoxDialog (string urlToOpen, string enteredUsername, string enteredPassword)
		{
			if (urlToOpen.IndexOf("register_app", StringComparison.OrdinalIgnoreCase) < 0) //url misses '/register_app' at the end
			{
				if (urlToOpen.EndsWith ("/")) {	//input url is like "www.mylocalbox.nl/"
					urlToOpen = urlToOpen + "register_app";
				} 
				else {	//input url is like "www.mylocalbox.nl"
					urlToOpen = urlToOpen + "/register_app";
				}
			}
			Android.App.FragmentTransaction fragmentTransaction;
			fragmentTransaction = FragmentManager.BeginTransaction ();

			Android.App.DialogFragment dialogFragmentRegisterLocalBox;
			RegisterLocalBoxFragment registerLocalBoxFragment = new RegisterLocalBoxFragment (urlToOpen, enteredUsername, enteredPassword);

			dialogFragmentRegisterLocalBox = registerLocalBoxFragment;
			dialogFragmentRegisterLocalBox.Show (fragmentTransaction, "registerlocalboxdialog");
		}
			
		//Register new LocalBox part 4
		public async void RegisterLocalBox (LocalBox localBox, string enteredUsername, string enteredPassword)
		{
			ShowProgressDialog ("LocalBox laden...");

			localBox.User = enteredUsername;
			bool authenticateSucceeded = await BusinessLayer.Instance.Authenticate (localBox, enteredPassword);

			HideProgressDialog ();
			if (!authenticateSucceeded) {
				//ShowToast("Inloggegevens zijn foutief");
				ReEnterCredential (localBox);
			} 
			else {
				//ShowToast("Inloggegevens geaccepteerd");
				if (localBox.HasCryptoKeys && !localBox.HasPassPhrase) {
					EnterPassphrase (localBox);
				} else {
					SetUpPassphrase (localBox);
				}
			}
		}
			
		//Register new LocalBox part 4
		public void ReEnterCredential(LocalBox localBox)
		{
			LayoutInflater factory = LayoutInflateHelper.GetLayoutInflater (this);
			View viewRegisterLocalBox = factory.Inflate (Resource.Layout.dialog_register_localbox, null);

			EditText editTextUsername = (EditText)viewRegisterLocalBox.FindViewById<EditText> (Resource.Id.editText_dialog_register_username);          
			EditText editTextPassword = (EditText)viewRegisterLocalBox.FindViewById<EditText> (Resource.Id.editText_dialog_register_password); 
			editTextUsername.Text = localBox.User;

			// Build the dialog.
			var dialogBuilder = new AlertDialog.Builder (this);
			dialogBuilder.SetTitle ("Registreren");
			dialogBuilder.SetView (viewRegisterLocalBox);

			dialogBuilder.SetPositiveButton (Resource.String.add, (EventHandler<DialogClickEventArgs>)null);
			dialogBuilder.SetNegativeButton (Resource.String.cancel, (EventHandler<DialogClickEventArgs>)null);

			var dialog = dialogBuilder.Create ();
			dialog.Show ();

			// Get the buttons.
			var buttonRegister = dialog.GetButton ((int)DialogButtonType.Positive);
			var buttonCancelRegistration = dialog.GetButton ((int)DialogButtonType.Negative);

			buttonRegister.Click += async (sender, args) => {
				if (String.IsNullOrEmpty (editTextUsername.Text)) {
					ShowToast("Gebruikersnaam is niet ingevuld");
				} else if (String.IsNullOrEmpty (editTextPassword.Text)) {
					ShowToast("Wachtwoord is niet ingevuld");
				} else {
					try {
						ShowProgressDialog ("Uw inloggegevens controleren. Een ogenblik geduld a.u.b.");

						localBox.User = editTextUsername.Text;
						bool authenticateSucceeded = await BusinessLayer.Instance.Authenticate (localBox, editTextPassword.Text);

						HideProgressDialog ();
						if (!authenticateSucceeded) {
							ShowToast("Inloggegevens zijn foutief");
						} else {
							dialog.Dismiss ();
							ShowToast("Inloggegevens geaccepteerd");

							if (localBox.HasCryptoKeys && !localBox.HasPassPhrase) {
								EnterPassphrase (localBox);
							} else {
								SetUpPassphrase (localBox);
							}
						}
					} catch (Exception ex) {
						HideProgressDialog ();
						ShowToast("LocalBox registratie mislukt. Probeer het a.u.b. opnieuw");
						Log.Info ("STACKTRACE", ex.StackTrace);
						Log.Info ("MESSAGE", ex.Message);
					}
				}
			};
			buttonCancelRegistration.Click += (sender, args) => {
				DataLayer.Instance.DeleteLocalBox (localBox.Id);
				menuFragment.UpdateLocalBoxes ();
				dialog.Dismiss ();
			};
		}


		//Register new LocalBox part 5
		private void SetUpPassphrase (LocalBox localBox)
		{
			LayoutInflater factory = LayoutInflateHelper.GetLayoutInflater (this);
			View viewNewPhrase = factory.Inflate (Resource.Layout.dialog_new_passphrase, null);

			EditText editNewPassphrase = (EditText)viewNewPhrase.FindViewById<EditText> (Resource.Id.editText_dialog_new_passphrase);          
			EditText editNewPassphraseVerify = (EditText)viewNewPhrase.FindViewById<EditText> (Resource.Id.editText_dialog_new_passphrase_verify);

			// Build the dialog.
			var dialogBuilder = new AlertDialog.Builder (this);
			dialogBuilder.SetTitle ("Passphrase");
			dialogBuilder.SetView (viewNewPhrase);

			dialogBuilder.SetPositiveButton ("OK", (EventHandler<DialogClickEventArgs>)null);
			dialogBuilder.SetNegativeButton (Resource.String.cancel, (EventHandler<DialogClickEventArgs>)null);

			var dialog = dialogBuilder.Create ();
			dialog.Show ();

			// Get the buttons
			var buttonAddPassphrase = dialog.GetButton ((int)DialogButtonType.Positive);
			var buttonCancel = dialog.GetButton ((int)DialogButtonType.Negative);

			buttonAddPassphrase.Click += async (sender, args) => {
				string passphraseOne = editNewPassphrase.Text;
				string passphraseTwo = editNewPassphraseVerify.Text;

				if (String.IsNullOrEmpty (passphraseOne)) {
					ShowToast("Passphrase is niet ingevuld");
				} else if (String.IsNullOrEmpty (passphraseTwo)) {
					ShowToast("U dient uw ingevoerde passphrase te verifieren");
				} else {
					if (!passphraseOne.Equals (passphraseTwo)) {
						ShowToast("De ingevoerde passphrases komen niet overeen. Corrigeer dit a.u.b.");
					} else {
						try 
						{
							ShowProgressDialog ("Passphrase aanmaken. Dit kan enige tijd in beslag nemen. Een ogenblik geduld a.u.b.");
							bool newPassphraseSucceeded = await BusinessLayer.Instance.SetPublicAndPrivateKey (localBox, passphraseOne);
							HideProgressDialog();

							if (!newPassphraseSucceeded) {
								ShowToast("Passphrase instellen mislukt. Probeer het a.u.b. opnieuw");
							} 
							else {
								dialog.Dismiss ();
								ShowToast("LocalBox succesvol geregistreerd");

								menuFragment.UpdateLocalBoxes ();
								SplashActivity.intentData = null;
							}
						} 
						catch {
							HideProgressDialog ();
							ShowToast("Passphrase instellen mislukt. Probeer het a.u.b. opnieuw");
						}
					}
				}
			};
			buttonCancel.Click += (sender, args) => {
				DataLayer.Instance.DeleteLocalBox (localBox.Id);
				menuFragment.UpdateLocalBoxes ();
				dialog.Dismiss ();
			};
		}


		//Register new LocalBox part 5
		private void EnterPassphrase (LocalBox localBox)
		{
			LayoutInflater factory = LayoutInflateHelper.GetLayoutInflater (this);
			View viewNewPhrase = factory.Inflate (Resource.Layout.dialog_enter_passphrase, null);
			EditText editEnterPassphrase = (EditText)viewNewPhrase.FindViewById<EditText> (Resource.Id.editText_dialog_enter_passphrase);          

			// Build the dialog.
			var dialogBuilder = new AlertDialog.Builder (this);
			dialogBuilder.SetTitle ("Passphrase");
			dialogBuilder.SetView (viewNewPhrase);
			dialogBuilder.SetPositiveButton ("OK", (EventHandler<DialogClickEventArgs>)null);
			dialogBuilder.SetNegativeButton (Resource.String.cancel, (EventHandler<DialogClickEventArgs>)null);

			var dialog = dialogBuilder.Create ();
			dialog.Show ();

			// Get the buttons.
			var buttonAddPassphrase = dialog.GetButton ((int)DialogButtonType.Positive);
			var buttonCancel = dialog.GetButton ((int)DialogButtonType.Negative);

			buttonAddPassphrase.Click += async (sender, args) => {
				string passphrase = editEnterPassphrase.Text;

				if (String.IsNullOrEmpty (passphrase)) {
					ShowToast("Passphrase is niet ingevuld");
				} 
				else {
					try {
						ShowProgressDialog ("Uw passphrase controleren. Een ogenblik geduld a.u.b.");
						bool correctPassphraseEntered = await BusinessLayer.Instance.ValidatePassPhrase (localBox, passphrase);
						HideProgressDialog();

						if (!correctPassphraseEntered) {
							ShowToast("Passphrase onjuist. Probeer het a.u.b. opnieuw");
						} else {
							dialog.Dismiss ();

							ShowToast("Passphrase geaccepteerd en LocalBox succesvol geregistreerd");

							menuFragment.UpdateLocalBoxes ();

							SplashActivity.intentData = null;
						}
					} catch {
						HideProgressDialog ();
						ShowToast("Passphrase verifieren mislukt. Probeer het a.u.b. opnieuw");
					}
				}
			};
			buttonCancel.Click += (sender, args) => {
				DataLayer.Instance.DeleteLocalBox (localBox.Id);
				menuFragment.UpdateLocalBoxes ();
				dialog.Dismiss ();
			};
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

		private void ShowToast(string message)
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
