using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Net;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using Android.Webkit;
using Android.Graphics.Drawables;
using Android.Graphics;

using LocalBox_Common;
using Xamarin;

namespace LocalBox_Droid
{
	public class MenuFragment : ListFragment
	{
		private List<LocalBox> foundLocalBoxes;
		private MenuAdapter menuAdapter;
		private ImageView imageViewLogo;
		private CustomProgressDialog progressDialog = new CustomProgressDialog ();

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
		}

		public override View OnCreateView (LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle)
		{
			View view = layoutInflater.Inflate (Resource.Layout.fragment_menu, viewGroup, false);

			//imageViewLogo = Activity.FindViewById<ImageView> (Resource.Id.imageViewLogo);

			SetAdapter ();
			
			return view;
		}

		private async void SetAdapter(){
            foundLocalBoxes = await DataLayer.Instance.GetLocalBoxes ();

			menuAdapter = new MenuAdapter (Activity, foundLocalBoxes);
			ListAdapter = menuAdapter;
		}


		public override void OnViewCreated (View view, Bundle savedInstanceState)
		{
			base.OnViewCreated (view, savedInstanceState);

			ListView.ItemClick 		+= ListView_OnItemClick;
			ListView.ItemLongClick 	+= ListView_OnItemLongClick;
		}


		async void ListView_OnItemClick (object sender, AdapterView.ItemClickEventArgs e)
		{
			ResetUIToBeginState(false);


			SslValidator.CertificateErrorRaised = false;

			//Set selected local box
			Waardes.Instance.GeselecteerdeBox = foundLocalBoxes[e.Position].Id;

			//Reset certificate validation check to default behavior
			/*
			ServicePointManager.ServerCertificateValidationCallback = null;

			if (foundLocalBoxes[e.Position].OriginalSslCertificate != null) { //Selected localbox does have a ssl certificate
				//Set ssl validator for selected LocalBox
				SslValidator sslValidator = new SslValidator ();
				ServicePointManager.ServerCertificateValidationCallback = sslValidator.ValidateServerCertficate;
			}*/

			//Change action bar color to color of selected localbox
			if (DataLayer.Instance.GetSelectedOrDefaultBox ().BackColor != null && 
				DataLayer.Instance.GetSelectedOrDefaultBox ().BackColor.StartsWith("#")) {
				HomeActivity.colorOfSelectedLocalBox = DataLayer.Instance.GetSelectedOrDefaultBox ().BackColor;
			} else {
				HomeActivity.colorOfSelectedLocalBox = Constants.lightblue;
			}
			SetCustomActionBarColor ();

			//Change logo image to logo of selected local box
			if (DataLayer.Instance.GetSelectedOrDefaultBox ().LogoUrl != null) {

				string logoUrl = DataLayer.Instance.GetSelectedOrDefaultBox ().LogoUrl;

				string documentsPath = DocumentConstants.DocumentsPath;
				string pathToLogo =  System.IO.Path.Combine(documentsPath, logoUrl.Substring(logoUrl.LastIndexOf("/") + 1));

				if (File.Exists (pathToLogo)) 
				{ //Verander logo
					Android.Net.Uri uriLogo = Android.Net.Uri.Parse (pathToLogo);
					//imageViewLogo.SetImageURI (uriLogo);
				} 
				else //Default logo
				{
					//imageViewLogo.SetImageResource (Resource.Drawable.beeldmerk_belastingdienst);
				}
			}

			//Update fragment data
			Android.Support.V4.App.FragmentTransaction fragmentTransaction = Activity.SupportFragmentManager.BeginTransaction ();
			fragmentTransaction.SetCustomAnimations (Resource.Animation.enter, Resource.Animation.exit);

			//Show progress dialog while loading
			ShowProgressDialog (Activity, null);

			try {
				HomeActivity homeActivity = (HomeActivity)Activity;

				ExplorerFragment explorerFragment = new ExplorerFragment (await DataLayer.Instance.GetFolder ("/"), homeActivity);

				HomeActivity.openedExplorerFragments = new List<ExplorerFragment>();
				HomeActivity.openedExplorerFragments.Add(explorerFragment);

				//Add new directory to opened directories list
				ExplorerFragment.openedDirectories = new List<string>();
				ExplorerFragment.openedDirectories.Add("/");

				fragmentTransaction.Replace (Resource.Id.fragment_container_explorer, explorerFragment, "explorerFragment");

				//Clear fragment back stack
				int entryCount = Activity.SupportFragmentManager.BackStackEntryCount; 

				while (entryCount > 0) {
					Activity.SupportFragmentManager.PopBackStackImmediate();
					entryCount = Activity.SupportFragmentManager.BackStackEntryCount; 
				}
					
				//Add fragment to stack - needed for back button functionality
				fragmentTransaction.AddToBackStack (null);

				// Start the animated transition.
				fragmentTransaction.Commit ();

				//Show hidden buttons
				RelativeLayout fragmentContainerExplorerBottom = Activity.FindViewById<RelativeLayout> (Resource.Id.fragment_container_explorer_blank);
				fragmentContainerExplorerBottom.Visibility = ViewStates.Visible;

				//Show shadow
				View shadowContainerExplorer = Activity.FindViewById<View> (Resource.Id.shadow_container_explorer);
				shadowContainerExplorer.Visibility = ViewStates.Visible;

				//Hide back button
				ImageButton buttonBackExplorer = Activity.FindViewById<ImageButton> (Resource.Id.button_back_explorer);
				buttonBackExplorer.Visibility = ViewStates.Invisible;

				HideProgressDialog();
			} 
			catch (Exception ex){
				Insights.Report(ex);
				HideProgressDialog ();
				Toast.MakeText (Activity, "Er is iets fout gegaan", ToastLength.Short).Show ();
			}
		}


