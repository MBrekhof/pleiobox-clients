using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using LocalBox_Common;
using LocalBox_Common.Remote.Model;

namespace LocalBox_Droid
{
	public class SitesAdapter : BaseAdapter<Site>
	{
		private Activity context;
		private List<Site> sitesAll;
		public List<Identity> sitesMatch;

		public SitesAdapter (Activity context, List<Site> sites) : base ()
		{
			this.context = context;
			this.sitesAll = sites;
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override Site this [int position] {  
			get { return sitesAll [position]; }
		}
			
		public override int Count {
			get { return sitesAll.Count; }
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			View view = convertView;
			if (view == null) // no view to re-use, create new
				view = context.LayoutInflater.Inflate (Android.Resource.Layout.SimpleListItemMultipleChoice, null);

			view.SetMinimumHeight (50);

			var textView = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			textView.Text = sitesAll [position].Name;

			//Set font
			FontHelper.SetFont (textView);
			textView.SetTextSize (Android.Util.ComplexUnitType.Dip, 20);
			textView.SetTextColor (Android.Graphics.Color.Gray);

			return view;
		}
	}
}