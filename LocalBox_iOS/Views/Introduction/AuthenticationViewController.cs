﻿
using System;

using Foundation;
using UIKit;
using LocalBox_Common.Remote;
using LocalBox_Common;
using LocalBox_Common.Remote.Authorization;
using System.Text;
using LocalBox_iOS.Views;

namespace LocalBox_iOS
{
	public partial class AuthenticationViewController : UIViewController
	{
		private string PleioUrl;
		private HomeController _home;
		private bool _introduction;

		public AuthenticationViewController (string PleioUrl, HomeController home, bool introduction) : base ("AuthenticationViewController", null)
		{
			this.PleioUrl = PleioUrl;
			this._home = home;
			this._introduction = introduction;
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

						this._home.InitialiseMenuAfterRegistration();
					}

					this.View.RemoveFromSuperview();

					// show (second) site-selection screen
					if (_introduction) {
						var sites = new AddSitesViewController (_home, true);
						sites.View.BackgroundColor = UIColor.FromRGB(14, 94, 167);
						_home.View.Add(sites.View);
						_home.AddChildViewController(sites);
					}

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

		public override void ViewDidAppear(bool didAppear) 
		{
			ActivityIndicator.Hidden = true;
			LoginButton.Hidden = false;

			PasswordField.Text = "";
		}
	}
}

