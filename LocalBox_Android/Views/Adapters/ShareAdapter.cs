using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

using LocalBox_Common;

namespace LocalBox_Droid
{
	public class ShareAdapter : BaseAdapter<Identity>
	{
		private Activity context;
		private List<Identity> localBoxUsersAll;
		public List<Identity> localBoxUsersMatch;

		public ShareAdapter (Activity context, List<Identity> localBoxUsers) : base ()
		{
			this.context = context;
			this.localBoxUsersAll = localBoxUsers;
			this.localBoxUsersMatch = localBoxUsers;
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override Identity this [int position] {  
			get { return localBoxUsersMatch [position]; }
		}

		public override int Count {
			get { return localBoxUsersMatch.Count; }
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			View view = convertView;
			if (view == null) // no view to re-use, create new
				view = context.LayoutInflater.Inflate (Android.Resource.Layout.SimpleListItemMultipleChoice, null);//custom_listview_item_share, null);

			view.SetMinimumHeight (50);

			var textView = view.FindViewById<TextView> (Android.Resource.Id.Text1);
            textView.Text = localBoxUsersMatch [position].Title;

			//Set font
			FontHelper.SetFont (textView);
			textView.SetTextSize (Android.Util.ComplexUnitType.Dip, 20);
			textView.SetTextColor (Android.Graphics.Color.Gray);

			return view;
		}



		public void Filter (string query)
		{
			localBoxUsersMatch = new List<Identity> ();

			foreach (Identity identity in localBoxUsersAll) 
			{
				string identityTitle = identity.Title.ToLower();

				if (identityTitle.Contains (query.ToLower())) {
					localBoxUsersMatch.Add (identity);
					Console.WriteLine ("Match found: " + identityTitle);
				}
			}
			NotifyDataSetChanged ();
		}
			
	}
}