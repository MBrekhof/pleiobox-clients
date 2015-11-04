using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;
using Android.App;
using Android.Graphics;

using LocalBox_Common;
using Xamarin;

namespace LocalBox_Droid
{
	[Activity(Label = "Document", ScreenOrientation=Android.Content.PM.ScreenOrientation.Landscape)]	
	public class DocumentFullscreenActivity : Activity
	{

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.activity_document_fullscreen);

			//Hide action bar
			this.ActionBar.Hide ();

			//Change color of custom action bar
			LinearLayout documentActionBar = FindViewById<LinearLayout> (Resource.Id.document_fullscreen_custom_actionbar);
			if (!String.IsNullOrEmpty (HomeActivity.colorOfSelectedLocalBox)) {
				documentActionBar.SetBackgroundColor (Color.ParseColor (HomeActivity.colorOfSelectedLocalBox));
			} else {
				documentActionBar.SetBackgroundColor (Color.ParseColor (Constants.lightblue));
			}

			try{
				//Open file in webview
				WebView webViewDocumentFullscreen 	= FindViewById<WebView> (Resource.Id.webview_document_fullscreen);
				string fullPath = "file:///" + DocumentFragment.pathOfFile;

				webViewDocumentFullscreen.LoadUrl(fullPath);

				//Resize webview + make it scrollable
				webViewDocumentFullscreen.Settings.LoadWithOverviewMode = true;
				webViewDocumentFullscreen.Settings.UseWideViewPort = true; 
				webViewDocumentFullscreen.Settings.BuiltInZoomControls = true;

				//Set file name
				TextView textviewFilename = FindViewById<TextView> (Resource.Id.textview_filename_document_fullscreen);
				textviewFilename.Text = DocumentFragment.fileName;

				//Set Font
				FontHelper.SetFont (textviewFilename);

			}catch (Exception ex){
				Insights.Report(ex);
				Toast.MakeText (this, "Het openen van het bestand is mislukt", ToastLength.Short).Show ();
				this.Finish ();
			}
		}


		protected override void OnPause ()
		{
			base.OnPause ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
		}

	}
}

