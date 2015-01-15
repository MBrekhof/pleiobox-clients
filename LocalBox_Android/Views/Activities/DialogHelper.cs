using System;
using System.Threading;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;
using Android.Content;
using Android.Views.InputMethods;
using Android.InputMethodServices;

using LocalBox_Common;

using Xamarin;
using System.Net;

namespace LocalBox_Droid
{
	public class DialogHelperForHomeActivity
	{
		HomeActivity homeActivity;

		public DialogHelperForHomeActivity (HomeActivity homeActivity)
		{
			this.homeActivity = homeActivity;
		}



		public void ShowCertificateMismatchDialog()
		{
			var builder = new AlertDialog.Builder(homeActivity)
				.SetTitle("Waarschuwing")
				.SetMessage("De identiteit van de server kan niet goed vastgesteld worden. " +
					"Maakt u gebruik van een vertrouwd netwerk om de identiteit " +
					"extra te controleren?")
				.SetPositiveButton("Ja", async (s, args) =>
					{
						//Get new certificate from server
						bool newCertificateIsValid = CertificateHelper.RenewCertificateForLocalBox(DataLayer.Instance.GetSelectedOrDefaultBox());

						if(newCertificateIsValid){
							homeActivity.ShowToast ("Controle met succes uitgevoerd. U kunt weer verder werken.");
						}else {
							homeActivity.ShowToast ("Dit netwerk is niet te vertrouwen.");
						}
					})
				.SetNegativeButton("Nee", (s, args) =>{ });
			builder.Show ();
		}



		public void ShowShareFileDatePicker (string pathToNewFileShare)
		{
			homeActivity.HideProgressDialog ();

			var selectedExpirationDate = DateTime.Now.AddDays (7);//Selectie standaard op 7 dagen na vandaag
			var picker = new DatePickerDialog (homeActivity, (object sender, DatePickerDialog.DateSetEventArgs e) => {
				selectedExpirationDate = e.Date;

				if (selectedExpirationDate.Date <= DateTime.Now.Date) {
					homeActivity.HideProgressDialog ();
					Toast.MakeText (Android.App.Application.Context, "Gekozen vervaldatum moet na vandaag zijn. Probeer het a.u.b. opnieuw.", ToastLength.Long).Show ();
				} else {
					ThreadPool.QueueUserWorkItem (async o => {
						PublicUrl createdPublicUrl = await DataLayer.Instance.CreatePublicFileShare (pathToNewFileShare, selectedExpirationDate);

						if (createdPublicUrl != null) {
							//Open e-mail intent
							var emailIntent = new Intent (Android.Content.Intent.ActionSend);

							string bodyText = "Mijn gedeelde bestand: \n" +
							                  createdPublicUrl.publicUri + "\n \n" +
							                  "Deze link is geldig tot: " + selectedExpirationDate.ToString ("dd-MM-yyyy");

							emailIntent.PutExtra (Android.Content.Intent.ExtraSubject, "Publieke URL naar gedeeld LocalBox bestand");
							emailIntent.PutExtra (Android.Content.Intent.ExtraText, bodyText);
							emailIntent.SetType ("message/rfc822");
							homeActivity.RunOnUiThread (() => {
								homeActivity.HideProgressDialog ();

								homeActivity.StartActivity (emailIntent);
							});
						} else {
							homeActivity.HideProgressDialog ();
							Toast.MakeText (Android.App.Application.Context, "Bestand delen mislukt", ToastLength.Short).Show ();
						}
					});
				}


			}, selectedExpirationDate.Year, selectedExpirationDate.Month - 1, selectedExpirationDate.Day);
			picker.SetTitle ("Selecteer vervaldatum:");
			picker.Show ();
		}





		public void ShowIntroductionDialog ()
		{
			var alertDialogIntro = new AlertDialog.Builder (homeActivity);
			alertDialogIntro.SetView (homeActivity.LayoutInflater.Inflate (Resource.Layout.fragment_introduction, null));

			alertDialogIntro.SetPositiveButton ("Start de registratie van een nieuwe LocalBox", delegate { 
				ShowOpenUrlDialog();
				alertDialogIntro.Dispose();
			});
			alertDialogIntro.Create ().Show ();
		}


