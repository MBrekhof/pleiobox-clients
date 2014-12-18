using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Support.V4.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Webkit;

using System.IO;

namespace LocalBox_Droid
{
	public class DocumentFragment : Fragment
	{
		public static string pathOfFile;
		public static string fileName;

		public DocumentFragment(string pathOfFile, string fileName)
		{
			DocumentFragment.pathOfFile = pathOfFile;
			DocumentFragment.fileName = fileName;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
		}

		public override View OnCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle)
		{
			View view = layoutInflater.Inflate(Resource.Layout.fragment_document, viewGroup, false);

			return view;
		}

		public override void OnActivityCreated (Bundle savedInstanceState)
		{
			base.OnActivityCreated (savedInstanceState);

			//Open file in webview
			WebView webViewDocument = Activity.FindViewById<WebView> (Resource.Id.webview_document);
			webViewDocument.LoadUrl("file:///" + pathOfFile);

			//Resize webview + make it scrollable
			webViewDocument.Settings.LoadWithOverviewMode = true;
			webViewDocument.Settings.UseWideViewPort = true; 
			webViewDocument.Settings.BuiltInZoomControls = true;

			//Show textview with filename
			TextView textviewFilename = Activity.FindViewById<TextView> (Resource.Id.textview_filename);
			textviewFilename.Text = fileName;
			textviewFilename.Visibility = ViewStates.Visible;

			//Set font
			FontHelper.SetFont (textviewFilename);

			//Enable fullscreen button
			ImageButton buttonFullscreenDocument = Activity.FindViewById<ImageButton> (Resource.Id.button_fullscreen_document);
			buttonFullscreenDocument.Visibility = ViewStates.Visible;
		}


	}
}

