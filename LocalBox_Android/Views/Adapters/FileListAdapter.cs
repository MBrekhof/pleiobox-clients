using System.Collections.Generic;
using System.IO;
using System.Linq;

using Android.Content;
using Android.Views;
using Android.Widget;

namespace LocalBox_Droid
{
	public class FileListAdapter : ArrayAdapter<FileSystemInfo>
	{
		private readonly Context context;

		public FileListAdapter(Context context, IList<FileSystemInfo> fileSystemInfo)
			: base(context, Resource.Layout.custom_listview_filepicker, Android.Resource.Id.Text1, fileSystemInfo)
		{
			this.context = context;
		}


		public void AddDirectoryContents(IEnumerable<FileSystemInfo> directoryContents)
		{
			Clear();
			// Notify the _adapter that things have changed or that there is nothing 
			// to display.
			if (directoryContents.Any())
			{
				#if __ANDROID_11__
				// .AddAll was only introduced in API level 11 (Android 3.0). 
				// If the "Minimum Android to Target" is set to Android 3.0 or 
				// higher, then this code will be used.
				AddAll(directoryContents.ToArray());
				#else
				// This is the code to use if the "Minimum Android to Target" is
				// set to a pre-Android 3.0 API (i.e. Android 2.3.3 or lower).
				lock (this)
					foreach (var entry in directoryContents)
					{
						Add(entry);
					}
				#endif

				NotifyDataSetChanged();
			}
			else
			{
				NotifyDataSetInvalidated();
			}
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			var fileSystemEntry = GetItem(position);

			View view = convertView;
			if (view == null) // no view to re-use, create new
				view = context.GetLayoutInflater().Inflate(Resource.Layout.custom_listview_filepicker, parent, false);

			var textView = view.FindViewById<TextView>(Resource.Id.textview_listitem_filepicker);
			textView.Text = fileSystemEntry.Name;

			var imageView = view.FindViewById<ImageView> (Resource.Id.imageview_listitem_filepicker);


			//Juiste icoon tonen bij bestandstype
			if (fileSystemEntry.IsDirectory() == true) {
				imageView.SetImageResource (Resource.Drawable.folder_filepicker);
			} else {
				imageView.SetImageResource (Resource.Drawable.file_filepicker);
			} 

			//Set font
			FontHelper.SetFont (textView);

			return view;
		}
	}
}
