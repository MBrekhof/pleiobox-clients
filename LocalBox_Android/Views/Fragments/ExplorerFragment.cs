using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Graphics.Drawables;
using Android.Webkit;
using Android.Preferences;
using Android.Net;

using LocalBox_Common;
using LocalBox_Common.Remote;
using Xamarin;

namespace LocalBox_Droid
{
	public class ExplorerFragment : ListFragment
	{
		public static List<string> openedDirectories;
		public static bool openedFolderIsUnencrypted = true;
		private static bool openedFolderIsShare = false;
		public string currentTreeNodePath;
		public bool favoriteFolderOpened;
		private List<TreeNode> foundTreeNodeChildren;
		private int lastShownTreeNodeId;
		private HomeActivity parentActivity;
		private PopupWindow popupWindow;
		private ListView explorerListView;
		private ImageButton buttonUploadFileExplorer;
		private ExplorerAdapter explorerAdapter;

		//Default constructor for explorer
		public ExplorerFragment (TreeNode foundTreeNode, HomeActivity homeActivity)
		{
			foundTreeNodeChildren = foundTreeNode.Children;
			this.parentActivity = homeActivity;

			//Add favorite list item in root listview of local box
			if (openedDirectories.Count == 0) {
				foundTreeNodeChildren.Insert (0, new TreeNode (){ Name = "Lokale favorieten", Type = "favorite" });
			}

			this.currentTreeNodePath = foundTreeNode.Path;
			favoriteFolderOpened = false;
		}

		//Favorite constructor for explorer
		public ExplorerFragment (List<TreeNode> foundTreeNodes, HomeActivity homeActivity)
		{
			this.parentActivity = homeActivity;
			foundTreeNodeChildren = foundTreeNodes;
			favoriteFolderOpened = true;
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
		}

		public override View OnCreateView (LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle)
		{
			View view = layoutInflater.Inflate (Resource.Layout.fragment_explorer, viewGroup, false);

			explorerAdapter = new ExplorerAdapter (Activity, foundTreeNodeChildren, false);
			ListAdapter = explorerAdapter;

			return view;
		}

		public override void OnViewCreated (View view, Bundle savedInstanceState)
		{
			base.OnViewCreated (view, savedInstanceState);

			explorerListView = ListView;

			ListView.ItemClick += ListView_OnItemClick;
			ListView.ItemLongClick += ListView_OnItemLongClick;

			buttonUploadFileExplorer = Activity.FindViewById<ImageButton> (Resource.Id.button_upload_file_explorer);

			if (openedDirectories.Count > 1) {
				if (favoriteFolderOpened == false) {
					buttonUploadFileExplorer.Visibility = ViewStates.Visible;
				}
			} else {
				buttonUploadFileExplorer.Visibility = ViewStates.Invisible;
			}
		}


		public override void OnStop ()
		{
			parentActivity.HideProgressDialog ();
			base.OnStop ();
		}


