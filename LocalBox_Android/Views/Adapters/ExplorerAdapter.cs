using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LocalBox_Common;


namespace localbox.android
{
	public class ExplorerAdapter : BaseAdapter<TreeNode> {

		Activity context;

		public List<TreeNode> foundTreeNodes { get; private set;}
		private bool adapterUsedToMoveFile;

		public ExplorerAdapter(Activity context, List<TreeNode> foundTreeNodes, bool adapterUsedToMoveFile) : base() {
			this.context = context;
			this.foundTreeNodes = foundTreeNodes;
			this.adapterUsedToMoveFile = adapterUsedToMoveFile;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override TreeNode this[int position] {  
			get { return foundTreeNodes[position]; }
		}

		public override int Count {
			get { return foundTreeNodes.Count; }
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View view = convertView;

			if (view == null) // no view to re-use, create new
				view = context.LayoutInflater.Inflate(Resource.Layout.custom_listview_item_explorer, null);

			var textView = view.FindViewById<TextView> (Resource.Id.list_item_textview_explorer);


			var imageViewLeft = view.FindViewById<ImageView> (Resource.Id.list_item_image_explorer);
			var imageViewRight = view.FindViewById<ImageView> (Resource.Id.list_item_image_explorer_special_type);

			TreeNode treeNodeToShow = foundTreeNodes [position];

			textView.Text = treeNodeToShow.Name;

			//Juiste icoon tonen bij bestandstype 
			string mimeType = MimeTypeHelper.GetMimeType (treeNodeToShow.Path);

			//LINKER ICOON LISTITEM
			if (treeNodeToShow.IsDirectory == true) {
				if (treeNodeToShow.HasKeys) { //Encrypted
					imageViewLeft.SetImageResource (Resource.Drawable.ic_type_map_versleuteld);
				} else {
					//Unencrypted?? Check if folder is subfolder of encrypted folder - then it is also encrypted..
					imageViewLeft.SetImageResource (Resource.Drawable.ic_type_map);

					if (adapterUsedToMoveFile) //Used in MoveFileFragment
					{ 
						if (!MoveFileFragment.openedFolderIsUnencrypted) {
							if (MoveFileFragment.openedDirectories.Count > 1) {
								imageViewLeft.SetImageResource (Resource.Drawable.ic_type_map_versleuteld);
							}
						}
					} 
					else { //Used in ExplorerFragment
						if (!ExplorerFragment.openedFolderIsUnencrypted) {
							if (ExplorerFragment.openedDirectories.Count > 1) {
								imageViewLeft.SetImageResource (Resource.Drawable.ic_type_map_versleuteld);
							}
						}

					}
				}
			} else if (treeNodeToShow.Type.Equals ("favorite")) {
				imageViewLeft.SetImageResource (Resource.Drawable.ic_lijst_favorieten);
			} else if (mimeType.Equals ("image/jpeg") ||
					   mimeType.Equals ("image/png")) {
				imageViewLeft.SetImageResource (Resource.Drawable.ic_type_foto);
			} else if (mimeType.Equals ("application/pdf")) {
				imageViewLeft.SetImageResource (Resource.Drawable.ic_type_p_d_f);
			} else if (mimeType.Equals ("application/zip")) {
				imageViewLeft.SetImageResource (Resource.Drawable.ic_type_gecompileerd);
			} else if (mimeType.Equals ("application/mp4")) {
				imageViewLeft.SetImageResource (Resource.Drawable.ic_type_film);
			}
			else if (mimeType.Equals ("application/vnd.ms-excel") ||
					 mimeType.Equals ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")) {
				imageViewLeft.SetImageResource (Resource.Drawable.ic_type_berekeningen);
			}
			else if (mimeType.Equals ("application/vnd.ms-powerpoint") ||
					 mimeType.Equals ("application/vnd.openxmlformats-officedocument.presentationml.presentation")) {
				imageViewLeft.SetImageResource (Resource.Drawable.ic_type_presentatie);
			}
			else if (mimeType.Equals ("application/msword") || 
					 mimeType.Equals ("application/vnd.openxmlformats-officedocument.wordprocessingml.document")){
				imageViewLeft.SetImageResource (Resource.Drawable.ic_type_tekstdocumenten);
			}
			else {
				imageViewLeft.SetImageResource (Resource.Drawable.ic_type_onbekend);
			}


			//RECHTER ICOON LISTITEM
			if (treeNodeToShow.IsFavorite == true) {
				imageViewRight.SetImageResource (Resource.Drawable.ic_ind_favorieten);
				imageViewRight.Visibility = ViewStates.Visible;
			} else if (treeNodeToShow.IsShare == true) {
				imageViewRight.SetImageResource (Resource.Drawable.ic_ind_met_mij_gedeeld);
				imageViewRight.Visibility = ViewStates.Visible;
			} else if (treeNodeToShow.IsShared == true) {
				imageViewRight.SetImageResource (Resource.Drawable.ic_ind_zelf_gedeeld);
				imageViewRight.Visibility = ViewStates.Visible;
			}
			else {
				imageViewRight.Visibility = ViewStates.Invisible;
			}
		
			//Set font
			FontHelper.SetFont (textView);

			return view;
		}

	}
}
