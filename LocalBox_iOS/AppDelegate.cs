using System;
using System.Linq;
using System.Collections.Generic;

using Foundation;
using UIKit;

using LocalBox_iOS.Views;
using LocalBox_Common;
using LocalBox_iOS.Helpers;

using Xamarin;

namespace LocalBox_iOS
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	[Register ("AppDelegate")]
	public partial class AppDelegate : UIApplicationDelegate
	{
		public static LocalBox localBoxToRegister;
        public static string fileToUpload;

        DateTime? _enteredBackground;
        bool openUrl = false;

		// class-level declarations
		public override UIWindow Window {
			get;
			set;
		}

        UIViewController ViewController { get; set; }

		// This method is invoked when the application is about to move from active to inactive state.
		// OpenGL applications should use this method to pause.
		public override void OnResignActivation (UIApplication application)
		{
		}
		// This method should be used to release shared resources and it should store the application state.
		// If your application supports background exection this method is called instead of WillTerminate
		// when the user quits.
		public override void DidEnterBackground (UIApplication application)
		{
            _enteredBackground = DateTime.UtcNow;
		}
		// This method is called as part of the transiton from background to active state.
		public override void WillEnterForeground (UIApplication application)
		{
		}
		// This method is called when the application is about to terminate. Save data, if needed.
		public override void WillTerminate (UIApplication application)
		{
		}

        public override bool FinishedLaunching (UIApplication application, NSDictionary launchOptions)
        {
            Window = new UIWindow (UIScreen.MainScreen.Bounds);

			//Needed to call ServicePointManager.ServerCertificateValidationCallback for all sll web requests
			Environment.SetEnvironmentVariable ("MONO_TLS_SESSION_CACHE_TIMEOUT", "0");

			//Initialize Xamarin Insights => API key can be replaced with your own Xamarin Insights API key
			Insights.Initialize("91f27e3df29c7c6f973068c514b1d64cfd895fff");


            ViewController = new HomeController();
            Window.RootViewController = ViewController;
            Window.MakeKeyAndVisible ();
            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.LightContent;
            DataLayer.Instance.EmptyDecryptedFolder();
            return true;
        }
			
        public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            openUrl = true;

			if (url.Scheme.Equals("file"))
            {
                return OpenFile(application, url, sourceApplication, annotation);
            }
            return true;
        }
			
        bool OpenFile(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
        {
            if (ViewController is HomeController)
            {
                if (DataLayer.Instance.DatabaseCreated())
                {
                    if (DataLayer.Instance.DatabaseUnlocked())
                    {
						((HomeController)ViewController).ImportFile(url.Path);
                    }
                    else
                    {
                        fileToUpload = url.Path;
                    }
                }
                else
                {
                    DialogHelper.ShowErrorDialog("Fout", "Voordat u een bestand kan toevoegen, moet u eerst een Localbox toevoegen!");
                }
            }
            return true;
        }
	}
}

