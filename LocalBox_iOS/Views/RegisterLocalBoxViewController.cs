using System;
using System.Net;
using CoreGraphics;

using Foundation;
using UIKit;

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
						localBoxToBeAdded.AccessToken = accessToken;
						localBoxToBeAdded.RefreshToken = refreshToken;
						localBoxToBeAdded.DatumTijdTokenExpiratie = expiresAsStringWithCorrection;

						homeController.AddLocalBox (localBoxToBeAdded);
						this.View.RemoveFromSuperview ();
					} else {
						new UIAlertView("Error", "Het ophalen van LocalBox data is mislukt. \nProbeer het a.u.b. opnieuw", null, "OK", null).Show ();
					}
				}
				else if (url.StartsWith ("lbox://oauth-return"))
				{ 	//User rejected permission
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
					if(foundCookie.Name.StartsWith("PHPSESSID"))
					{
						cookieString = foundCookie.Name + "=" + foundCookie.Value; 

						if(url.EndsWith("register_app")){

							if(webViewRegisterLocalBox.Request.Url.AbsoluteString.Contains(foundCookie.Domain)){ //Get the correct cookie
								RegisterLocalBox(url);
								this.View.RemoveFromSuperview();
							}
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
											"&response_type=token&redirect_uri=lbox://oauth-return";

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

