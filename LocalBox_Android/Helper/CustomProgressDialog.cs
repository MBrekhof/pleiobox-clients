using System;
using Android.App;

namespace LocalBox_Droid
{
	public class CustomProgressDialog
	{
		ProgressDialog progressDialog;

		public void Show (Android.Content.Context context, string messageToShow)
		{
			try {
				string message;

				if (String.IsNullOrEmpty (messageToShow)) {
					message = "Laden...";
				} else {
					message = messageToShow;
				}
				if (progressDialog != null) {
					progressDialog = null;
				}

				progressDialog = new ProgressDialog (context);
				progressDialog.Indeterminate = true;
				progressDialog.SetProgressStyle (ProgressDialogStyle.Spinner);
				progressDialog.SetMessage (message);
				progressDialog.SetCancelable (false);
				progressDialog.Show ();
			} catch {
			}
		}

		public void Hide(){
			if (progressDialog != null) {
				progressDialog.Hide ();
			}
		}

	}
}

