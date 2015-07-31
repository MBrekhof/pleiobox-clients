using System;
using System.Net;

using UIKit;
using Foundation;
using CoreGraphics;

using LocalBox_Common;
using LocalBox_iOS.Views;

using Xamarin;

namespace LocalBox_iOS
{
	public partial class RegisterLocalBoxViewController : UIViewController
	{
		private string urlToOpen;
		private string cookieString;
		private HomeController homeController;
		private LocalBox localBoxToBeAdded;


		public RegisterLocalBoxViewController (string urlToOpen, HomeController homeController) : base ("RegisterLocalBoxViewController", null)
		{
			this.urlToOpen = urlToOpen;
			this.homeController = homeController;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			try{
				NSHttpCookieStorage.SharedStorage.AcceptPolicy = NSHttpCookieAcceptPolicy.Always;

				webViewRegisterLocalBox.ScalesPageToFit = true;
				webViewRegisterLocalBox.LoadRequest(new NSUrlRequest(NSUrl.FromString(urlToOpen)));
				webViewRegisterLocalBox.LoadStarted += webViewLoadStarted;
				webViewRegisterLocalBox.ShouldStartLoad += webViewShouldStartLoad;
				webViewRegisterLocalBox.LoadFinished += webViewLoadFinished;
			}
			catch (Exception ex){
				Insights.Report(ex);
				this.View.RemoveFromSuperview();
				new UIAlertView ("Error", "Openen van internet browser is mislukt. \nProbeer het a.u.b. opnieuw", null, "OK", null).Show ();
			}
		}



		bool webViewShouldStartLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			try {
				var url = request.ToString ();
			
				if (url.StartsWith ("lbox://oauth-return?code=")) { 	
					if (!string.IsNullOrEmpty (localBoxToBeAdded.ApiKey) &&
						!string.IsNullOrEmpty (localBoxToBeAdded.ApiSecret) &&
						!string.IsNullOrEmpty (localBoxToBeAdded.BaseUrl)) {

						string code = url.Substring ("lbox://oauth-return?code=".Length);
						string domain = "";
						if(localBoxToBeAdded.BaseUrl.EndsWith ("/")){
							domain = localBoxToBeAdded.BaseUrl.Substring (0, localBoxToBeAdded.BaseUrl.Length - 1);
						}else {
							domain = localBoxToBeAdded.BaseUrl;
						}

						string requestUrl = domain + "/oauth/v2/token";
						string postString = "client_id=" + localBoxToBeAdded.ApiKey +
						"&client_secret=" + localBoxToBeAdded.ApiSecret +
						"&code=" + code +
						"&grant_type=authorization_code" +
						"&redirect_uri=lbox://oauth-return";

						GetTokensAndAddLocalBox (requestUrl, postString);
					}
				}
				else if (url.StartsWith ("lbox://oauth-return")) { //webview wants to open redirect uri
					this.View.RemoveFromSuperview ();
				}

				return true;
			} 
			catch (Exception ex){
				Insights.Report(ex);
				this.View.RemoveFromSuperview ();
				var alertView = new UIAlertView("Error", "Het ophalen van LocalBox data is mislukt. \nProbeer het a.u.b. opnieuw", null, "OK", null);
				alertView.Show();

				return false;
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

				homeController.AddLocalBox (localBoxToBeAdded);
			} 
			else {
				new UIAlertView("Error", "Het ophalen van LocalBox data is mislukt. \nProbeer het a.u.b. opnieuw", null, "OK", null).Show ();
			}
			this.View.RemoveFromSuperview();
		}





		void webViewLoadStarted (object sender, EventArgs e)
		{
			viewActivityIndicator.Hidden = false;

			string url = webViewRegisterLocalBox.Request.Url.AbsoluteString;
			Console.WriteLine("CURRENT URL: " + url);
		}


		void webViewLoadFinished (object sender, EventArgs e)
		{
			viewActivityIndicator.Hidden = true;

			string url = webViewRegisterLocalBox.Request.Url.AbsoluteString;
			Console.WriteLine("URL OPENED: " + url);

			//Set cookie
			var store = Foundation.NSHttpCookieStorage.SharedStorage;
			var cookies = store.Cookies;

			if(cookies.Length > 0){

				foreach(NSHttpCookie foundCookie in cookies)
				{
					if(foundCookie.Name.StartsWith("Elgg"))
					{
						cookieString = foundCookie.Name + "=" + foundCookie.Value; 

						if(url.EndsWith("register_app")){
							RegisterLocalBox(url);
							this.View.RemoveFromSuperview();
						}
					}
				}
			}
		}



		private async void RegisterLocalBox (string newUrl)
		{
			LocalBox box = await BusinessLayer.Instance.RegisterLocalBox (newUrl, cookieString, false);

			if (box != null) {

				if (string.IsNullOrEmpty (box.AccessToken) || string.IsNullOrEmpty (box.RefreshToken)) {
					localBoxToBeAdded = box;
					homeController.Add (this.View);

					//Create request url to get the access token and refresh token
					var domainUrl = newUrl.Substring (0, newUrl.IndexOf ("/register"));
					var tokensRequestUrl = 	domainUrl + "/oauth/v2/auth?client_id=" + box.ApiKey +
											"&response_type=code&redirect_uri=lbox://oauth-return";

					//Get access token and refresh token
					webViewRegisterLocalBox.LoadRequest (new NSUrlRequest (NSUrl.FromString (tokensRequestUrl)));
				}
			}
			else {
				viewActivityIndicator.Hidden = true;
				new UIAlertView ("Error", "Het ophalen van LocalBox data is mislukt. \nProbeer het a.u.b. opnieuw", null, "OK", null).Show ();
			}
		}


		partial void CloseView (Foundation.NSObject sender)
		{
			this.View.RemoveFromSuperview();
		}
	}

}