		public void ShowAboutAppDialog ()
		{
			Android.App.FragmentTransaction fragmentTransaction;
			fragmentTransaction = homeActivity.FragmentManager.BeginTransaction ();

			Android.App.DialogFragment dialogFragmentAboutApp;
			AboutAppFragment aboutFragment = new AboutAppFragment ();

			dialogFragmentAboutApp = aboutFragment;
			dialogFragmentAboutApp.Show (fragmentTransaction, "aboutdialog");
		}


		public async void ShowShareDialog (string pathOfFolderToShare, bool alreadyShared)
		{
			homeActivity.ShowProgressDialog (null);
			try {
				Android.App.FragmentTransaction fragmentTransaction;
				fragmentTransaction = homeActivity.FragmentManager.BeginTransaction ();

				List<Identity> localBoxUsers = await DataLayer.Instance.GetLocalboxUsers();
				Share shareSettings = null;

				if (alreadyShared) {//Existing share
					//Get share settings
					shareSettings = await BusinessLayer.Instance.GetShareSettings (pathOfFolderToShare);
				}

				ShareFragment shareFragment = new ShareFragment (pathOfFolderToShare, localBoxUsers, shareSettings);
				homeActivity.dialogFragmentShare = shareFragment;

				homeActivity.HideProgressDialog ();
				if (localBoxUsers.Count > 0) {
					homeActivity.dialogFragmentShare.Show (fragmentTransaction, "sharedialog");
				} else {
					homeActivity.ShowToast("Geen gebruikers gevonden om mee te kunnen delen");
				}
			} catch (Exception ex){
				Insights.Report(ex);
				homeActivity.HideProgressDialog ();
			}
		}


		public void HideShareDialog (bool isNewShare)
		{
			homeActivity.dialogFragmentShare.Dismiss ();

			if (isNewShare) {
				homeActivity.ShowToast ("Map succesvol gedeeld");
			} else {
				homeActivity.ShowToast ("Deel instellingen succesvol gewijzigd");
			}
			homeActivity.RefreshExplorerFragmentData ();
		}


		public async void ShowMoveFileDialog(TreeNode treeNodeToMove)
		{
			Console.WriteLine ("Bestand om te verplaatsen: " + treeNodeToMove.Name);
			homeActivity.ShowProgressDialog (null);
			try {
				Android.App.FragmentTransaction fragmentTransaction;
				fragmentTransaction = homeActivity.FragmentManager.BeginTransaction ();

				List<TreeNode>foundDirectoryTreeNodes 	= new List<TreeNode>();
				TreeNode rootTreeNode 			= await DataLayer.Instance.GetFolder ("/");

				foreach(TreeNode foundTreeNode in rootTreeNode.Children)
				{
					if(foundTreeNode.IsDirectory)
					{
						foundDirectoryTreeNodes.Add(foundTreeNode);
					}
				}
				MoveFileFragment moveFileFragment = new MoveFileFragment(foundDirectoryTreeNodes, treeNodeToMove, homeActivity);
				homeActivity.dialogFragmentMoveFile = moveFileFragment;

				homeActivity.HideProgressDialog ();

				if (foundDirectoryTreeNodes.Count > 0) {
					homeActivity.dialogFragmentMoveFile.Show (fragmentTransaction, "movefiledialog");
				} else {
					homeActivity.ShowToast("Geen mappen gevonden om bestand naar te verplaatsen");
				}
			} catch (Exception ex){
				Insights.Report(ex);
				homeActivity.HideProgressDialog ();
				homeActivity.ShowToast ("Er is iets fout gegaan bij het ophalen van mappen. \nProbeer het a.u.b. opnieuw");
			}
		}

		public void HideMoveFileDialog ()
		{
			homeActivity.dialogFragmentMoveFile.Dismiss ();
			homeActivity.ShowToast("Bestand succesvol verplaatst");
			homeActivity.RefreshExplorerFragmentData ();
		}
			


