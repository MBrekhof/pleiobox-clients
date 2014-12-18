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

using LocalBox_Common;
using LocalBox_Common.Remote;

namespace LocalBox_Droid
{
	public class ShareFragment : DialogFragment
	{
		private List<Identity> foundLocalBoxUsers;
		private static List<Identity> selectedUsersToShareWith;

		private bool isNewShare;
		private string pathOfFolderToShare;
		private ShareAdapter shareAdapter;
		private ListView listViewShare;
		private Share existingShare;
		private CustomProgressDialog progressDialog = new CustomProgressDialog();

		public ShareFragment(string pathOfFolderToShare, List<Identity> foundUsers, Share shareSettings)
		{
			this.pathOfFolderToShare = pathOfFolderToShare;

			if (shareSettings == null) //Nieuwe share
			{ 
				selectedUsersToShareWith = new List<Identity> ();
				this.isNewShare = true;
				this.foundLocalBoxUsers = foundUsers;
			} 
			else //Bestaande share
			{
                selectedUsersToShareWith = shareSettings.Identities;
				this.existingShare = shareSettings;
				this.isNewShare = false;

				//Zet geselecteerde items (personen waar reeds mee geshared is) bovenaan lijst
				this.foundLocalBoxUsers = new List<Identity> ();

				foreach (Identity identity in selectedUsersToShareWith) {
					this.foundLocalBoxUsers.Add (identity);
				}
					
				foreach (Identity foundIdentity in foundUsers) 
				{
					bool found = false;

					foreach (Identity foundSelectedIdentity in selectedUsersToShareWith) {
                        if (foundIdentity.Id == foundSelectedIdentity.Id) {
							found = true;
						}
					}

					if (found == false) {
						this.foundLocalBoxUsers.Add (foundIdentity);
					}
				}
			}
		}


		public override View OnCreateView (LayoutInflater layoutInflater, ViewGroup viewGroup, Bundle bundle)
		{
			View view = layoutInflater.Inflate (Resource.Layout.fragment_share, viewGroup, false);

			listViewShare = view.FindViewById<ListView> (Resource.Id.listViewShare);
			listViewShare.ChoiceMode = ChoiceMode.Multiple;

			shareAdapter = new ShareAdapter (Activity, foundLocalBoxUsers);

			listViewShare.Adapter = shareAdapter;

			Dialog.SetTitle ("Map delen met");

			SetSelectedItemsChecked ();
		
			//Share button clicked
			Button buttonShareWithSelectedUsers = view.FindViewById<Button> (Resource.Id.button_share_with_selected_users);
			buttonShareWithSelectedUsers.Click += async delegate {
			
				ShowProgressDialog (Activity, null);

				try {
					bool shareSucceeded = false;
					bool shouldShowErrorMessage = true;

					if (isNewShare) {
						bool usersSelected = selectedUsersToShareWith.Count > 0;

						if (usersSelected) {
							shareSucceeded = await BusinessLayer.Instance.ShareFolder (pathOfFolderToShare, selectedUsersToShareWith);
						} else {
							HideProgressDialog ();
							Toast.MakeText (Activity, "U heeft geen gebruikers geselecteerd om mee te delen", ToastLength.Short).Show ();
							shouldShowErrorMessage = false;
						}
					}
					else {
						shareSucceeded = await BusinessLayer.Instance.UpdateSettingsSharedFolder (existingShare, selectedUsersToShareWith);
					}

					if (shareSucceeded) {
						HomeActivity homeActivity = (HomeActivity)Activity;
						homeActivity.HideShareDialog (isNewShare);
					}
					else {
						if(shouldShowErrorMessage){
							Toast.MakeText (Activity, "Er is iets fout gegaan bij het delen", ToastLength.Short).Show ();
						}
					}
					HideProgressDialog ();
				} 
				catch {
					HideProgressDialog ();
					Toast.MakeText (Activity, "Er is iets fout gegaan bij het delen", ToastLength.Short).Show ();
				}
			};


			//List item select event
			listViewShare.ItemClick += (object sender, Android.Widget.AdapterView.ItemClickEventArgs e) => {

				Identity selectedIdentity = shareAdapter.localBoxUsersMatch [e.Position];

				CheckedTextView item = (CheckedTextView)e.View;

				if(item.Checked)//Voeg toe aan lijst
				{
					selectedUsersToShareWith.Add(selectedIdentity);
				}
				else //Verwijder uit lijst
				{
					foreach(Identity identityToDelete in selectedUsersToShareWith){

                        if(identityToDelete.Id == selectedIdentity.Id){
							selectedUsersToShareWith.Remove(identityToDelete);
							return;
						}
					}
				}
                Console.WriteLine(selectedIdentity.Title);
			};
				

			//Search EditText
			EditText editTextSearchUser = view.FindViewById<EditText>(Resource.Id.edittext_share_user_search);
			editTextSearchUser.AfterTextChanged += (sender, args) =>
			{
				shareAdapter.Filter(editTextSearchUser.Text);

				SetSelectedItemsChecked();
			};
				
			return view;
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

		void SetSelectedItemsChecked()
		{
			listViewShare.ClearChoices ();
			
			//loop through listitems and check the selected items
			foreach (Identity showedIdentity in shareAdapter.localBoxUsersMatch) {

				foreach (Identity selectedIdentity in selectedUsersToShareWith) {
                    if (showedIdentity.Id == selectedIdentity.Id) {
						int indexOfItem = shareAdapter.localBoxUsersMatch.IndexOf (showedIdentity);
						listViewShare.SetItemChecked (indexOfItem, true);
					}
				}
			}
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
	