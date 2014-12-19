using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;

using MonoTouch.Foundation;
using MonoTouch.UIKit;

using LocalBox_Common;
using LocalBox_iOS.Helpers;
using LocalBox_iOS.Views.ItemView;

using Xamarin;

namespace LocalBox_iOS.Views
{
    public partial class HomeController : UIViewController, IHome
	{
		public static HomeController homeController;
        private UIViewController _master;
		private UIViewController _detail;
		private UIPageViewController _introduction;
		private UIView _pinView;
		private UIColor _defaultColor;

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

			bool databaseCreated = DataLayer.Instance.DatabaseCreated ();


			if(!databaseCreated) {
				PincodeInstellen ();
			} else {
				PincodeOpgeven ();
			}


			SslValidator.CertificateMismatchFound += (object sender, EventArgs e) => {
				SslValidator.CertificateErrorRaised = true;

				//Incorrect ssl found so show pop-up
				Console.WriteLine ("SSL mismatch!!!");
				InvokeOnMainThread (() => {
					var alert = new UIAlertView ("Waarschuwing", 
						           "De identiteit van de server kan niet goed vastgesteld worden. " +
						           "Maakt u gebruik van een vertrouwd netwerk om de identiteit " +
						           "extra te controleren?", null, "Ja", "Nee");

					alert.Clicked += async (s, buttonArgs) => {
						if (buttonArgs.ButtonIndex == 0) {

							//Get new certificate from server
							bool certificateSucessfullyRenewed = await CertificateHelper.VerifyCertificateForLocalBox (DataLayer.Instance.GetSelectedOrDefaultBox ());

							if (certificateSucessfullyRenewed) {
								new UIAlertView ("Succes", "Controle met succes uitgevoerd. U kunt weer verder werken.", null, "OK").Show ();
							} else {
								new UIAlertView ("Foutmelding", "Dit netwerk is niet te vertrouwen.", null, "OK").Show ();
							}
						}
					};
				
					alert.Show ();
				});
			};
        }


		public void ShowRegisterLocalBoxView(string urlToOpen)
		{
			if (urlToOpen.IndexOf("register_app", StringComparison.OrdinalIgnoreCase) < 0) //url misses '/register_app' at the end
			{
				if (urlToOpen.EndsWith ("/")) {	//input url is like "www.mylocalbox.nl/"
					urlToOpen = urlToOpen + "register_app";
				}
				else {	//input url is like "www.mylocalbox.nl"
					urlToOpen = urlToOpen + "/register_app";
				}
			}

			RegisterLocalBoxViewController registerLocalBoxViewController = new RegisterLocalBoxViewController (urlToOpen, this);
			registerLocalBoxViewController.View.Frame = new RectangleF(0, 0, View.Bounds.Width, View.Bounds.Height);

			View.Add(registerLocalBoxViewController.View);
			AddChildViewController(registerLocalBoxViewController);
		}


		public void ShowIntroductionView()
		{
			_introduction = new UIPageViewController(UIPageViewControllerTransitionStyle.Scroll,
								UIPageViewControllerNavigationOrientation.Horizontal,
								UIPageViewControllerSpineLocation.Min);

			IntroductionViewController firstPageController = new IntroductionViewController(0, _introduction, this);

			_introduction.DataSource = new IntroductionPageDataSource(_introduction, this);

			_introduction.SetViewControllers(new UIViewController[] { firstPageController }, 
											UIPageViewControllerNavigationDirection.Forward, false, s => { });

			//Dimension
			_introduction.View.Frame = new RectangleF(0, 0, View.Bounds.Width, View.Bounds.Height);

			View.Add(_introduction.View);
			AddChildViewController(_introduction);
		}

        void UpdateMaster(UIViewController viewController)
        {
            if (_master != null)
            {
                _master.View.RemoveFromSuperview();
            }
            _master = viewController;
            _master.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
            _master.View.Frame = new RectangleF(0, 0, 224, View.Bounds.Height);

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
                viewController.View.Frame = new RectangleF(224, 0, View.Bounds.Width - 224, View.Bounds.Height);

                View.Add(viewController.View);

                if(_detail != null)
                    _detail.View.RemoveFromSuperview();
                _detail = viewController;

                return;
            }

