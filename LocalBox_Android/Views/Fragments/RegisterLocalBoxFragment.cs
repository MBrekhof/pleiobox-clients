using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;

using LocalBox_Common;

namespace localbox.android
{

	public class RegisterLocalBoxFragment : DialogFragment
	{
		public HomeActivity parentActivity;
		public string uriOfUrlToOpen;
		private string urlToOpen;
		public bool registrationStarted;

		public RegisterLocalBoxFragment(string urlToOpen)
		{
			this.urlToOpen = urlToOpen;

			//Determine http or https - uri used to download json
			if (urlToOpen.StartsWith ("http://", StringComparison.OrdinalIgnoreCase)) {
				this.uriOfUrlToOpen = "http://";
			} else {
				this.uriOfUrlToOpen = "https://";
			}
		}

		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			this.parentActivity = (HomeActivity)Activity;
		}

		public override View OnCreateView (LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle)
		{
			Dialog.SetTitle("Registreren");

			View view = layoutInflater.Inflate (Resource.Layout.fragment_register_localbox, viewGroup, false);

			WebView webview = view.FindViewById<WebView>(Resource.Id.webview_register_localbox);

			try{
				CookieManager.Instance.SetAcceptCookie (true);

				webview.Settings.JavaScriptEnabled = true;
				webview.Settings.UseWideViewPort = true;
				webview.Settings.JavaScriptCanOpenWindowsAutomatically = true;
				webview.SetWebViewClient(new MyWebViewClient(this));
				webview.LoadUrl(urlToOpen);
			}catch{
				return view;
			}
			return view;
		}


		public async void StartRegistration(string url, string cookieString)
		{
			parentActivity.ShowProgressDialog("Laden...");

			if (url.StartsWith ("lbox://")) {
				url = url.Replace ("lbox://", uriOfUrlToOpen);
			}

			LocalBox box = await BusinessLayer.Instance.RegisterLocalBox (url, cookieString, true);

			if (box != null) {
				parentActivity.HideProgressDialog ();
				parentActivity.RegisterLocalBox (box);
				this.Dismiss ();
			} else {
				parentActivity.HideProgressDialog ();
				Toast.MakeText (Activity, "Het ophalen van LocalBox data is mislukt. \nProbeer het a.u.b. opnieuw", ToastLength.Long).Show ();
			}
		}




		private class MyWebViewClient : WebViewClient
		{
			RegisterLocalBoxFragment registerLocalBoxFragment;
			string cookieString;

			public MyWebViewClient(RegisterLocalBoxFragment registerLocalBoxFragment)
			{
				this.registerLocalBoxFragment = registerLocalBoxFragment;
			}

			public override WebResourceResponse ShouldInterceptRequest (WebView view, string url)
			{
				return base.ShouldInterceptRequest (view, url);
			}

			public override bool ShouldOverrideUrlLoading (WebView view, string url)
			{
				//Determine http or https - uri used to download json
				if (url.StartsWith ("http://", StringComparison.OrdinalIgnoreCase)) {
					registerLocalBoxFragment.uriOfUrlToOpen = "http://";
				} else {
					registerLocalBoxFragment.uriOfUrlToOpen = "https://";
				}

				view.LoadUrl (url);

				return true;
			}


			public override void OnPageFinished (WebView view, string url)
			{
				base.OnPageFinished (view, url);

				if (CookieManager.Instance.GetCookie (url) != null) {
					cookieString = CookieManager.Instance.GetCookie (url);

					if(url.EndsWith("register_app")){

						if (registerLocalBoxFragment.registrationStarted == false) //Voorkomt 2x zelfde aanroep
						{
							registerLocalBoxFragment.StartRegistration (url, cookieString);
							registerLocalBoxFragment.registrationStarted = true;

							registerLocalBoxFragment.Dismiss ();
						}
					}

				}

			}
		}
			
	}
}

