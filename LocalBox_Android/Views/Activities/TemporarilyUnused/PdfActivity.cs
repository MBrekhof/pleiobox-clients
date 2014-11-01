using System;
using System.IO;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using Android.Graphics.Drawables;
using Android.Graphics;

//using pdftron;
//using pdftron.PDF;
//using pdftron.PDF.Tools;
//using pdftron.FDF;

using LocalBox_Common;
using LocalBox_Common.Remote;
//Reference to unused pdftron dll deleted
/*
namespace localbox.android
{
	[Activity (Label = "PDF viewer")]			
	public class PdfActivity : Activity
	{
		private string absolutePathOfPDF;
		private string relativePathOfPDF;
		private string fileName;
		private string temporaryFilePath;
		private PDFViewCtrl mPdfViewCtrl;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			try {
				absolutePathOfPDF = Intent.GetStringExtra ("absolutePathOfPDF");
				relativePathOfPDF = Intent.GetStringExtra ("relativePathOfPDF");
				fileName 		  = Intent.GetStringExtra ("fileName");

				//Hide action bar
				this.ActionBar.Hide ();

				// Initialize PDFNet and load the pdf file
				try {
					PDFNet.Initialize (Android.App.Application.Context);
					PDFNet.SetDefaultDiskCachingEnabled(false);
				} 
				catch (pdftron.Common.PDFNetException e) {
					Console.WriteLine (e.GetMessage ());
					return;
				}

				SetContentView (Resource.Layout.activity_pdf);

				//Change color of custom action bar
				RelativeLayout pdfActionBar = FindViewById<RelativeLayout> (Resource.Id.pdf_fullscreen_custom_actionbar);
				if (!String.IsNullOrEmpty (HomeActivity.colorOfSelectedLocalBox)) {
					pdfActionBar.SetBackgroundColor (Color.ParseColor (HomeActivity.colorOfSelectedLocalBox));
				} else {
					pdfActionBar.SetBackgroundColor (Color.ParseColor (Constants.lightblue));
				}

				mPdfViewCtrl = FindViewById<PDFViewCtrl> (Resource.Id.view_pdf);

				//Set file name
				TextView textviewFilename = FindViewById<TextView> (Resource.Id.textview_filename_pdf);
				textviewFilename.Text = fileName;

				PDFDoc pdfToOpen = new PDFDoc (absolutePathOfPDF);
				mPdfViewCtrl.Doc = pdfToOpen;

				ToolManager toolManager = new ToolManager (mPdfViewCtrl);

				mPdfViewCtrl.ToolManager = toolManager;

				ImageButton buttonSavePdf = FindViewById<ImageButton> (Resource.Id.button_save_pdf);
				buttonSavePdf.Click += delegate {
					UpdatePdfFile (false);
				};

			} catch {
				Toast.MakeText (this, "Het openen van het PDF bestand is mislukt", ToastLength.Short).Show ();
				this.Finish ();
			}
			
		}

		public override void OnBackPressed ()
		{
			//Controleer of pdf document gewijzigd is en zo ja, geef een waarschuwing dat de wijzigingen verloren gaan
			PDFDoc pdfToSave = mPdfViewCtrl.Doc;
			bool documentLocked = false;
			if (pdfToSave != null) {
				documentLocked = pdfToSave.TryLock ();
				if (documentLocked) {
					if (pdfToSave.IsModified ()) { //Bestand aangepast dus toon waarschuwing

						var dialogBuilder = new AlertDialog.Builder (this);
						dialogBuilder.SetTitle ("Let op");
						dialogBuilder.SetMessage ("U heeft wijzigingen aangebracht aan het PDF document welke nog niet zijn opgeslagen. \nDeze wijzigingen gaan verloren als u het document niet opslaat.");

						dialogBuilder.SetPositiveButton ("Opslaan", (EventHandler<DialogClickEventArgs>)null);
						dialogBuilder.SetNegativeButton ("Niet opslaan", (EventHandler<DialogClickEventArgs>)null);

						var dialog = dialogBuilder.Create ();
						dialog.Show ();

						// Get the buttons.
						var buttonAddFolder = dialog.GetButton ((int)DialogButtonType.Positive);
						var buttonCancel = dialog.GetButton ((int)DialogButtonType.Negative);

						buttonAddFolder.Click += (sender, args) => {
							UpdatePdfFile(true);
						};

						buttonCancel.Click += (sender, args) => {
							if (File.Exists (temporaryFilePath)) 
							{
								File.Delete (temporaryFilePath);
							}
							base.OnBackPressed ();
						};
					}
					else 
					{ 	//Bestand niet aangepast dus sluit activity zonder waarschuwing
						if (File.Exists (temporaryFilePath)) 
						{
							File.Delete (temporaryFilePath);
						}
						base.OnBackPressed ();
					}
				}else{
					base.OnBackPressed ();
				}
			}else{
				base.OnBackPressed ();
			}
		}





		private async void UpdatePdfFile(bool closeActivity)
		{
			CustomProgressDialog progressDialog = new CustomProgressDialog ();
			progressDialog.Show (this, null);

			try {
				bool deleteSucceed = false;
				bool uploadedSucceeded = false;

				PDFDoc pdfToSave = mPdfViewCtrl.Doc;

				bool documentLocked = false;

				if (pdfToSave != null) {

					documentLocked = pdfToSave.TryLock ();
					if (documentLocked) 
					{
						if (pdfToSave.IsModified ()) 
						{
							string fileNameOfOriginalPdf = pdfToSave.GetFileName ();

							if (fileNameOfOriginalPdf.Length == 0) 
							{
								deleteSucceed = false;
							} 
							else 
							{
								Java.IO.File file = new Java.IO.File (fileNameOfOriginalPdf);

								if (!file.Exists () || file.CanWrite ()) 
								{
									//Save pdf file to temporary file - this file is used to upload
									var documentsPath = DocumentConstants.DocumentsPath;
									temporaryFilePath = System.IO.Path.Combine (documentsPath, "temporary.pdf");

									if (File.Exists (temporaryFilePath)) {
										File.Delete (temporaryFilePath);
									}
										
									//Save temporary file in filesystem
									pdfToSave.Save (temporaryFilePath, pdftron.SDF.SDFDoc.SaveOptions.e_compatibility);

									if (File.Exists (temporaryFilePath)) {

										//Delete file from local box
										deleteSucceed = await DataLayer.Instance.DeleteFileOrFolder (relativePathOfPDF);
									}

									if (deleteSucceed) {
										//Upload new file to local box
										uploadedSucceeded = await DataLayer.Instance.UploadFile (relativePathOfPDF, temporaryFilePath);
									}
									else{
										//Verwijder temporary file
										if (File.Exists (temporaryFilePath)) {
											File.Delete (temporaryFilePath);
										}
									}
								}
							}

							if (uploadedSucceeded) {
								if (File.Exists (temporaryFilePath)) 
								{
									File.Delete (temporaryFilePath);
								}
								Toast.MakeText (this, "PDF bestand succesvol bijgewerkt", ToastLength.Short).Show ();

								if(closeActivity){
									this.Finish();
								}
							} 
							else if (!deleteSucceed || !uploadedSucceeded) 
							{
								Toast.MakeText (this, "Het bijwerken van het PDF bestand is mislukt", ToastLength.Short).Show ();
							}

						}else{
							Toast.MakeText (this, "Er is niets gewijzigd aan het document.", ToastLength.Long).Show ();
						}
					}
				}
			} catch {
				Toast.MakeText (this, "Het bijwerken van het PDF bestand is mislukt", ToastLength.Long).Show ();
				progressDialog.Hide ();
			}
			progressDialog.Hide ();
		}


		protected override void OnPause ()
		{
			base.OnPause ();
			LockHelper.SetLastActivityOpenedTime ("PdfActivity");
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			bool shouldShowLockScreen = LockHelper.ShouldLockApp ("PdfActivity");

			if (shouldShowLockScreen) 
			{
				//Lock scherm
				HomeActivity.shouldLockApp = true;
				StartActivity(typeof(PinActivity));
			} 
		}
			
	}
}
*/

