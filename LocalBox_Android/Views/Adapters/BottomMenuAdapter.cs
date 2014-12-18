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
	public class BottomMenuAdapter : BaseAdapter<string> {

		private Activity context;
		private List<string> foundStrings;

		public BottomMenuAdapter(Activity context, List<string> foundStrings) : base() {
			this.context = context;
			this.foundStrings = foundStrings;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override string this[int position] {  
			get { return foundStrings[position]; }
		}

		public override int Count {
			get { return foundStrings.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView;
			if (view == null) // no view to re-use, create new
				view = context.LayoutInflater.Inflate(Resource.Layout.custom_listview_item_menu_bottom, null);

			var textView = view.FindViewById<TextView> (Resource.Id.list_item_textview_menu_bottom);
			textView.Text = foundStrings[position];

			var imageView = view.FindViewById<ImageView> (Resource.Id.list_item_image_menu_bottom);

			if (position == 0) {
				imageView.SetImageResource (Resource.Drawable.ic_bottom_toevoegen_lb);
			}
			else if (position == 1) {
				imageView.SetImageResource (Resource.Drawable.ic_bottom_vergrendel);
			} else if (position == 2) {
				imageView.SetImageResource (Resource.Drawable.ic_bottom_over_de_app);
			}
				
			//Set font
			FontHelper.SetFont (textView);

			return view;
		}
	}
}
