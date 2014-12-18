using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LocalBox_Common;

namespace LocalBox_Droid
{
	[Activity(Label = "LocalBox", ScreenOrientation=Android.Content.PM.ScreenOrientation.Landscape, WindowSoftInputMode = SoftInput.AdjustPan)]
	public class PinActivity : Activity
	{
		private DialogFragment dialogFragment;
		private FragmentTransaction fragmentTransaction;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.activity_pin);

			//Hide action bar
			this.ActionBar.Hide ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			//Used to determine of app should be locked
			LockHelper.SetLastActivityOpenedTime ("PinActivity");

			OpenCorrectPinDialog ();
		}


		public void OpenCorrectPinDialog()
		{
			bool databaseCreated = DataLayer.Instance.DatabaseCreated ();

			fragmentTransaction = FragmentManager.BeginTransaction();

			if (dialogFragment != null) {
				dialogFragment = null;
			}

			if(!databaseCreated) {
				dialogFragment = PinSetUpFragment.NewInstance();
				dialogFragment.Show(fragmentTransaction, "dialog");
			} else {
				dialogFragment = PinEnterFragment.NewInstance();
				dialogFragment.Show(fragmentTransaction, "dialog");
			}
		}
			
	}
}

