using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;

using LocalBox_Common;

namespace LocalBox_Droid
{
	public class FileListFragment : ListFragment
	{
		public static readonly string DefaultInitialDirectory = "/sdcard";

		private FileListAdapter fileListadapter;
		private DirectoryInfo directoryInfo;
		private string currentDirectoryOpened;

		public FileListFragment(string directoryToOpen)
		{
			currentDirectoryOpened = directoryToOpen;
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			fileListadapter = new FileListAdapter(Activity, new FileSystemInfo[0]);
			ListAdapter = fileListadapter;
		}


		public override async void OnListItemClick(ListView listView, View view, int position, long id)
		{
			FileSystemInfo fileSystemInfo = fileListadapter.GetItem(position);

			if (fileSystemInfo.IsFile ()) {
				CustomProgressDialog progressDialog = new CustomProgressDialog ();
				progressDialog.Show (Activity, "Bestand uploaden. Een ogenblik geduld a.u.b.");

				try {
					//get current folder path to add file to
					int numberOfDirectoriesOpened = ExplorerFragment.openedDirectories.Count;
					string directoryNameToUploadFileTo = ExplorerFragment.openedDirectories [numberOfDirectoriesOpened - 1];

					string fullDestinationPath = System.IO.Path.Combine (directoryNameToUploadFileTo, fileSystemInfo.Name);
					bool uploadedSucceeded = await DataLayer.Instance.UploadFile (fullDestinationPath, fileSystemInfo.FullName);

					if (!uploadedSucceeded) {
						Toast.MakeText (Activity, "Er is iets fout gegaan", ToastLength.Short).Show ();
					} else {
						Toast.MakeText (Activity, "Bestand succesvol geupload", ToastLength.Short).Show ();
						
						Activity.Finish ();
					}
				} catch {
					Toast.MakeText (Activity, "Er is iets fout gegaan", ToastLength.Short).Show ();
				}
				progressDialog.Hide ();
			} else {
				// Dig into this directory, and display it's contents
				Android.Support.V4.App.FragmentTransaction fragmentTransaction = Activity.SupportFragmentManager.BeginTransaction ();
				//fragmentTransaction.SetCustomAnimations (Resource.Animation.enter, Resource.Animation.exit);

				FileListFragment fileListFragment = new FileListFragment (fileSystemInfo.FullName);
				fragmentTransaction.Replace (Resource.Id.fragment_container_filepicker, fileListFragment, "fileListFragment");

				//Add fragment to stack - needed for back button functionality
				fragmentTransaction.AddToBackStack (null);

				// Start the animated transition.
				fragmentTransaction.Commit ();
			}
			base.OnListItemClick(listView, view, position, id);
		}



		public override void OnResume()
		{
			base.OnResume();
			RefreshFilesList(currentDirectoryOpened);
		}

		public void RefreshFilesList(string directory)
		{
			IList<FileSystemInfo> visibleThings = new List<FileSystemInfo>();
			var dir = new DirectoryInfo(directory);

			try
			{
				foreach (var item in dir.GetFileSystemInfos().Where(item => item.IsVisible()))
				{
					visibleThings.Add(item);
				}
			}
			catch (Exception ex)
			{
				Log.Error("FileListFragment", "Couldn't access the directory " + directoryInfo.FullName + "; " + ex);
				Toast.MakeText(Activity, "Problem retrieving contents of " + directory, ToastLength.Long).Show();
				return;
			}

			directoryInfo = dir;

			fileListadapter.AddDirectoryContents(visibleThings);

			Log.Verbose("FileListFragment", "Displaying the contents of directory {0}.", directory);
		}
	}
}