		public void ShowNewFolderDialog ()
		{
			LayoutInflater factory = LayoutInflateHelper.GetLayoutInflater (homeActivity);

			View viewNewFolder = factory.Inflate (Resource.Layout.dialog_new_folder, null);

			EditText editTextFolderName = (EditText)viewNewFolder.FindViewById<EditText> (Resource.Id.editText_dialog_folder_name);          

			//Build the dialog
			var dialogBuilder = new AlertDialog.Builder (homeActivity);
			dialogBuilder.SetTitle (Resource.String.folder_new);
			dialogBuilder.SetView (viewNewFolder);
			dialogBuilder.SetPositiveButton (Resource.String.add, (EventHandler<DialogClickEventArgs>)null);
			dialogBuilder.SetNegativeButton (Resource.String.cancel, (EventHandler<DialogClickEventArgs>)null);

			var dialog = dialogBuilder.Create ();
			dialog.Show ();

			//Get the buttons
			var buttonAddFolder = dialog.GetButton ((int)DialogButtonType.Positive);
			var buttonCancel = dialog.GetButton ((int)DialogButtonType.Negative);

			buttonAddFolder.Click += async(sender, args) => {
				if (String.IsNullOrEmpty (editTextFolderName.Text)) {
					homeActivity.ShowToast("Naam is niet ingevuld");
				} else {
					homeActivity.ShowProgressDialog("Map wordt aangemaakt. Een ogenblik geduld a.u.b.");
					try{
						int numberOfDirectoriesOpened = ExplorerFragment.openedDirectories.Count;
						string directoryNameToUploadFileTo = ExplorerFragment.openedDirectories [numberOfDirectoriesOpened - 1];

						bool addedSuccesfully = (await DataLayer.Instance.CreateFolder (System.IO.Path.Combine (directoryNameToUploadFileTo, (editTextFolderName.Text))));

						dialog.Dismiss ();

						if (!addedSuccesfully) {
							homeActivity.HideProgressDialog();
							homeActivity.ShowToast("Toevoegen map mislukt. Probeer het a.u.b. opnieuw");
						} else {
							homeActivity.HideProgressDialog();
							homeActivity.ShowToast("Map succesvol toegevoegd");

							//Refresh data
							homeActivity.RefreshExplorerFragmentData();
						}
					}catch (Exception ex){
						Insights.Report(ex);
						homeActivity.HideProgressDialog();
						homeActivity.ShowToast("Toevoegen map mislukt. Probeer het a.u.b. opnieuw");
					}
				}
			};
			buttonCancel.Click += (sender, args) => {
				dialog.Dismiss ();
			};
		}






