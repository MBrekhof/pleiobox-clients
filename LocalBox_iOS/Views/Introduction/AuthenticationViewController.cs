
using System;

using Foundation;
using UIKit;
using LocalBox_Common.Remote;
using LocalBox_Common;
using LocalBox_Common.Remote.Authorization;
using XLabs.Platform.Services;
using System.Text;
using LocalBox_iOS.Views;

namespace LocalBox_iOS
{
	public partial class AuthenticationViewController : UIViewController
	{
		private string PleioUrl;
		private HomeController _home;
		private UIPageViewController _introduction;
		private UIViewController _addsites;

		public AuthenticationViewController (string PleioUrl, HomeController home,  UIPageViewController introduction, UIViewController addsites) : base ("AuthenticationViewController", null)
		{
			this.PleioUrl = PleioUrl;
			this._home = home;
			this._introduction = introduction;
			this._addsites = addsites;
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
			
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			ActivityIndicator.Hidden = true;

			// Perform any additional setup after loading the view, typically from a nib.
			LoginButton.TouchUpInside += async (s, e) => {
				LoginButton.Hidden = true;
				ActivityIndicator.Hidden = false;
				ActivityIndicator.StartAnimating();

				var authorization = new LocalBoxAuthorization(PleioUrl);

				var result = await authorization.Authorize(LoginField.Text, PasswordField.Text); 
				if (result) {
					var business = new BusinessLayer();

					if (DataLayer.Instance.GetLocalBoxesSync ().Count  == 0) {
						await business.RegisterLocalBox(PleioUrl);
					}

					this._introduction.SetViewControllers(new UIViewController[] {
						_addsites
					}, UIPageViewControllerNavigationDirection.Forward, true, s2 => {});
				} else {
					LoginButton.Hidden = false;
					ActivityIndicator.Hidden = true;
					ActivityIndicator.StopAnimating();

					var alert = UIAlertController.Create("Fout", "Gebruikersnaam of wachtwoord is onjuist.", UIAlertControllerStyle.Alert);
					alert.AddAction (UIAlertAction.Create ("Ok", UIAlertActionStyle.Cancel, null));
					PresentViewController(alert, animated: true, completionHandler: null);
				}
			};
		}

		public override async void ViewDidAppear(bool didAppear) 
		{
			ActivityIndicator.Hidden = true;
			LoginButton.Hidden = false;

			PasswordField.Text = "";
		}
	}
}

