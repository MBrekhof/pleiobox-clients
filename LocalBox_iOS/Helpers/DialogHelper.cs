using System;
using MonoTouch.UIKit;
using System.Threading;
using MonoTouch.Foundation;

namespace LocalBox_iOS.Helpers
{
    public static class DialogHelper
    {
		public static UIActivityIndicatorView _indicatorViewLeftCorner;
		private static UIAlertView blockingDialog;

        public static void ShowProgressDialog(string title, string message, Action action) {
            UIAlertView dialog = new UIAlertView(title, message, null, null);
            dialog.Show();
			_indicatorViewLeftCorner.Hidden = false;
            action();
            dialog.DismissWithClickedButtonIndex(0, true);
        }


        public static void ShowProgressDialog(string title, string message, Action action, Action finalize) {
            UIAlertView dialog = new UIAlertView(title, message, null, null);

            dialog.Show();
			_indicatorViewLeftCorner.Hidden = false;

            ThreadPool.QueueUserWorkItem(delegate
            {
                // Run the given Async task
                action();

                // Run the completion handler on the UI thread and remove the spinner
                using (var pool = new NSAutoreleasePool())
                {
                    try
                    {
                        pool.InvokeOnMainThread(() =>
                        {
                            dialog.DismissWithClickedButtonIndex(0, true);

							_indicatorViewLeftCorner.Hidden = true;
                            finalize();
                      });
                    }
                    catch
                    {
                    }
                }
            });

        }
			
        public static void ShowErrorDialog(string title, string message){
            UIAlertView error = new UIAlertView(title, message, null, "OK");
            error.Show();
        }


		public static void HideProgressDialog()
		{
			_indicatorViewLeftCorner.Hidden = true;
		}


		public static void ShowBlockingProgressDialog(string title, string message) {
			blockingDialog = new UIAlertView(title, message, null, null);
			blockingDialog.Show();
			_indicatorViewLeftCorner.Hidden = false;
		}

		public static void HideBlockingProgressDialog()
		{
			if (blockingDialog != null) {
				blockingDialog.DismissWithClickedButtonIndex (0, true);
				blockingDialog = null;
			}
			_indicatorViewLeftCorner.Hidden = true;
		}
    }
}