		public void ShowOpenUrlDialog ()
		{
			LayoutInflater factory = LayoutInflateHelper.GetLayoutInflater (homeActivity);
			View viewNewFolder = factory.Inflate (Resource.Layout.dialog_open_url, null);

			EditText editTextUrl = (EditText)viewNewFolder.FindViewById<EditText> (Resource.Id.editText_dialog_open_url);          
			editTextUrl.SetHint (Resource.String.yourlocalboxplaceholder);

			var dialogBuilder = new AlertDialog.Builder (homeActivity);
			dialogBuilder.SetTitle ("Nieuwe LocalBox");
			dialogBuilder.SetView (viewNewFolder);
			dialogBuilder.SetPositiveButton ("Open URL", (EventHandler<DialogClickEventArgs>)null);
			dialogBuilder.SetNegativeButton (Resource.String.cancel, (EventHandler<DialogClickEventArgs>)null);
			var dialog = dialogBuilder.Create ();
			dialog.Show ();

			var buttonCancel = dialog.GetButton ((int)DialogButtonType.Negative);
			var buttonOpenUrl = dialog.GetButton ((int)DialogButtonType.Positive);
			buttonOpenUrl.Click += async (sender, args) => {
				if (String.IsNullOrEmpty (editTextUrl.Text)) {
					homeActivity.ShowToast("URL is niet ingevuld");
				} else {

					//Reset certificate validation check - otherwise it will cause errors if there is an active certificate pinning enabled
					ServicePointManager.ServerCertificateValidationCallback = (p1, p2, p3, p4) => true;

					if(editTextUrl.Text.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase))
					{
						ShowHttpWarningDialog(editTextUrl.Text, dialog);

						//Dismiss keyboard
						InputMethodManager manager = (InputMethodManager) homeActivity.GetSystemService(Context.InputMethodService);
						manager.HideSoftInputFromWindow(editTextUrl.WindowToken, 0);

						//Dismiss dialog
						dialog.Dismiss();
					}
					else if(editTextUrl.Text.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
					{

						var certificateStatus = await CertificateHelper.GetCertificateStatusForUrl (editTextUrl.Text);

						if (certificateStatus == CertificateValidationStatus.Valid) {
							homeActivity.ShowRegisterLocalBoxDialog(editTextUrl.Text, false);
							dialog.Dismiss();
						}
						else if(certificateStatus == CertificateValidationStatus.ValidWithErrors)
						{
							homeActivity.ShowRegisterLocalBoxDialog(editTextUrl.Text, true);
							dialog.Dismiss();
						}
						else if (certificateStatus == CertificateValidationStatus.SelfSigned) {
							ShowSelfSignedCertificateDialog(editTextUrl.Text, dialog);
						} 
						else if (certificateStatus == CertificateValidationStatus.Invalid) {
							ShowInvalidCertificateDialog (editTextUrl.Text, dialog);						
						} 
						else {
							ShowErrorRegisterDialog(editTextUrl.Text, dialog);
						}
					}
					else {
						homeActivity.ShowToast("Het opgegeven webadres dient met https:// of http:// te beginnen.");
					}
				}
			};
			buttonCancel.Click += (sender, args) => {
				dialog.Dismiss ();
			};
		}



		public void ShowHttpWarningDialog(string urlToOpen, AlertDialog urlDialog)
		{
			var dialogAlert = (new AlertDialog.Builder (homeActivity)).Create ();
			dialogAlert.SetIcon (Android.Resource.Drawable.IcDialogAlert);
			dialogAlert.SetTitle ("Waarschuwing");
			dialogAlert.SetMessage ("U heeft een http webadres opgegeven. Weet u zeker dat u een onbeveiligde verbinding wilt opzetten?");
			dialogAlert.SetButton2 ("Cancel", (s, ev) => { dialogAlert.Dismiss(); });
			dialogAlert.SetButton ("Ga verder", (s, ev) => {
				homeActivity.ShowRegisterLocalBoxDialog(urlToOpen, false);
				urlDialog.Dismiss ();
				dialogAlert.Dismiss();
			});
			dialogAlert.Show ();
		}


		public void ShowSelfSignedCertificateDialog(string urlToOpen, AlertDialog urlDialog)
		{
			var dialogAlert = (new AlertDialog.Builder (homeActivity)).Create ();
			dialogAlert.SetIcon (Android.Resource.Drawable.IcDialogAlert);
			dialogAlert.SetTitle ("Waarschuwing");
			dialogAlert.SetMessage ("U heeft een webadres opgegeven met een ssl certificaat welke niet geverifieerd is. Weet u zeker dat u wilt doorgaan?");
			dialogAlert.SetButton2 ("Cancel", (s, ev) => { dialogAlert.Dismiss(); });
			dialogAlert.SetButton ("Ga verder", (s, ev) => {
				homeActivity.ShowRegisterLocalBoxDialog(urlToOpen, true);
				urlDialog.Dismiss ();
				dialogAlert.Dismiss();
			});
			dialogAlert.Show ();
		}


		public void ShowInvalidCertificateDialog(string urlToOpen, AlertDialog urlDialog)
		{
			var dialogAlert = (new AlertDialog.Builder (homeActivity)).Create ();
			dialogAlert.SetIcon (Android.Resource.Drawable.IcDialogAlert);
			dialogAlert.SetTitle ("Error");
			dialogAlert.SetMessage ("U heeft een webadres opgegeven met een ongeldig SSL certificaat. U kunt hierdoor geen verbinding maken met de betreffende LocalBox");
			dialogAlert.SetButton ("OK", (s, ev) => {
				dialogAlert.Dismiss();
			});
			dialogAlert.Show ();
		}

