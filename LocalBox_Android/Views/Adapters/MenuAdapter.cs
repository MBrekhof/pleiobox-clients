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

namespace localbox.android
{
	public class MenuAdapter : BaseAdapter<LocalBox> 
	{
		private Activity context;
		private List<LocalBox> foundLocalBoxes;
	
		public MenuAdapter(Activity context, List<LocalBox> foundLocalBoxes) : base() {
			this.context = context;
			this.foundLocalBoxes = foundLocalBoxes;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override LocalBox this[int position] {  
			get { return foundLocalBoxes[position]; }
		}

		public override int Count {
			get { return foundLocalBoxes.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView;
			if (view == null) // no view to re-use, create new
				view = context.LayoutInflater.Inflate(Resource.Layout.custom_listview_item_menu, null);

			var textView = view.FindViewById<TextView> (Resource.Id.list_item_textview_menu);
			textView.Text = foundLocalBoxes[position].Name;

			//Set font
			FontHelper.SetFont (textView);

			return view;
		}

		public void SetFoundLocalBoxes(List<LocalBox> foundLocalBoxes){
			this.foundLocalBoxes = foundLocalBoxes;
		}
	}
}