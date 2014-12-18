using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;

using LocalBox_Common;

namespace LocalBox_Droid
{
	public class BottomMenuFragment : ListFragment
	{
		private List<string> listValues;

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
		}

		public override View OnCreateView(LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle)
		{
			View view = layoutInflater.Inflate(Resource.Layout.fragment_menu, viewGroup, false);

			listValues = new List<string> ();
			listValues.Add ("Toevoegen");
			listValues.Add ("Vergrendelen");
			listValues.Add ("Over de app");

			ListAdapter = new BottomMenuAdapter(Activity, listValues);

			return view;
		}

		public override void OnListItemClick (ListView listView, View view, int position, long id){
			HomeActivity homeActivity = (HomeActivity)Activity;

			if (position == 0) { //Toevoegen aangeklikt
				homeActivity.ShowIntroductionDialog ();
			}
			else if (position == 1) { //vergrendelen aangeklikt
				//Lock scherm
				HomeActivity.shouldLockApp = true;
				Activity.StartActivity(typeof(PinActivity));
			} 
			else if (position == 2){ //Over de app aangeklikt
				homeActivity.ShowAboutAppDialog ();
			}

		}

	}
}

