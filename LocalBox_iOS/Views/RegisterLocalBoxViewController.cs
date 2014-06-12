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
				webViewRegisterLocalBox.LoadRequest(new NSUrlRequest(NSUrl.FromString(urlToOpen)));
				webViewRegisterLocalBox.LoadStarted += delegate
				{
					viewActivityIndicator.Hidden = false;

					string url = webViewRegisterLocalBox.Request.Url.AbsoluteString;
					Console.WriteLine("CURRENT URL: " + url);
				};

				/*
				webViewRegisterLocalBox.ShouldStartLoad = (webView, request, navType) => 
				{
					string url = request.ToString();

					string headers = request.Headers.ToString();
					Console.WriteLine(headers);

					if (url.EndsWith (".json") && url.StartsWith("lbox://")) {
						viewActivityIndicator.Hidden = false;

						string newUrl = url.Replace("lbox://", "http://");
						RegisterLocalBox(newUrl);
						
						return false;
					}else
					{
						return true;
					}
				};*/


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
									RegisterLocalBox(url);
									this.View.RemoveFromSuperview();
								}
							}
						}
					}
				};
			}catch{
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
				homeController.RequestWachtwoord (AppDelegate.localBoxToRegister);
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

	}
}