            viewController.View.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
            viewController.View.Frame = new RectangleF(View.Bounds.Width, 0, View.Bounds.Width - 200, View.Bounds.Height);

            View.Add(viewController.View);

            SetBoxKleur();

            UIView.Animate(0.5, 0, UIViewAnimationOptions.CurveEaseOut, () =>
            {
                viewController.View.Frame = new RectangleF(224, 0, View.Bounds.Width - 224, View.Bounds.Height);
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

        public void Lock()
        {
            if (_master != null)
                _master.View.RemoveFromSuperview();
            if (_detail != null)
                _detail.View.RemoveFromSuperview();

            DataLayer.Instance.LockDatabase();
            kleurenBalk.BackgroundColor = _defaultColor;

            PincodeOpgeven(null);
        }

        public void InitialiseMenu()
        {
			UpdateMaster(new MenuViewController(this));
        }

		public void InitialiseMenuAfterRegistration(){
			UpdateMaster(new MenuViewController(this));
			RemoveDetail();
			kleurenBalk.BackgroundColor = _defaultColor;
		}


		public void AddLocalBox(LocalBox localBoxToAdd)
		{
			bool result = false;
	
			DialogHelper.ShowProgressDialog ("LocalBox toevoegen", "Bezig met het toevoegen van een LocalBox", async () => {
			
				result = await BusinessLayer.Instance.Authenticate (localBoxToAdd);

				if (result) {
					LocalBox_iOS.Helpers.CryptoHelper.ValidateKeyPresence (localBoxToAdd, InitialiseMenuAfterRegistration);
					AppDelegate.localBoxToRegister = null;
				}
			});
		}





        public void PincodeInstellen()
        {
			imageViewSplash.Hidden = false;

            float x = (1024 - 372) / 2;
            PincodeInstellenView instellenView = new PincodeInstellenView(new RectangleF(x, 64f, 372f, 582f));
            instellenView.OnPinFilled += PincodeIngesteld;
            _pinView = instellenView;

            View.Add(instellenView);
        }

        void PincodeIngesteld(object sender, EventArgs e)
        {
            if (_pinView != null)
            {
                _pinView.RemoveFromSuperview();
                _pinView = null;
            }

            InitialiseMenu();
        }

        public void PincodeOpgeven()
        {
            if (_master != null)
                _master.View.RemoveFromSuperview();
            if (_detail != null)
                _detail.View.RemoveFromSuperview();

            if (_pinView == null)
            {
				imageViewSplash.Hidden = false;

                float x = (1024 - 372) / 2;
                var instellenView = new PincodeInvoerenView(new RectangleF(x, 64f, 372f, 582f));
                instellenView.OnPinFilled += Resume;
                _pinView = instellenView;
                View.Add(instellenView);
            }
        }

        void Resume(object sender, EventArgs e)
        {
            if (_pinView != null)
            {
                _pinView.RemoveFromSuperview();
                _pinView = null;
            }
				
            if (AppDelegate.fileToUpload != null)
            {
				ImportFile(AppDelegate.fileToUpload);
                AppDelegate.fileToUpload = null;
            }
			InitialiseMenu();
        }


        public void PincodeOpgeven(LocalBox box)
        {
			AppDelegate.localBoxToRegister = box;

			if (_introduction != null)
            {
				_introduction.RemoveFromParentViewController();
				_introduction.View.RemoveFromSuperview();
            }

            if (_pinView == null)
            {
				imageViewSplash.Hidden = false;

                float x = (1024 - 372) / 2;
                var instellenView = new PincodeInvoerenView(new RectangleF(x, 64f, 372f, 582f));
                instellenView.OnPinFilled += PincodeIngesteld;
                _pinView = instellenView;
                View.Add(instellenView);
            }
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