		async void ListView_OnItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			try {
				TreeNode clickedItem = foundTreeNodeChildren [e.Position];

				//Hide bottom menu for favorite file

				if (favoriteFolderOpened) {
					parentActivity.HideBottomExplorerMenuItems ();
				} else {
					parentActivity.ShowBottomExplorerMenuItems ();
				}

				//Show progress dialog while loading
				parentActivity.ShowProgressDialog (null);


				if (clickedItem.IsDirectory == true)  //Folder aangeklikt (NIET "Lokale favorieten")
				{
					Android.Support.V4.App.FragmentTransaction fragmentTransaction = Activity.SupportFragmentManager.BeginTransaction ();
					fragmentTransaction.SetCustomAnimations (Resource.Animation.enter, Resource.Animation.exit);

					ExplorerFragment explorerFragment = new ExplorerFragment (await DataLayer.Instance.GetFolder (clickedItem.Path), parentActivity);
					HomeActivity.openedExplorerFragments.Add(explorerFragment);
					fragmentTransaction.Replace (Resource.Id.fragment_container_explorer, explorerFragment, "explorerFragment");

					//Add new directory to opened directories list
					ExplorerFragment.openedDirectories.Add (clickedItem.Path);

					//Used to determine custom context menu
					if(ExplorerFragment.openedDirectories.Count == 2){
						if(clickedItem.IsShare){
							openedFolderIsShare = true;
						}else{
							openedFolderIsShare = false;
						}
					}

					//Used to determine upload action and icon in listview item (encrypted or unencrypted folder)
					if(ExplorerFragment.openedDirectories.Count == 2){
						if(clickedItem.HasKeys){
							openedFolderIsUnencrypted = false;
						}else{
							openedFolderIsUnencrypted = true;
						}
					}
						
					//Add fragment to stack - needed for back button functionality
					fragmentTransaction.AddToBackStack (null);

					// Start the animated transition.
					fragmentTransaction.Commit ();

					//Show hidden buttons
					ImageButton buttonBackExplorer = Activity.FindViewById<ImageButton> (Resource.Id.button_back_explorer);
					buttonBackExplorer.Visibility = ViewStates.Visible;
				} 
				else if (clickedItem.Type.Equals ("favorite"))  //List item "Lokale favorieten" aangeklikt
				{
					//Hide bottom menu for favorite folder
					parentActivity.HideBottomExplorerMenuItems ();

					Android.Support.V4.App.FragmentTransaction fragmentTransaction = Activity.SupportFragmentManager.BeginTransaction ();
					fragmentTransaction.SetCustomAnimations (Resource.Animation.enter, Resource.Animation.exit);

					List<TreeNode> favorites = DataLayer.Instance.GetFavorites ();

					ExplorerFragment explorerFragment = new ExplorerFragment (favorites, parentActivity);
					HomeActivity.openedExplorerFragments.Add(explorerFragment);
					fragmentTransaction.Replace (Resource.Id.fragment_container_explorer, explorerFragment, "explorerFragment");

					//Add new directory to opened directories list
					ExplorerFragment.openedDirectories.Add (clickedItem.Path);

					//Add fragment to stack - needed for back button functionality
					fragmentTransaction.AddToBackStack (null);

					// Start the animated transition.
					fragmentTransaction.Commit ();

					//Show hidden buttons
					ImageButton buttonBackExplorer = Activity.FindViewById<ImageButton> (Resource.Id.button_back_explorer);
					buttonBackExplorer.Visibility = ViewStates.Visible;

				} 
				else { 	//Bestand aangeklikt
						//Afbeeldingen openen in webview - andere typen bestanden openen middels content provider

					string mimeTypeOfClickedItem = MimeTypeHelper.GetMimeType(clickedItem.Path);
					clickedItem.Type = mimeTypeOfClickedItem;

					if (mimeTypeOfClickedItem.Equals ("image/jpeg") ||
						mimeTypeOfClickedItem.Equals ("image/png")) {

						lastShownTreeNodeId = clickedItem.Id;

						Android.Support.V4.App.FragmentTransaction fragmentTransaction = Activity.SupportFragmentManager.BeginTransaction ();
						fragmentTransaction.SetCustomAnimations (Resource.Animation.enter, Resource.Animation.exit);

						DocumentFragment documentFragment = new DocumentFragment (await DataLayer.Instance.GetFilePath (clickedItem.Path), clickedItem.Name);
						fragmentTransaction.Replace (Resource.Id.fragment_container_document, documentFragment, "documentFragment");

						// Start the animated transition.
						fragmentTransaction.Commit ();
					}
					else if(mimeTypeOfClickedItem.Equals("video/mp4"))
					{
						var videoPlayerActivity = new Intent (Activity, typeof(VideoPlayerActivity));
						string pathToVideo = await DataLayer.Instance.GetFilePath (clickedItem.Path);
						videoPlayerActivity.PutExtra ("PathToVideo", pathToVideo);

						StartActivity (videoPlayerActivity);
					}

					//Disabled - reason: no license for PDFTron
					/*else if(mimeTypeOfClickedItem.Equals("application/pdf")){

						var pdfIntent = new Intent (Activity, typeof(PdfActivity));

						string absolutePathOfPDF = await DataLayer.Instance.GetFilePath (clickedItem.Path);
						pdfIntent.PutExtra ("absolutePathOfPDF", absolutePathOfPDF);
						pdfIntent.PutExtra ("relativePathOfPDF", clickedItem.Path);
						pdfIntent.PutExtra ("fileName", clickedItem.Name);

						StartActivity (pdfIntent);   
					}*/

					else {
						//Open bestand in andere app
						OpenFileIn (clickedItem);
					}
				}
			} catch (Exception ex){
				Insights.Report(ex);
				parentActivity.HideProgressDialog ();
				Toast.MakeText (Activity, "Het openen is mislukt. Probeer het a.u.b. opnieuw", ToastLength.Short).Show ();
			}
			parentActivity.HideProgressDialog ();
		}


