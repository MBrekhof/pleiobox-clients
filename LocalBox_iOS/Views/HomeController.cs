﻿using System;
using System.IO;
using CoreGraphics;
using System.Diagnostics;

using Foundation;
using UIKit;

using LocalBox_Common;
using LocalBox_iOS.Helpers;
using LocalBox_iOS.Views.ItemView;

using Xamarin;
using System.Net;
using System.Text;

namespace LocalBox_iOS.Views
{
    public partial class HomeController : UIViewController, IHome
	{
		public static HomeController homeController;
        private UIViewController _master;
		private UIViewController _detail;
		private UIViewController _authentication;
		private UIViewController _sites;
		private UIColor _defaultColor;

		#if DEBUG
			public const string PleioUrl = "http://www.pleio.dev";
		#else
			public const string PleioUrl = "https://www.pleio.nl";
		#endif

        public HomeController () : base ()
        {
        }

		public HomeController (IntPtr handle) : base (handle)
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

            _defaultColor = kleurenBalk.BackgroundColor;

			homeController = this;

			InitialiseMenu ();
        }

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			SslValidator.CertificateMismatchFound += HandleInvalidCertificate;
		}


		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);

			SslValidator.CertificateMismatchFound -= HandleInvalidCertificate;
		}



		void HandleInvalidCertificate (object sender, EventArgs e)
		{
			SslValidator.CertificateErrorRaised = true;

			//Incorrect ssl found so show pop-up
			Console.WriteLine ("SSL mismatch!!!");
			InvokeOnMainThread (() => {
				var alert = new UIAlertView ("Waarschuwing", 
					"De identiteit van de server kan niet goed vastgesteld worden. " +
					"Maakt u gebruik van een vertrouwd netwerk om de identiteit " +
					"extra te controleren?", null, "Ja", "Nee");

				alert.Clicked += (s, buttonArgs) => {
					if (buttonArgs.ButtonIndex == 0) {

						SslValidator.CertificateErrorRaised = false;
						//Get new certificate from server
						bool certificateSucessfullyRenewed = CertificateHelper.RenewCertificateForLocalBox (DataLayer.Instance.GetSelectedOrDefaultBox ());

						if (certificateSucessfullyRenewed) {
							new UIAlertView ("Succes", "Controle met succes uitgevoerd. U kunt weer verder werken.", null, "OK").Show ();
						} else {
							new UIAlertView ("Foutmelding", "Dit netwerk is niet te vertrouwen.", null, "OK").Show ();
						}
					}
				};

				alert.Show ();
			});
		}


		public void ShowAddSitesView()
		{
			AddSitesViewController addSitesViewController = new AddSitesViewController(this);
			addSitesViewController.View.Frame = new CGRect (0, 0, View.Bounds.Width, View.Bounds.Height);
			View.Add (addSitesViewController.View);
			AddChildViewController(addSitesViewController);
		}
			

		public UIPageControl GetIntroductionProgressView(int CurrentPage)
		{
			int ywidth = 330;
			if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) { //iOS 8
				ywidth = 75;
			}
				
			var pageControl = new UIPageControl {
				Pages = 2,
				Frame = new CGRect (
					_authentication.View.Frame.Width / 2 - 50,  //X
					_authentication.View.Frame.Height - ywidth, 	//Y
					100, 										//Width
					50) 										//Height
			};
			pageControl.Enabled = false;
			pageControl.CurrentPage = CurrentPage;

			return pageControl;
		}

		public void ShowIntroductionView()
		{

			_authentication = new AuthenticationViewController (PleioUrl, this, true);
			_authentication.View.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
			_authentication.View.AddSubview(GetIntroductionProgressView(0));

			View.Add(_authentication.View);
			AddChildViewController(_authentication);
		}
						
        void UpdateMaster(UIViewController viewController)
        {
            if (_master != null)
            {
                _master.View.RemoveFromSuperview();
            }
            _master = viewController;
            _master.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            _master.View.Frame = new CGRect(0, 0, 224, View.Bounds.Height);

            View.Add(_master.View);

            if(AppDelegate.localBoxToRegister == null && DataLayer.Instance.GetLocalBoxesSync ().Count == 0) {
				ShowIntroductionView ();
			}
			imageViewSplash.Hidden = true;
        }

        public void UpdateDetail(UIViewController viewController, bool animate = true)
        {
            if (!animate)
            {
                viewController.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
                viewController.View.Frame = new CGRect(224, 0, View.Bounds.Width - 224, View.Bounds.Height);

                View.Add(viewController.View);

                if(_detail != null)
                    _detail.View.RemoveFromSuperview();
                _detail = viewController;

                return;
            }

            viewController.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            viewController.View.Frame = new CGRect(View.Bounds.Width, 0, View.Bounds.Width - 200, View.Bounds.Height);

            View.Add(viewController.View);

            SetBoxKleur();

            UIView.Animate(0.5, 0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                viewController.View.Frame = new CGRect(224, 0, View.Bounds.Width - 224, View.Bounds.Height);
            }, () => {
                if(_detail != null)
                    _detail.View.RemoveFromSuperview();
                _detail = viewController;
            });
            
           
        }

        public void RemoveDetail()
        {
            if (_detail == null)
                return;

            UIView.Animate(0.5, 0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                _detail.View.Layer.Opacity = 0;
            }, () => {
                _detail.View.RemoveFromSuperview();
                _detail = null;
            });
        }

        void SetBoxKleur()
        {
			try{
            	LocalBox theBox = DataLayer.Instance.GetSelectedOrDefaultBox();
            	if (!string.IsNullOrEmpty(theBox.BackColor))
            	{
              	  	float red, green, blue;
               	 	var colorString = theBox.BackColor.Replace("#", "");
                	red = Convert.ToInt32(string.Format("{0}", colorString.Substring(0, 2)), 16) / 255f;
                	green = Convert.ToInt32(string.Format("{0}", colorString.Substring(2, 2)), 16) / 255f;
                	blue = Convert.ToInt32(string.Format("{0}", colorString.Substring(4, 2)), 16) / 255f;
                	UIColor theColor = UIColor.FromRGBA(red, green, blue, 1.0f);
                	kleurenBalk.BackgroundColor = theColor;
				}
			}catch (Exception ex){
				Insights.Report(ex);
			}
        }
			
        public void InitialiseMenu()
        {
			UpdateMaster(new MenuViewController(this));
        }

		public void InitialiseMenuAfterRegistration() {


			UpdateMaster(new MenuViewController(this));
			RemoveDetail();
			kleurenBalk.BackgroundColor = _defaultColor;
		}

		public void ValidateKeyPresence(LocalBox localBox, Action onCompletion) {
			if (localBox.HasCryptoKeys)
			{
				if (!localBox.HasPassPhrase)
				{
					ShowPassphraseForExistingKey(localBox, onCompletion);
				}
			}
			else
			{
				CreateNewKeyPair(localBox, onCompletion);
			}
		}


		private void ShowPassphraseForExistingKey(LocalBox localBox, Action onCompletion, bool repeat = false) {
			string message = string.Empty;
			if (repeat)
			{
				message = "Verkeerde passphrase opgegeven, probeer het opnieuw";
			}
			else
			{
				message = "Bij deze Pleiobox is al een sleutel aanwezig, geef de passphrase op van deze sleutel";
			}

			UIAlertView createPassPhrase = new UIAlertView("Passphrase", message, null, "Annuleer", "Ok");
			createPassPhrase.AlertViewStyle = UIAlertViewStyle.SecureTextInput;
			createPassPhrase.GetTextField(0).Placeholder = "Passphrase";

			createPassPhrase.Clicked += async (object s, UIButtonEventArgs args) => {
				if(args.ButtonIndex == 0) {
					DataLayer.Instance.DeleteLocalBox(localBox.Id);
					if(onCompletion != null) {
						onCompletion();
					}
				}else if(args.ButtonIndex == 1) {
					if(!await BusinessLayer.Instance.ValidatePassPhrase(localBox ,createPassPhrase.GetTextField(0).Text)) {
						ShowPassphraseForExistingKey(localBox, onCompletion, true);
					} else {
						if(onCompletion != null) {
							onCompletion();
						}
					}
				}
			};
			createPassPhrase.Show();
		}

		private void CreateNewKeyPair(LocalBox localBox, Action onCompletion, bool repeat = false)
		{
			string message = repeat ? "De opgegeven passphrases komen niet overeen, probeer het nog eens" : "U moet een passphrase instellen voordat u deze Pleiobox kunt gebruiken";

			UIAlertView createPassPhrase = new UIAlertView("Passphrase", message, null, "Annuleer", "Ok");
			createPassPhrase.AlertViewStyle = UIAlertViewStyle.LoginAndPasswordInput;
			var firstField = createPassPhrase.GetTextField(0);
			var secondField = createPassPhrase.GetTextField(1);

			firstField.Placeholder = "Passphrase";
			secondField.Placeholder = "Bevestig Passphrase";

			firstField.AutocorrectionType = UITextAutocorrectionType.No;
			firstField.SecureTextEntry = true;


			createPassPhrase.ShouldEnableFirstOtherButton = (av)=> {
				return av.GetTextField(0).Text.Length > 0 && av.GetTextField(0).Text.Equals(av.GetTextField(1).Text);
			};

			createPassPhrase.Clicked += async (object s, UIButtonEventArgs args) => {


				if (args.ButtonIndex == 0) {
					DataLayer.Instance.DeleteLocalBox (localBox.Id);
					if (onCompletion != null) {
						onCompletion ();
					}
				} else if (args.ButtonIndex == 1) {
					if (firstField.Text.Equals (secondField.Text)) {
	
						DialogHelper.ShowBlockingProgressDialog ("Beveiliging", "Bezig met het creëren van sleutels. Dit kan enige tijd in beslag nemen.");
						bool result = await BusinessLayer.Instance.SetPublicAndPrivateKey (localBox, firstField.Text); 
						if (!result) {
							DataLayer.Instance.DeleteLocalBox (localBox.Id);
							DialogHelper.ShowErrorDialog ("Fout", string.Format ("Er is een fout opgetreden bij het toevoegen van de Pleiobox {0}. De Pleiobox is niet toegevoegd", localBox.Name));
						} 
						if (onCompletion != null) {
							onCompletion ();
						}
						DialogHelper.HideBlockingProgressDialog ();

					} else {
						CreateNewKeyPair (localBox, onCompletion, true);
					}
					DialogHelper.HideProgressDialog ();
				}
			};
			createPassPhrase.Show();
		}
			
        void Resume(object sender, EventArgs e)
        {				
            if (AppDelegate.fileToUpload != null)
            {
				ImportFile(AppDelegate.fileToUpload);
                AppDelegate.fileToUpload = null;
            }
			InitialiseMenu();
        }
			
		public async void ImportFile (string pathToFile)
		{
			Console.WriteLine ("Filepath van bestaand of nieuw bestand " + pathToFile);

			//Get settings of last opened file
			string fileNameLastOpenedPdf = NSUserDefaults.StandardUserDefaults.StringForKey ("fileNameLastOpenedPdf");
			string pathLastOpenedPdf 	 = NSUserDefaults.StandardUserDefaults.StringForKey ("pathLastOpenedPdf");
			string temporaryFilePath 	 = NSUserDefaults.StandardUserDefaults.StringForKey ("temporaryFilePath");
			bool   isFavorite 			 = NSUserDefaults.StandardUserDefaults.BoolForKey ("isFavorite");

			if (!String.IsNullOrEmpty(fileNameLastOpenedPdf) && 
				!String.IsNullOrEmpty(pathLastOpenedPdf) && 
				!String.IsNullOrEmpty(temporaryFilePath)) 
			{
				//Controleer of pdf path overeenkomt met path van laatst geopende pdf file
				if (pathToFile.Contains (fileNameLastOpenedPdf)) {

					DialogHelper.ShowBlockingProgressDialog ("Bijwerken", "Eventuele annotaties aan het bestand opslaan. Een ogenblik geduld a.u.b.");

					try {
						bool updateSucceeded = await DataLayer.Instance.SavePdfAnnotations (pathLastOpenedPdf, pathToFile, false, isFavorite);
						if (updateSucceeded) {
						
							if(File.Exists(temporaryFilePath)){
								File.Delete(temporaryFilePath);
							}


							DialogHelper.HideBlockingProgressDialog ();

							new UIAlertView ("Succesvol", "PDF bestand succesvol bijgewerkt", null, "OK").Show ();
						} 
						else {
							DialogHelper.HideBlockingProgressDialog ();
							new UIAlertView ("Fout", "Er is een fout opgetreden bij het bijwerken van het PDF bestand. \n" +
							"Eventuele annotaties zijn niet verwerkt.", null, "OK").Show ();
						}
					} catch (Exception ex){
						Insights.Report(ex);
						DialogHelper.HideBlockingProgressDialog ();
						new UIAlertView ("Fout", "Er is een fout opgetreden bij het bijwerken van het PDF bestand. \n" +
							"Eventuele annotaties zijn niet verwerkt.", null, "OK").Show ();
					}
				}
				else {//New file
					UploadNewFile (pathToFile);
				}
				//Clear settings last opened file
				NSUserDefaults.StandardUserDefaults.SetString("", "fileNameLastOpenedPdf"); 
				NSUserDefaults.StandardUserDefaults.SetString("", "pathLastOpenedPdf"); 
				NSUserDefaults.StandardUserDefaults.SetString("", "temporaryFilePath"); 
				NSUserDefaults.StandardUserDefaults.SetBool(false, "IsFavorite");
				NSUserDefaults.StandardUserDefaults.Synchronize ();
			} 
			else {//New File
				UploadNewFile (pathToFile);
			}
		}

		private void UploadNewFile(string path)
		{
			var view = LocationChooserView.Create(path, true, null);

			View.Add(view);
		}


		public void MoveFile(string pathOfFileToMove, IListNode nodeView)
		{
			var view = LocationChooserView.Create(pathOfFileToMove, false, nodeView);

			View.Add(view);
		}


	}
}