		public void ShowErrorRegisterDialog(string urlToOpen, AlertDialog urlDialog)
		{
			var dialogAlert = (new AlertDialog.Builder (homeActivity)).Create ();
			dialogAlert.SetIcon (Android.Resource.Drawable.IcDialogAlert);
			dialogAlert.SetTitle ("Error");
			dialogAlert.SetMessage ("Er is een fout opgetreden. Controleer de verbinding en webadres en probeer het a.u.b. nogmaals");
			dialogAlert.SetButton ("OK", (s, ev) => {
				dialogAlert.Dismiss();
			});
			dialogAlert.Show ();
		}



		//Register new LocalBox part 2	
		public void ShowRegisterLocalBoxDialog (string urlToOpen, bool ignoreSslError)
		{
			if (urlToOpen.IndexOf("register_app", StringComparison.OrdinalIgnoreCase) < 0) //url misses '/register_app' at the end
			{
				if (urlToOpen.EndsWith ("/")) {	//input url is like "www.mylocalbox.nl/"
					urlToOpen = urlToOpen + "register_app";
				} 
				else {	//input url is like "www.mylocalbox.nl"
					urlToOpen = urlToOpen + "/register_app";
				}
			}
			Android.App.FragmentTransaction fragmentTransaction;
			fragmentTransaction = homeActivity.FragmentManager.BeginTransaction ();

			Android.App.DialogFragment dialogFragmentRegisterLocalBox;
			RegisterLocalBoxFragment registerLocalBoxFragment = new RegisterLocalBoxFragment (urlToOpen, ignoreSslError);

			dialogFragmentRegisterLocalBox = registerLocalBoxFragment;
			dialogFragmentRegisterLocalBox.Show (fragmentTransaction, "registerlocalboxdialog");
		}

		//Register new LocalBox part 3
		public async void AddLocalBox(LocalBox lbToAdd)
		{
			bool result = false;

			homeActivity.ShowProgressDialog ("LocalBox laden...");

			result = await BusinessLayer.Instance.Authenticate (lbToAdd);

			if (result) {
				if (lbToAdd.HasCryptoKeys && !lbToAdd.HasPassPhrase) {
					homeActivity.EnterPassphrase (lbToAdd);
				} else {
					homeActivity.SetUpPassphrase (lbToAdd);
				}
			}

			homeActivity.HideProgressDialog ();
		}

		//Register new LocalBox part 4
		public void SetUpPassphrase (LocalBox localBox)
		{
			LayoutInflater factory = LayoutInflateHelper.GetLayoutInflater (homeActivity);
			View viewNewPhrase = factory.Inflate (Resource.Layout.dialog_new_passphrase, null);

			EditText editNewPassphrase = (EditText)viewNewPhrase.FindViewById<EditText> (Resource.Id.editText_dialog_new_passphrase);          
			EditText editNewPassphraseVerify = (EditText)viewNewPhrase.FindViewById<EditText> (Resource.Id.editText_dialog_new_passphrase_verify);

			var dialogBuilder = new AlertDialog.Builder (homeActivity);
			dialogBuilder.SetTitle ("Passphrase");
			dialogBuilder.SetView (viewNewPhrase);
			dialogBuilder.SetPositiveButton ("OK", (EventHandler<DialogClickEventArgs>)null);
			dialogBuilder.SetNegativeButton (Resource.String.cancel, (EventHandler<DialogClickEventArgs>)null);
			var dialog = dialogBuilder.Create ();
			dialog.Show ();

			var buttonCancel = dialog.GetButton ((int)DialogButtonType.Negative);
			var buttonAddPassphrase = dialog.GetButton ((int)DialogButtonType.Positive);
			buttonAddPassphrase.Click += async (sender, args) => {
				string passphraseOne = editNewPassphrase.Text;
				string passphraseTwo = editNewPassphraseVerify.Text;

				if (String.IsNullOrEmpty (passphraseOne)) {
					homeActivity.ShowToast("Passphrase is niet ingevuld");
				} else if (String.IsNullOrEmpty (passphraseTwo)) {
					homeActivity.ShowToast("U dient uw ingevoerde passphrase te verifieren");
				} else {
					if (!passphraseOne.Equals (passphraseTwo)) {
						homeActivity.ShowToast("De ingevoerde passphrases komen niet overeen. Corrigeer dit a.u.b.");
					} else {
						try 
						{
							homeActivity.ShowProgressDialog ("Passphrase aanmaken. Dit kan enige tijd in beslag nemen. Een ogenblik geduld a.u.b.");
							bool newPassphraseSucceeded = await BusinessLayer.Instance.SetPublicAndPrivateKey (localBox, passphraseOne);
							homeActivity.HideProgressDialog();

							if (!newPassphraseSucceeded) {
								homeActivity.ShowToast("Passphrase instellen mislukt. Probeer het a.u.b. opnieuw");
							} 
							else {
								dialog.Dismiss ();
								homeActivity.ShowToast("LocalBox succesvol geregistreerd");

								homeActivity.menuFragment.UpdateLocalBoxes ();
								SplashActivity.intentData = null;
							}
						} 
						catch (Exception ex){
							Insights.Report(ex);
							homeActivity.HideProgressDialog ();
							homeActivity.ShowToast("Passphrase instellen mislukt. Probeer het a.u.b. opnieuw");
						}
					}
				}
			};
			buttonCancel.Click += (sender, args) => {
				DataLayer.Instance.DeleteLocalBox (localBox.Id);
				homeActivity.menuFragment.UpdateLocalBoxes ();
				dialog.Dismiss ();
			};
		}


