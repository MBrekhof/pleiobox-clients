using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

using Android.Graphics;

using LocalBox_Common;
using Xamarin;

namespace LocalBox_Droid
{
	public class MoveFileFragment : DialogFragment
	{
		public static List<string> openedDirectories = new List<string> ();
		public static bool openedFolderIsUnencrypted = true;

		private CustomProgressDialog progressDialog = new CustomProgressDialog();
		private ListView listViewMoveFile;
		private Button buttonMoveFileToSelectedFolder;
		private ImageButton buttonBack;
		private ExplorerAdapter explorerAdapter;
		private List<TreeNode> foundDirectoryTreeNodes;
		private List<TreeNode> selectedTreeNodes;
		private	TreeNode treeNodeToMove;
		private string pathOfCurrentOpenedFolder;
		private HomeActivity parentActivity;

		public MoveFileFragment(List<TreeNode> foundDirectoryTreeNodes, TreeNode treeNodeToMove, HomeActivity parentActivity)
		{
			this.foundDirectoryTreeNodes = foundDirectoryTreeNodes;
			this.treeNodeToMove = treeNodeToMove;
			this.parentActivity = parentActivity;
			selectedTreeNodes = new List<TreeNode> ();
		}

		public override View OnCreateView (LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle)
		{
			Dialog.SetTitle (treeNodeToMove.Name);

			View view = layoutInflater.Inflate (Resource.Layout.fragment_move_file, viewGroup, false);

			//Set color
			LinearLayout customActionBar = view.FindViewById<LinearLayout> (Resource.Id.custom_actionbar_fragment_move_file);
			customActionBar.SetBackgroundColor (Color.ParseColor (HomeActivity.colorOfSelectedLocalBox));

			buttonMoveFileToSelectedFolder = view.FindViewById<Button> (Resource.Id.button_move_file_to_selected_folder);
			buttonMoveFileToSelectedFolder.Enabled = false;
			buttonMoveFileToSelectedFolder.Click += delegate 
			{
				MoveFileToOpenedFolder();
			};
				
			openedDirectories.Add ("/");

			explorerAdapter = new ExplorerAdapter (Activity, foundDirectoryTreeNodes, true);

			listViewMoveFile = view.FindViewById<ListView> (Resource.Id.listViewMoveFile);
			listViewMoveFile.Adapter = explorerAdapter;  
			listViewMoveFile.ItemClick += async (object sender, Android.Widget.AdapterView.ItemClickEventArgs e) => { //List item select event

				ShowProgressDialog(Activity, null);

				try{
					List<TreeNode>foundDirectories 	= new List<TreeNode>();

					pathOfCurrentOpenedFolder = explorerAdapter.foundTreeNodes[e.Position].Path;

					TreeNode selectedTreeNode = await DataLayer.Instance.GetFolder (pathOfCurrentOpenedFolder);

					foreach(TreeNode foundTreeNode in selectedTreeNode.Children)
					{
						if(foundTreeNode.IsDirectory)
						{
							foundDirectories.Add(foundTreeNode);
						}
					}
					openedDirectories.Add (selectedTreeNode.Path);
					selectedTreeNodes.Add(selectedTreeNode);
					buttonBack.Visibility = ViewStates.Visible;

					//Used to determine folder icon
					if(openedDirectories.Count == 2){
						if(selectedTreeNode.HasKeys){
							openedFolderIsUnencrypted = false;
						}else{
							openedFolderIsUnencrypted = true;
						}
					}

					explorerAdapter = new ExplorerAdapter (Activity, foundDirectories, true);
					listViewMoveFile.Adapter = explorerAdapter;

					Activity.RunOnUiThread(new Action(()=> { 
						explorerAdapter.NotifyDataSetChanged();
						buttonMoveFileToSelectedFolder.Enabled = true;
						HideProgressDialog();

						Console.WriteLine("Current opened folder; " + pathOfCurrentOpenedFolder);
					}));
				}
				catch (Exception ex){
					Insights.Report(ex);
					HideProgressDialog();
					Toast.MakeText (Activity, "Er is iets fout gegaan. Probeer het a.u.b. opnieuw", ToastLength.Short).Show ();
				}
			};

			
			buttonBack = view.FindViewById<ImageButton> (Resource.Id.imagebutton_back_move_file);
			buttonBack.Visibility = ViewStates.Invisible;
			buttonBack.Click += delegate 
			{
				int directoriesOpened = selectedTreeNodes.Count;

				if(directoriesOpened > 0)
				{
					selectedTreeNodes.RemoveAt(directoriesOpened - 1);

					if(selectedTreeNodes.Count == 0){
						explorerAdapter = new ExplorerAdapter (Activity, foundDirectoryTreeNodes, true);
						listViewMoveFile.Adapter = explorerAdapter;

						buttonBack.Visibility = ViewStates.Invisible;

						//Set path of destination folder
						pathOfCurrentOpenedFolder = "/";

						buttonMoveFileToSelectedFolder.Enabled = false;
					}
					else{
						List<TreeNode>foundDirectories 	= new List<TreeNode>();

						//Set path of destination folder
						pathOfCurrentOpenedFolder = selectedTreeNodes[directoriesOpened - 2].Path;

						foreach(TreeNode foundTreeNode in selectedTreeNodes[directoriesOpened - 2].Children)
						{
							if(foundTreeNode.IsDirectory)
							{
								foundDirectories.Add(foundTreeNode);
							}
						}
						explorerAdapter = new ExplorerAdapter (Activity, foundDirectories, true);
						listViewMoveFile.Adapter = explorerAdapter;
					}
				}

				//UPDATE UI
				Activity.RunOnUiThread(new Action(()=> { 
					explorerAdapter.NotifyDataSetChanged();
				}));

				int count = openedDirectories.Count;
				openedDirectories.RemoveAt(count -1);
			};
			return view;
		}