		void ListView_OnItemLongClick (object sender, AdapterView.ItemLongClickEventArgs e)
		{
			LayoutInflater inflater = (LayoutInflater)Activity.GetSystemService (Context.LayoutInflaterService); 
			View popupView;

			TreeNode selectedItem = foundTreeNodeChildren [e.Position];

			int numberOfDirectoriesOpened = ExplorerFragment.openedDirectories.Count;
			string currentDirectoryName = ExplorerFragment.openedDirectories [numberOfDirectoriesOpened - 1];

			if (selectedItem.IsDirectory == true) { //Directory clicked - so open folder popup layout
				if (currentDirectoryName.Equals ("/")) {

					if (selectedItem.IsShared || selectedItem.IsShare == false) { //Andermans share
						popupView = inflater.Inflate (Resource.Layout.custom_popup_folder_root, null);
						ClickHandlersFolderRootPopupMenu (popupView, selectedItem);
					} 
					else {//Eigen share
						popupView = inflater.Inflate (Resource.Layout.custom_popup_folder_subfolder, null);
						ClickHandlersFolderSubFolderPopupMenu (popupView, selectedItem);
					}
						
				} else { //Subfolder
					popupView = inflater.Inflate (Resource.Layout.custom_popup_folder_subfolder, null);
					ClickHandlersFolderSubFolderPopupMenu (popupView, selectedItem);
				}
			} 
			else 
			{  	//File clicked - so open file popup layout
				if (openedFolderIsShare || favoriteFolderOpened) {
					popupView = inflater.Inflate (Resource.Layout.custom_popup_file_share, null);
				} 
				else if (openedFolderIsUnencrypted) {
					popupView = inflater.Inflate (Resource.Layout.custom_popup_file_unencrypted, null);
				}
				else {
					popupView = inflater.Inflate (Resource.Layout.custom_popup_file_encrypted, null);
				}
				ClickHandlersFilePopupMenu (popupView, selectedItem);
			}

			popupWindow = new PopupWindow (popupView, e.View.Width, e.View.Height);

			//Hide popup window when clicking outside its view
			popupWindow.Focusable = true;
			popupWindow.Update ();
			popupWindow.SetBackgroundDrawable (new BitmapDrawable ());

			//Menu alleen tonen als het niet list item "Lokale favorieten" betreft
			if (selectedItem.Type == null) {//Directory selected so show menu (alleen als het niet een share van een ander is)

				if (currentDirectoryName.Equals ("/")) {
					if (!selectedItem.IsShare) {
						popupWindow.ShowAsDropDown (e.View, 0, -e.View.Height);
					}
				} else {
					if (openedFolderIsShare == false) { 
						popupWindow.ShowAsDropDown (e.View, 0, -e.View.Height);
					}
				}
			}
			else if (!selectedItem.Type.Equals ("favorite")) {//File selected so show menu
				popupWindow.ShowAsDropDown (e.View, 0, -e.View.Height);
			}
		}

		private void ClickHandlersFilePopupMenu (View popupView, TreeNode selectedItem)
		{
			ImageButton buttonFileOpenIn = (ImageButton)popupView.FindViewById (Resource.Id.button_popup_file_openin);
			buttonFileOpenIn.Click += delegate {
				OpenFileIn (selectedItem);
			};

			ImageButton buttonFavoriteFile = (ImageButton)popupView.FindViewById (Resource.Id.button_popup_file_favorite);
			buttonFavoriteFile.Click += delegate {

				if(selectedItem.IsFavorite){
					UnFavoriteFile (selectedItem);
				}
				else{
					FavoriteFile (selectedItem);
				}
			};

			if (!openedFolderIsShare && !favoriteFolderOpened) {

				ImageButton buttonMoveFile = (ImageButton)popupView.FindViewById (Resource.Id.button_popup_file_move);
				buttonMoveFile.Click += delegate {
					popupWindow.Dismiss();
					parentActivity.ShowMoveFileDialog(selectedItem);
				};
					
				ImageButton buttonDeleteFile = (ImageButton)popupView.FindViewById (Resource.Id.button_popup_file_delete);
				buttonDeleteFile.Click += delegate {
					if (selectedItem.Id == lastShownTreeNodeId) {//Document is geopend in document fragment, dus deze resetten
						parentActivity.ClearContentInDocumentFragment ();
					}
					DeleteFolderOrFile (selectedItem.Path);
				};

				if (openedFolderIsUnencrypted) {
					ImageButton buttonPublicShareFile = (ImageButton)popupView.FindViewById (Resource.Id.button_popup_file_share);
					buttonPublicShareFile.Click += delegate {
						CreatePublicFileShare (selectedItem.Path);
					};
				}
			}
		}