		//Register new LocalBox part 4
		public void EnterPassphrase (LocalBox localBox)
		{
			LayoutInflater factory = LayoutInflateHelper.GetLayoutInflater (homeActivity);
			View viewNewPhrase = factory.Inflate (Resource.Layout.dialog_enter_passphrase, null);
			EditText editEnterPassphrase = (EditText)viewNewPhrase.FindViewById<EditText> (Resource.Id.editText_dialog_enter_passphrase);          

			var dialogBuilder = new AlertDialog.Builder (homeActivity);
			dialogBuilder.SetTitle ("Passphrase");
			dialogBuilder.SetView (viewNewPhrase);
			dialogBuilder.SetPositiveButton ("OK", (EventHandler<DialogClickEventArgs>)null);
			dialogBuilder.SetNegativeButton (Resource.String.cancel, (EventHandler<DialogClickEventArgs>)null);
			var dialog = dialogBuilder.Create ();
			dialog.Show ();

			var buttonCancel = dialog.GetButton ((int)DialogButtonType.Negative);
			var buttonAddPassphrase = dialog.GetButton ((int)DialogButtonType.Positive);
			buttonAddPassphrase.Click += async (sender, args) => {
				string passphrase = editEnterPassphrase.Text;

				if (String.IsNullOrEmpty (passphrase)) {
					homeActivity.ShowToast("Passphrase is niet ingevuld");
				} 
				else {
					try {
						homeActivity.ShowProgressDialog ("Uw passphrase controleren. Een ogenblik geduld a.u.b.");
						bool correctPassphraseEntered = await BusinessLayer.Instance.ValidatePassPhrase (localBox, passphrase);
						homeActivity.HideProgressDialog();

						if (!correctPassphraseEntered) {
							homeActivity.ShowToast("Passphrase onjuist. Probeer het a.u.b. opnieuw");
						} else {
							dialog.Dismiss ();
							homeActivity.ShowToast("Passphrase geaccepteerd en LocalBox succesvol geregistreerd");
							homeActivity.menuFragment.UpdateLocalBoxes ();
							SplashActivity.intentData = null;
						}
					} catch (Exception ex){
						Insights.Report(ex);
						homeActivity.HideProgressDialog ();
						homeActivity.ShowToast("Passphrase verifieren mislukt. Probeer het a.u.b. opnieuw");
					}
				}
			};
			buttonCancel.Click += (sender, args) => {
				DataLayer.Instance.DeleteLocalBox (localBox.Id);
				homeActivity.menuFragment.UpdateLocalBoxes ();
				dialog.Dismiss ();
			};
		}
	}
}

