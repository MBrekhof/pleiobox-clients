using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Android.OS;
using Android.App;
using Android.Views;
using Android.Widget;
using Android.Webkit;
using Android.Content;
using Android.Runtime;
using Android.Net.Http;

using LocalBox_Common;

namespace localbox.android
{
	public class RegisterLocalBoxFragment : DialogFragment
	{
		public HomeActivity parentActivity;
		public string uriOfUrlToOpen;
		public bool registrationStarted;
		private string urlToOpen;
		public bool ignoreSslError;
		private WebView webview;
		public LocalBox localBoxToBeAdded;

		public RegisterLocalBoxFragment(string urlToOpen, bool ignoreSslError)
		{
			this.urlToOpen = urlToOpen;
			this.ignoreSslError = ignoreSslError;

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

			webview = view.FindViewById<WebView>(Resource.Id.webview_register_localbox);

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

			LocalBox box = await BusinessLayer.Instance.RegisterLocalBox (url, cookieString, true);

			if (box != null) {

				//Set certificate for localbox
				//box.OriginalSslCertificate = CertificateHelper.BytesOfCertificate;

				parentActivity.HideProgressDialog ();

				if (string.IsNullOrEmpty (box.AccessToken) || string.IsNullOrEmpty (box.RefreshToken)) {
					localBoxToBeAdded = box;

					//Create request url to get the access token and refresh token
					var domainUrl = url.Substring (0, url.IndexOf ("/register"));
					var tokensRequestUrl = 	domainUrl + "/oauth/v2/auth?client_id=" + box.ApiKey +
						"&response_type=token&redirect_uri=lbox://oauth-return";

					//Get access token and refresh token
					webview.LoadUrl(tokensRequestUrl);
				}

			} else {
				parentActivity.HideProgressDialog ();
				Toast.MakeText (Activity, "Het ophalen van LocalBox data is mislukt. \nProbeer het a.u.b. opnieuw", ToastLength.Long).Show ();
			}
		}

		public void AddLocalBox()
		{
			parentActivity.AddLocalBox (localBoxToBeAdded);
		}






		private class MyWebViewClient : WebViewClient
		{
			private RegisterLocalBoxFragment registerLocalBoxFragment;
			private string cookieString;
			private bool localBoxAlreadyAdded = false;

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
						}
					}
				}

			}

			public override void OnReceivedSslError(WebView view, SslErrorHandler handler, SslError error)
			{
				if (registerLocalBoxFragment.ignoreSslError) { //user accepted to proceed using an invalid certificate
					ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;
					handler.Proceed (); 
				} else {
					registerLocalBoxFragment.parentActivity.ShowToast ("Er is een fout gevonden in het SSL certificaat. Probeer het a.u.b. opnieuw.");
					registerLocalBoxFragment.Dismiss ();
				}
			}

			public override void OnPageStarted (WebView view, string url, Android.Graphics.Bitmap favicon)
			{
				base.OnPageStarted (view, url, favicon);

				if (!localBoxAlreadyAdded) {
				
					if (url.Contains ("refresh_token=") && url.Contains ("access_token=")) {

						//Get access token
						var startIndexAccessToken = url.IndexOf ("access_token=") + "access_token=".Length;
						var endIndexAccessToken = url.IndexOf ("&expires_in");
						var accessToken = url.Substring (startIndexAccessToken, endIndexAccessToken - startIndexAccessToken);

						//Get refresh token
						var startIndexRefreshToken = url.IndexOf ("refresh_token=") + "refresh_token=".Length;
						var refreshToken = url.Substring (startIndexRefreshToken);

						//Get expiration date access token
						var startIndexExpires = url.IndexOf ("expires_in=") + "expires_in=".Length;
						var endIndexExpires = url.IndexOf ("&token_type=");
						var expiresAsInt = int.Parse (url.Substring (startIndexExpires, endIndexExpires - startIndexExpires));
						var expiresAsStringWithCorrection = DateTime.UtcNow.AddSeconds (expiresAsInt * 0.9).ToString (); //Expire at 90% of expire duration

						if (!string.IsNullOrEmpty (accessToken) && !string.IsNullOrEmpty (refreshToken)) {
							registerLocalBoxFragment.localBoxToBeAdded.AccessToken = accessToken;
							registerLocalBoxFragment.localBoxToBeAdded.RefreshToken = refreshToken;
							registerLocalBoxFragment.localBoxToBeAdded.DatumTijdTokenExpiratie = expiresAsStringWithCorrection;

							registerLocalBoxFragment.AddLocalBox ();
							registerLocalBoxFragment.Dismiss ();
						} else {
							registerLocalBoxFragment.parentActivity.ShowToast ("Het ophalen van LocalBox data is mislukt. \\nProbeer het a.u.b. opnieuw");
						}
						localBoxAlreadyAdded = true;

					} else if (url.StartsWith ("lbox://oauth-return")) { 	//User rejected permission
						registerLocalBoxFragment.Dismiss ();
					}

				}
			}
		}

	}
}