		void ListView_OnItemLongClick (object sender, AdapterView.ItemLongClickEventArgs e)
		{
			LocalBox clickedItem = foundLocalBoxes [e.Position];

			LayoutInflater inflater = (LayoutInflater)Activity.GetSystemService (Context.LayoutInflaterService); 
			View popupView;
			popupView = inflater.Inflate (Resource.Layout.custom_popup_root, null);

			PopupWindow popupWindow = new PopupWindow (popupView, e.View.Width, e.View.Height);

			//Hide popup window when clicking outside its view
			popupWindow.Focusable = true;
			popupWindow.Update ();
			popupWindow.SetBackgroundDrawable(new BitmapDrawable());

			popupWindow.ShowAsDropDown (e.View, 0, - e.View.Height);

			ImageButton buttonDelete = (ImageButton) popupView.FindViewById(Resource.Id.button_popup_root_delete);

			buttonDelete.Click += delegate {
				popupWindow.Dismiss();

				var alertDialogConfirmDelete = new Android.App.AlertDialog.Builder (Activity);
				alertDialogConfirmDelete.SetTitle("Waarschuwing");
				alertDialogConfirmDelete.SetMessage("Weet u zeker dat u deze LocalBox wilt verwijderen? \nDeze actie is niet terug te draaien.");

				alertDialogConfirmDelete.SetPositiveButton ("Verwijderen", async delegate { 
					try{
						DataLayer.Instance.DeleteLocalBox(clickedItem.Id);
						ResetUIToBeginState(true);
						UpdateLocalBoxes();

						List<LocalBox> registeredLocalBoxes = await DataLayer.Instance.GetLocalBoxes ();
						if (registeredLocalBoxes.Count == 0) {
							HomeActivity homeActivity = (HomeActivity)Activity;
							homeActivity.ShowLoginDialog();
						}
						//Reset logo
						//imageViewLogo.SetImageResource (Resource.Drawable.beeldmerk_belastingdienst);
					}catch (Exception ex){
						Insights.Report(ex);
						Toast.MakeText (Android.App.Application.Context, "Het verwijderen van de LocalBox is mislukt", ToastLength.Short).Show ();
					}
				});
				alertDialogConfirmDelete.SetNegativeButton ("Annuleren", delegate { 
					alertDialogConfirmDelete.Dispose();
				});
				alertDialogConfirmDelete.Create ().Show ();
			};
		}

		private void SetCustomActionBarColor()
		{
			LinearLayout 	menuActionBar 	  = Activity.FindViewById<LinearLayout> (Resource.Id.fragment_container_menu_custom_actionbar);
			RelativeLayout 	explorerActionBar = Activity.FindViewById<RelativeLayout> (Resource.Id.fragment_container_explorer_custom_actionbar);
			RelativeLayout 	documentActionBar = Activity.FindViewById<RelativeLayout> (Resource.Id.fragment_container_document_custom_actionbar);

			menuActionBar.SetBackgroundColor (Color.ParseColor (HomeActivity.colorOfSelectedLocalBox));
			explorerActionBar.SetBackgroundColor (Color.ParseColor(HomeActivity.colorOfSelectedLocalBox));
			documentActionBar.SetBackgroundColor (Color.ParseColor(HomeActivity.colorOfSelectedLocalBox));
		}


		private void ResetUIToBeginState(bool dropboxDeleted)
		{
			ImageButton buttonFullscreenDocument = Activity.FindViewById<ImageButton> (Resource.Id.button_fullscreen_document);
			TextView textviewFilename 		 	 = Activity.FindViewById<TextView> (Resource.Id.textview_filename);
			WebView webViewDocument 			 = Activity.FindViewById<WebView> (Resource.Id.webview_document);

			textviewFilename.Visibility 		 = ViewStates.Invisible;
			buttonFullscreenDocument.Visibility  = ViewStates.Invisible;

			webViewDocument.ClearView ();

			//Clear fragment back stack
			int entryCount = Activity.SupportFragmentManager.BackStackEntryCount;
			while (entryCount > 0) {
				Activity.SupportFragmentManager.PopBackStackImmediate();
				entryCount = Activity.SupportFragmentManager.BackStackEntryCount; 
			}

			HomeActivity homeActivity = (HomeActivity)Activity;
			homeActivity.CheckToHideButtons ();

			ExplorerFragment.openedDirectories = new List<string> ();

			if (dropboxDeleted) {
				//Change action bar color to default color
				HomeActivity.colorOfSelectedLocalBox = Constants.lightblue;
				SetCustomActionBarColor ();
			}
		}


		public async void UpdateLocalBoxes()
		{
			ShowProgressDialog (Activity, null);

			foundLocalBoxes = new List<LocalBox>();
            foundLocalBoxes = await DataLayer.Instance.GetLocalBoxes ();

			menuAdapter.SetFoundLocalBoxes (foundLocalBoxes);
			menuAdapter.NotifyDataSetChanged ();

			HideProgressDialog ();
		}


		private void ShowProgressDialog (Android.Content.Context context, string textToShow)
		{
			Activity.RunOnUiThread (new Action (() => { 
				progressDialog.Show (context, textToShow);
			}));
		}

		private void HideProgressDialog ()
		{
			if (progressDialog != null) {
				Activity.RunOnUiThread (new Action (() => { 
					progressDialog.Hide ();
				}));
			}
		}

	}
}
