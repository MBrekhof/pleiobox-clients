using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.App;
using Android.Graphics.Drawables;
using Android.Graphics;

using LocalBox_Common;

namespace LocalBox_Droid
{
	[Activity (Label = "Selecteer een bestand om te uploaden")]			
	public class FilePickerActivity : FragmentActivity
	{

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.activity_filepicker);

			FileListFragment fileListFragment = new FileListFragment (FileListFragment.DefaultInitialDirectory);
			SupportFragmentManager.BeginTransaction().Add (Resource.Id.fragment_container_filepicker, fileListFragment).Commit ();

			//Change color of action bar
			ColorDrawable colorDrawable;
			if (!String.IsNullOrEmpty (HomeActivity.colorOfSelectedLocalBox)) {
				colorDrawable = new ColorDrawable (Color.ParseColor (HomeActivity.colorOfSelectedLocalBox));
			} else {
				colorDrawable = new ColorDrawable (Color.ParseColor (Constants.lightblue));
			}
			this.ActionBar.SetBackgroundDrawable (colorDrawable); 
		}

		protected override void OnPause ()
		{
			base.OnPause ();
		}

		protected override void OnResume ()
		{
			base.OnResume ();
		}

	}

}