		private void ClickHandlersFolderRootPopupMenu (View popupView, TreeNode selectedItem)
		{
			ImageButton buttonShareFolder = (ImageButton)popupView.FindViewById (Resource.Id.button_popup_folder_root_share);
			buttonShareFolder.Click += delegate {
				popupWindow.Dismiss ();
				parentActivity.ShowShareDialog(selectedItem.Path, selectedItem.IsShared);
			};

			ImageButton buttonDeleteFolder = (ImageButton)popupView.FindViewById (Resource.Id.button_popup_folder_root_delete);
			buttonDeleteFolder.Click += delegate {
				DeleteFolderOrFile (selectedItem.Path);
			};
		}

		private  void ClickHandlersFolderSubFolderPopupMenu (View popupView, TreeNode selectedItem)
		{
			ImageButton buttonDeleteFolder = (ImageButton)popupView.FindViewById (Resource.Id.button_popup_folder_subfolder_delete);
			buttonDeleteFolder.Click += delegate {
				DeleteFolderOrFile (selectedItem.Path);
			};
		}

		public async void RefreshData ()
		{
			parentActivity.HideProgressDialog ();
			parentActivity.ShowProgressDialog(null);

			try {
				//Get current opened directory name

				int numberOfDirectoriesOpened = ExplorerFragment.openedDirectories.Count;
				if(numberOfDirectoriesOpened >= 1){

					string currentDirectoryName = ExplorerFragment.openedDirectories [numberOfDirectoriesOpened - 1];
					
					foundTreeNodeChildren = new List<TreeNode> ();
					foundTreeNodeChildren = (await DataLayer.Instance.GetFolder (currentDirectoryName, true)).Children;

					parentActivity.RunOnUiThread(new Action(()=> {

						//Add favorite list item in root listview of local box
						if (openedDirectories.Count == 1) {
							foundTreeNodeChildren.Insert (0, new TreeNode (){ Name = "Lokale favorieten", Type = "favorite" });
						}

						explorerAdapter = new ExplorerAdapter (Activity, foundTreeNodeChildren, false);
						ListAdapter = explorerAdapter;

						//Update UI
						explorerAdapter.NotifyDataSetChanged();
						explorerListView.InvalidateViews();
					}));
				}
				parentActivity.HideProgressDialog();
			} 
			catch (Exception ex){
				Insights.Report(ex);
				Console.WriteLine (ex.Message);
				Console.WriteLine (ex.StackTrace);

				parentActivity.HideProgressDialog ();
				if (!SslValidator.CertificateErrorRaised) {
					Toast.MakeText (Android.App.Application.Context, "Data verversen mislukt. Probeer het a.u.b. opnieuw", ToastLength.Short).Show ();
				}
			}
		}

