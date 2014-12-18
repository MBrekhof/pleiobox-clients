using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using LocalBox_iOS.Views;
using LocalBox_Common;
using System.Net;

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
				OpenUrlDialog();
			};
		}

		void OpenUrlDialog()
		{
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

						if(urlString.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase))
						{
							UIAlertView alertHttpUrl = new UIAlertView ("Waarschuwing", 
								"U heeft een http webadres opgegeven. Weet u zeker dat u een onbeveiligde verbinding wilt opzetten?", null, 
								"Annuleer", "Ga verder");
							alertHttpUrl.Clicked += (object send, UIButtonEventArgs a) => 
							{
								if (a.ButtonIndex == 1)
								{
									if(!string.IsNullOrEmpty(urlString))
									{
										OpenInternetBrowser(urlString);
									}
								}else {
									OpenUrlDialog();
								}
							};
							alertHttpUrl.Show();
						}
						else if (urlString.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
						{
							if(CertificateHelper.DoesHaveAValidCertificate(urlString))
							{
								OpenInternetBrowser(urlString);
							}
							else {
								UIAlertView alertHttpUrl = new UIAlertView ("Error", 
									"U heeft een webadres opgegeven met een ssl certificaat welke niet geverifieerd is. Dit wordt momenteel niet ondersteund door de iOS app.\n\n" +
									"Als alternatief kunt u, indien de LocalBox server dit ondersteund, een http:// webadres opgeven.", null, 
									 "OK", null);
								alertHttpUrl.Clicked += (object send, UIButtonEventArgs a) => 
								{
									OpenUrlDialog();
								};
								alertHttpUrl.Show();
							}
						}else {
							UIAlertView alertUrl = new UIAlertView ("Error", 
								"Het opgegeven webadres dient met https:// of http:// te beginnen.", null, 
								"OK", null);
							alertUrl.Clicked += (object send, UIButtonEventArgs a) => 
							{
								OpenUrlDialog();
							};
							alertUrl.Show();
						}
					}
				}
			};
			alertOpenUrl.Show();
		}





		void OpenInternetBrowser(string urlString)
		{
			//Open webview
			introductionPageViewController.View.RemoveFromSuperview ();

			homeController.ShowRegisterLocalBoxView(urlString);
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