		private async void MoveFileToOpenedFolder ()
		{
			bool uploadedSucceeded = false;
			bool deleteSucceeded = false;

			ShowProgressDialog (Activity, "Bestand verplaatsen. \nEen ogenblik geduld a.u.b.");

			try{
				string destinationPath = System.IO.Path.Combine(pathOfCurrentOpenedFolder, treeNodeToMove.Name);

				if(treeNodeToMove.Path.Equals(destinationPath)){
					HideProgressDialog();
					Toast.MakeText (Activity, "Gekozen map mag niet hetzelfde zijn als de oorspronkelijke locatie van het bestand", ToastLength.Long).Show ();
				}
				else{
					//decrypt file to filesystem and get path of decrypted file
					string filePath = await DataLayer.Instance.GetFilePath (treeNodeToMove.Path);

					//upload file to selected destination folder
					uploadedSucceeded = await DataLayer.Instance.UploadFile (destinationPath, filePath);

					//remove file from old destination
					if(uploadedSucceeded){
						deleteSucceeded = await DataLayer.Instance.DeleteFileOrFolder (treeNodeToMove.Path);

						//Update folder where file is moved to
						DataLayer.Instance.GetFolder(pathOfCurrentOpenedFolder, true);
					}

					//close this dialog en show succeeded message
					HideProgressDialog();

					if(deleteSucceeded){
						parentActivity.HideMoveFileDialog();
					}else{
						Toast.MakeText (Activity, "Er is iets fout gegaan. Probeer het a.u.b. opnieuw", ToastLength.Short).Show ();
					}
				}
			}
			catch (Exception ex){
				Insights.Report(ex);
				HideProgressDialog ();
				Toast.MakeText (Activity, "Er is iets fout gegaan. Probeer het a.u.b. opnieuw", ToastLength.Short).Show ();
			}
		}

		public override void OnViewCreated (View view, Bundle savedInstanceState)
		{
			base.OnViewCreated (view, savedInstanceState);
		}

		public override void OnStop ()
		{
			HideProgressDialog ();
			base.OnStop ();
		}

		private void ShowProgressDialog (Android.Content.Context context, string textToShow)
		{
			Activity.RunOnUiThread (new Action (() => { 
				progressDialog.Show (context, textToShow);
			}));
		}

		private void HideProgressDialog ()
		{
			if (progressDialog != null) {
				Activity.RunOnUiThread (new Action (() => { 
					progressDialog.Hide ();
				}));
			}
		}

	}
}

