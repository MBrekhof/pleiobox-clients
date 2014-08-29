using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using LocalBox_Common;
using LocalBox_iOS.Views;

namespace LocalBox_iOS
{
	public partial class RegisterLocalBoxViewController : UIViewController
	{
		private string urlToOpen;
		private string cookieString;
		private string enteredUsername;
		private string enteredPassword;
		private HomeController homeController;

		public RegisterLocalBoxViewController (string urlToOpen, HomeController homeController) : base ("RegisterLocalBoxViewController", null)
		{
			this.urlToOpen = urlToOpen;
			this.homeController = homeController;
		}

		public override void DidReceiveMemoryWarning ()
		{
			base.DidReceiveMemoryWarning ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			try{
				NSHttpCookieStorage.SharedStorage.AcceptPolicy = NSHttpCookieAcceptPolicy.Always;

				webViewRegisterLocalBox.ScalesPageToFit = true;

				var request = new NSUrlRequest(NSUrl.FromString(urlToOpen));

				webViewRegisterLocalBox.LoadRequest(request);
				webViewRegisterLocalBox.LoadStarted += delegate
				{
					viewActivityIndicator.Hidden = false;

					string url = webViewRegisterLocalBox.Request.Url.AbsoluteString;
					Console.WriteLine("CURRENT URL: " + url);
				};
					
				webViewRegisterLocalBox.ShouldStartLoad += webViewShouldStartLoad;

				webViewRegisterLocalBox.LoadFinished += async delegate
				{
					viewActivityIndicator.Hidden = true;

					string url = webViewRegisterLocalBox.Request.Url.AbsoluteString;

					Console.WriteLine("URL OPENED: " + url);

					//Set cookie
					var store = MonoTouch.Foundation.NSHttpCookieStorage.SharedStorage;
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
									}
			
									this.View.RemoveFromSuperview();
								}
							}
						}
					}
				};
			}catch (Exception ex){
				this.View.RemoveFromSuperview();
				var alertView = new UIAlertView("Error", "Openen van internet browser is mislukt. \nProbeer het a.u.b. opnieuw", null, "OK", null);
				alertView.Show();
			}
		}


		private async void RegisterLocalBox(string newUrl)
		{
			LocalBox box = await BusinessLayer.Instance.RegisterLocalBox (newUrl, cookieString, false);

			if (box != null) {
				AppDelegate.localBoxToRegister = box;
				this.View.RemoveFromSuperview ();
				homeController.RequestWachtwoord (AppDelegate.localBoxToRegister, enteredUsername, enteredPassword);
			} else {
				viewActivityIndicator.Hidden = true;
				var alertView = new UIAlertView("Error", "Het ophalen van LocalBox data is mislukt. \nProbeer het a.u.b. opnieuw", null, "OK", null);
				alertView.Show();
			}
		}


		partial void CloseView (MonoTouch.Foundation.NSObject sender)
		{
			this.View.RemoveFromSuperview();
		}


		bool webViewShouldStartLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			//Get username and password from web form
			enteredUsername = webView.EvaluateJavascript("document.getElementById('username').value");
			enteredPassword = webView.EvaluateJavascript("document.getElementById('password').value");

			return true;
		}

	}
}

