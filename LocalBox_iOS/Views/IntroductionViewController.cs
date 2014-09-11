using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using LocalBox_iOS.Views;

namespace LocalBox_iOS
{
	public partial class IntroductionViewController : UIViewController
	{
		public int pageIndex { get; private set; }
		private UIPageViewController introductionPageViewController;
		private HomeController homeController;

		public IntroductionViewController (int pageIndex, UIPageViewController introductionPageViewController, HomeController homeController) : base ("IntroductionViewController", null)
		{
			this.pageIndex = pageIndex;
			this.introductionPageViewController = introductionPageViewController;
			this.homeController = homeController;
		}
			
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (this.pageIndex == 0) {
				imageViewInfoGraphic.Image = UIImage.FromFile ("tour/tour_01.png");
				labelTitle.Text = "Stap 1";
				labelDescription.Text = "Open de URL naar de te registreren LocalBox.";
				HideButtons ();
			} else if (this.pageIndex == 1) {
				imageViewInfoGraphic.Image = UIImage.FromFile ("tour/tour_02.png");
				labelTitle.Text = "Stap 2";
				labelDescription.Text = "Voer de inloggegevens van uw LocalBox in.";
				HideButtons ();
			} else if (this.pageIndex == 2) {
				imageViewInfoGraphic.Image = UIImage.FromFile ("tour/tour_04.png");
				labelTitle.Text = "Stap 3";
				labelDescription.Text = "Stel een passphrase in of voer een bestaande passphrase in.";
				HideButtons ();
			} else if (this.pageIndex == 3) {
				imageViewInfoGraphic.Image = UIImage.FromFile ("tour/tour_05.png");
				labelTitle.Text = "";
				labelDescription.Text = "";
				labelImageDescription.Text = "U kunt beginnen met het registreren van een LocalBox.";
				ShowButtons ();
			}


			buttonCancel.TouchUpInside += (o, s) => 
			{
				introductionPageViewController.View.RemoveFromSuperview();
			};

			buttonOpenInternetBrowser.TouchUpInside += (o, s) => {
			
				UIAlertView alertOpenUrl = new UIAlertView ("Nieuwe LocalBox", 
															"Voer hieronder de url naar de te registeren LocalBox in", null, 
															"Annuleer", "Open URL");
				alertOpenUrl.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
				alertOpenUrl.GetTextField(0).Placeholder = "https://yourlocalbox.com";
				alertOpenUrl.Clicked += (object sender, UIButtonEventArgs args) => 
				{
					if (args.ButtonIndex == 1)
					{
						string urlString = ((UIAlertView)sender).GetTextField(0).Text;

						if(string.IsNullOrEmpty(urlString))
						{
							var alertView = new UIAlertView("Error", "URL is niet ingevuld", null, "OK", null);
							alertView.Show();
						}
						else{
							//Open webview
							introductionPageViewController.View.RemoveFromSuperview ();

							homeController.ShowRegisterLocalBoxView(urlString);
						}
					}
				};
				alertOpenUrl.Show();
			};
		}


		void HideButtons()
		{
			buttonCancel.Hidden = true;
			buttonOpenInternetBrowser.Hidden = true;
			labelImageDescription.Hidden = true;
		}

		void ShowButtons()
		{
			buttonCancel.Hidden = false;
			buttonOpenInternetBrowser.Hidden = false;
			labelImageDescription.Hidden = false;
		}
	}
}
