using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Webkit;

namespace localbox.android
{
	public class AboutAppFragment : DialogFragment
	{

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
		}

		public override View OnCreateView (LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle)
		{
			Dialog.SetTitle("Over de app");

			View view = layoutInflater.Inflate (Resource.Layout.fragment_about_app, viewGroup, false);

			WebView webview = view.FindViewById<WebView>(Resource.Id.webview_about_app);

			webview.Settings.JavaScriptEnabled = true;
			webview.SetWebChromeClient(new WebChromeClient());
			webview.LoadUrl("file:///android_asset/aboutlocalbox.html");

			return view;
		}

	}
}