		private async void OpenFileIn (TreeNode clickedItem)
		{
			try {
	
				if (popupWindow != null) {
					popupWindow.Dismiss ();
				}

				Intent intent = new Intent (Intent.ActionView);

				string mimeTypeOfClickedItem = MimeTypeHelper.GetMimeType (clickedItem.Path);
				clickedItem.Type = mimeTypeOfClickedItem;

				if (clickedItem.Type.Equals ("application/pdf")) {

					new Thread (new ThreadStart (async delegate {

						//Show progress dialog while loading
						parentActivity.HideProgressDialog ();
						parentActivity.ShowProgressDialog (null);
						string fullFilePath = await DataLayer.Instance.GetFilePath (clickedItem.Path);

						//Controleer internet verbinding
						var connectivityManager = (ConnectivityManager)Android.App.Application.Context.GetSystemService 
											  (Context.ConnectivityService);
						var activeConnection = connectivityManager.ActiveNetworkInfo;

						if ((activeConnection != null) && activeConnection.IsConnected) {  	//Internet verbinding gedetecteerd

							//Create temp file
							string temporaryFilePath = System.IO.Path.Combine ("/storage/emulated/0/Download", clickedItem.Name);

							if (File.Exists (temporaryFilePath)) {
								File.Delete (temporaryFilePath);
							}
							
							//Save settings of last opened file
							ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences (Activity);
							ISharedPreferencesEditor editor = prefs.Edit ();
							editor.PutString ("fileNameLastOpenedPdf", clickedItem.Name);
							editor.PutString ("pathLastOpenedPdf", clickedItem.Path);
							editor.PutString ("temporaryFilePath", temporaryFilePath);
							editor.PutBoolean ("isFavorite", clickedItem.IsFavorite);
							editor.Apply ();

							//Save temporary file in filesystem
							RemoteExplorer remoteExplorer = new RemoteExplorer ();
							Byte[] fileBytes = remoteExplorer.GetFile (clickedItem.Path);

							File.WriteAllBytes (temporaryFilePath, fileBytes);

							Android.Net.Uri uri = Android.Net.Uri.Parse ("file://" + temporaryFilePath);
							intent.SetDataAndType (uri, clickedItem.Type);

							parentActivity.HideProgressDialog ();
							Activity.StartActivity (intent);
						} else {	
							//Geen internet verbinding
							
							var alertDialogConfirmDelete = new Android.App.AlertDialog.Builder (Activity);
							alertDialogConfirmDelete.SetTitle ("Geen verbinding");
							alertDialogConfirmDelete.SetMessage ("U heeft momenteel geen internet verbinding. Het maken van PDF annotaties is daarom niet mogelijk.");

							alertDialogConfirmDelete.SetPositiveButton ("OK", async delegate { 

								Android.Net.Uri uri = Android.Net.Uri.Parse ("file://" + fullFilePath);
								intent.SetDataAndType (uri, clickedItem.Type);

								parentActivity.HideProgressDialog ();
								Activity.StartActivity (intent);
							});
							alertDialogConfirmDelete.Create ().Show ();
						}
					})).Start ();
				} else {//Ander bestandstype dan PDF openen

					//Show progress dialog while loading
					parentActivity.HideProgressDialog ();
					parentActivity.ShowProgressDialog (null);
					string fullFilePath = await DataLayer.Instance.GetFilePath (clickedItem.Path);

					Android.Net.Uri uri = Android.Net.Uri.Parse (CustomContentProvider.CONTENT_URI + fullFilePath);

					intent.SetDataAndType (uri, clickedItem.Type);

					intent.SetFlags (ActivityFlags.GrantReadUriPermission);
					intent.SetFlags (ActivityFlags.NewTask);
					intent.SetFlags (ActivityFlags.ClearWhenTaskReset);

					Activity.StartActivity (intent);
				}
			} catch (Exception ex){
				Insights.Report(ex);
				Console.WriteLine (ex.Message);

				parentActivity.HideProgressDialog ();

				if (ex is ActivityNotFoundException) {
					Toast.MakeText (Android.App.Application.Context, "Geen app op uw apparaat gevonden om dit bestandstype te kunnen openen", ToastLength.Long).Show ();
				} else {
					Toast.MakeText (Android.App.Application.Context, "Openen bestand mislukt", ToastLength.Long).Show ();
				}
			}
		}



		private void CreatePublicFileShare (string filePath)
		{
			try {
				popupWindow.Dismiss ();

				parentActivity.ShowProgressDialog (null);
				parentActivity.ShowShareFileDatePicker(filePath);
			} 
			catch (Exception ex){
				Insights.Report(ex);
				parentActivity.HideProgressDialog ();
				Toast.MakeText (Android.App.Application.Context, "Bestand delen mislukt", ToastLength.Short).Show ();
			}
		}









