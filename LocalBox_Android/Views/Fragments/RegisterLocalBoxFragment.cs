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
using Xamarin;

namespace LocalBox_Droid
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
		public string domainUrl;

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
			}catch (Exception ex){
				Insights.Report(ex);
				return view;
			}
			return view;
		}


		public async void GetLocalBoxRegistrationJson(string url, string cookieString)
		{
			parentActivity.ShowProgressDialog("Laden...");

			LocalBox box = await BusinessLayer.Instance.RegisterLocalBox (url, cookieString, true);

			if (box != null) {

				parentActivity.HideProgressDialog ();

				if (string.IsNullOrEmpty (box.AccessToken) || string.IsNullOrEmpty (box.RefreshToken)) {
					localBoxToBeAdded = box;

					//Create request url to get the access token and refresh token
					domainUrl = url.Substring (0, url.IndexOf ("/register"));
					var tokensRequestUrl = 	domainUrl + "/oauth/v2/auth?client_id=" + box.ApiKey + "&response_type=code&redirect_uri=lbox://oauth-return";

					//Get access token and refresh token
					webview.LoadUrl(tokensRequestUrl);
				}

			} else {
				parentActivity.HideProgressDialog ();
				Toast.MakeText (Activity, "Het ophalen van LocalBox data is mislukt. \nProbeer het a.u.b. opnieuw", ToastLength.Long).Show ();
			}
		}

		public async void GetTokensAndAddLocalBox(string url, string postString)
		{
			var tokens = await BusinessLayer.Instance.GetRegistrationTokens (url, postString);

			if (tokens != null && !string.IsNullOrEmpty (tokens.AccessToken) && !string.IsNullOrEmpty (tokens.RefreshToken)) {

				//Get expiration date access token
				var expiresAsInt = tokens.ExpiresIn;
				var expiresAsStringWithCorrection = DateTime.UtcNow.AddSeconds (expiresAsInt * 0.9).ToString (); //Expire at 90% of expire duration
				
				localBoxToBeAdded.AccessToken = tokens.AccessToken;
				localBoxToBeAdded.RefreshToken = tokens.RefreshToken;
				localBoxToBeAdded.DatumTijdTokenExpiratie = expiresAsStringWithCorrection;

				parentActivity.AddLocalBox (localBoxToBeAdded);
			} 
			else {
				Toast.MakeText (Activity, "Het ophalen van LocalBox data is mislukt. \nProbeer het a.u.b. opnieuw", ToastLength.Long).Show ();
			}
			parentActivity.HideProgressDialog ();
			this.Dismiss ();
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
							registerLocalBoxFragment.GetLocalBoxRegistrationJson (url, cookieString);
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
	
					if (url.StartsWith ("lbox://oauth-return?code=")) { 	
						if (!string.IsNullOrEmpty (registerLocalBoxFragment.localBoxToBeAdded.ApiKey) &&
						    !string.IsNullOrEmpty (registerLocalBoxFragment.localBoxToBeAdded.ApiSecret) &&
						    !string.IsNullOrEmpty (registerLocalBoxFragment.domainUrl)) {

							string code = url.Substring ("lbox://oauth-return?code=".Length);

							string requestUrl = registerLocalBoxFragment.domainUrl + "/oauth/v2/token";

							string postString = "client_id=" + registerLocalBoxFragment.localBoxToBeAdded.ApiKey +
							                    "&client_secret=" + registerLocalBoxFragment.localBoxToBeAdded.ApiSecret +
							                    "&code=" + code +
												"&grant_type=authorization_code" +
												"&redirect_uri=lbox://oauth-return";
								
							registerLocalBoxFragment.GetTokensAndAddLocalBox (requestUrl, postString);
						}
						registerLocalBoxFragment.Dismiss ();
						localBoxAlreadyAdded = true;
					}
					else if (url.StartsWith ("lbox://oauth-return")) { //webview wants to open redirect uri
						registerLocalBoxFragment.Dismiss ();
					}
				}
			}

		}

	}
}