		private async void FavoriteFile (TreeNode selectedItem)
		{
			try {
				popupWindow.Dismiss ();

				parentActivity.ShowProgressDialog ("Bestand toevoegen aan favorieten");

				bool favoriteSucceed = await DataLayer.Instance.FavoriteFile (selectedItem);
				if (!favoriteSucceed) {
					Toast.MakeText (Android.App.Application.Context, "Favoriet maken mislukt", ToastLength.Short).Show ();
				} else {
					Toast.MakeText (Android.App.Application.Context, "Bestand succesvol favoriet gemaakt", ToastLength.Short).Show ();
				}
				parentActivity.HideProgressDialog ();

				//Refresh listview
				RefreshData ();
			} catch (Exception ex){
				Insights.Report(ex);
				parentActivity.HideProgressDialog ();
				Toast.MakeText (Android.App.Application.Context, "Favoriet maken mislukt", ToastLength.Short).Show ();
			}
		}

		private async void UnFavoriteFile (TreeNode selectedItem)
		{
			try {
				popupWindow.Dismiss ();

				parentActivity.ShowProgressDialog ("Bestanden verwijderen uit favorieten");

				bool unFavoriteSucceed = await DataLayer.Instance.UnFavoriteFile (selectedItem);
				if (!unFavoriteSucceed) {
					Toast.MakeText (Android.App.Application.Context, "Mislukt om bestand uit favorieten te verwijderen", ToastLength.Short).Show ();
				} else {
					Toast.MakeText (Android.App.Application.Context, "Bestand succesvol uit favorieten verwijderd", ToastLength.Short).Show ();
				}
				parentActivity.HideProgressDialog ();

				//Refresh listview
				int numberOfDirectoriesOpened = ExplorerFragment.openedDirectories.Count;
				if(numberOfDirectoriesOpened >= 1){ 

					if(ExplorerFragment.openedDirectories [numberOfDirectoriesOpened - 1] != null){ //Favorite folder niet geopend
						RefreshData();
					}else{
						//Favorite folder opened so refresh favorite list
						List<TreeNode> favorites = DataLayer.Instance.GetFavorites ();

						explorerAdapter = new ExplorerAdapter (Activity, favorites, false);
						ListAdapter = explorerAdapter;

						//UPDATE UI
						Activity.RunOnUiThread(new Action(()=> { 
							explorerAdapter.NotifyDataSetChanged();
							explorerListView.InvalidateViews();
						}));
					}
				}
				else
				{
					RefreshData();
				}
			} catch (Exception ex){
				Insights.Report(ex);
				parentActivity.HideProgressDialog ();
				Toast.MakeText (Android.App.Application.Context, "Mislukt om bestand uit favorieten te verwijderen", ToastLength.Short).Show ();
			}
		}

		private void DeleteFolderOrFile (string path)
		{
			popupWindow.Dismiss ();

			var alertDialogConfirmDelete = new Android.App.AlertDialog.Builder (Activity);
			alertDialogConfirmDelete.SetTitle("Waarschuwing");
			alertDialogConfirmDelete.SetMessage("Bent u zeker van deze verwijder actie? \nDeze actie is niet terug te draaien.");

			alertDialogConfirmDelete.SetPositiveButton ("Verwijderen", async delegate { 
				try {
					parentActivity.ShowProgressDialog ("Verwijderen... Een ogenblik geduld a.u.b.");

					bool deleteSucceed = await DataLayer.Instance.DeleteFileOrFolder (path);

					parentActivity.HideProgressDialog ();

					if (!deleteSucceed) {
						Toast.MakeText (Android.App.Application.Context, "Verwijderen mislukt - druk op ververs en probeer het a.u.b. opnieuw", ToastLength.Long).Show ();
					} else {
						Toast.MakeText (Android.App.Application.Context, "Verwijderen succesvol", ToastLength.Short).Show ();

						//Refresh listview
						RefreshData ();
					}
				} catch (Exception ex){
					Insights.Report(ex);
					parentActivity.HideProgressDialog (); 
					Toast.MakeText (Android.App.Application.Context, "Verwijderen mislukt - druk op ververs en probeer het a.u.b. opnieuw", ToastLength.Long).Show ();
				}
			});
			alertDialogConfirmDelete.SetNegativeButton ("Annuleren", delegate { 
				alertDialogConfirmDelete.Dispose();
			});
			alertDialogConfirmDelete.Create ().Show ();
		}

	}
}