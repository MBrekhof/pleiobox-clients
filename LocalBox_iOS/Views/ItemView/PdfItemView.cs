using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.IO;

using Foundation;
using UIKit;

using LocalBox_Common;
using LocalBox_iOS.Helpers;

//using PSPDFKit;

namespace LocalBox_iOS.Views.ItemView
{
	//Reference to binding project is deleted
	/*
	public class PdfItemView : PSPDFViewController
	{
		string pathOfPdfFile;
		string pathOfPdfFileAtFileSystem;
		private PSPDFDocument psPdfDocument;

		public PdfItemView (PSPDFDocument document) : base (document)
		{
		}

		public PdfItemView (NSUrl documentPath, string pathOfPdfFile, string pathOfPdfFileAtFileSystem) : this (new PSPDFDocument (documentPath))
		{
			this.pathOfPdfFile = pathOfPdfFile;
			this.pathOfPdfFileAtFileSystem = pathOfPdfFileAtFileSystem;
			psPdfDocument = base.Document;
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			psPdfDocument.AnnotationSaveMode = PSPDFAnnotationSaveMode.EmbeddedWithExternalFileAsFallback;
			AutosaveEnabled = false;

			//Logging veroorzaakt crash in een release build
			//PSPDFLogging.LogLevel = PSPDFLogLevelMask.All;


			UIBarButtonItem closeButton = new UIBarButtonItem (UIImage.FromFile ("buttons/IcTop_Sluiten.png"), UIBarButtonItemStyle.Plain, null);
			//new UIBarButtonItem("Sluiten", UIBarButtonItemStyle.Done, null);
			closeButton.Clicked += (o, e) => {

				UIAlertView alertView = new UIAlertView("Controle", 
														"Heeft u annotaties toegevoegd die u wilt opslaan?", null, 
														"Sluiten",
														"Opslaan en sluiten");
				alertView.Clicked += delegate(object a, UIButtonEventArgs eventArgs) {
					if(eventArgs.ButtonIndex == 1){
						UpdatePdfFileAndCloseView();
					}else{
						//Delete possible changes and close view
						DataLayer.Instance.DeleteLocalFileOrFolder(pathOfPdfFile);
						DismissViewController (true, null);
					}
				};
				alertView.Show();
			};
			NSObject closeButtonObject = NSObject.FromObject (closeButton);


			UIBarButtonItem saveButton = new UIBarButtonItem (UIImage.FromFile ("buttons/IcTop_Opslaan.png"), UIBarButtonItemStyle.Plain, null);
			//new UIBarButtonItem("Opslaan", UIBarButtonItemStyle.Done, null);
			saveButton.Clicked += (o, e) => {
				UpdatePdfFile();
			};

			NSObject saveButtonObject = NSObject.FromObject (saveButton);

			NSObject[] leftButtons = new NSObject[]{closeButtonObject, saveButtonObject};
			LeftBarButtonItems = leftButtons;
		}


		private void UpdatePdfFileAndCloseView ()
		{
			DialogHelper.ShowBlockingProgressDialog ("Bijwerken", "Eventuele aanpassingen opslaan. \nEen ogenblik geduld a.u.b.");

			//Save annotations
			ThreadPool.QueueUserWorkItem ((data) => {
				BeginInvokeOnMainThread (async() => {

					NSError occuredError;
					bool savedByPsPdfKit = psPdfDocument.SaveAnnotationsWithError(out occuredError);

						if(savedByPsPdfKit){

							bool updateSucceeded = await DataLayer.Instance.SavePdfAnnotations (pathOfPdfFile);

						if (updateSucceeded) {
							DialogHelper.HideBlockingProgressDialog ();
							DismissViewController (true, null);
						} else {
							DialogHelper.HideBlockingProgressDialog ();
							DialogHelper.ShowErrorDialog ("Fout", "Er is een fout opgetreden tijdens het bijwerken van het bestand.");
						}
					}
				});
			});
		}


		private void UpdatePdfFile ()
		{
			DialogHelper.ShowBlockingProgressDialog ("Opslaan", "Aanpassingen opslaan. \nEen ogenblik geduld a.u.b.");

			//Save annotations
			ThreadPool.QueueUserWorkItem ((data) => {
				BeginInvokeOnMainThread (async() => {

					NSError occuredError;
					bool savedByPsPdfKit = psPdfDocument.SaveAnnotationsWithError(out occuredError);

					if(savedByPsPdfKit){

						bool updateSucceeded = await DataLayer.Instance.SavePdfAnnotations (pathOfPdfFile);

						if (updateSucceeded) {
							DialogHelper.HideBlockingProgressDialog ();

							new UIAlertView ("Succesvol", 
							"Wijzigingen zijn succesvol opgeslagen.", null, 
							"OK").Show ();
						} else {
							DialogHelper.HideBlockingProgressDialog ();
							DialogHelper.ShowErrorDialog ("Fout", "Er is een fout opgetreden tijdens het bijwerken van het bestand.");
						}
					}

				});
			});
		}

	}*/
}


/*
using System;
using System.Threading.Tasks;
using System.Drawing;
using LocalBox_Common;
using MonoTouch.UIKit;
using pdftron;
using pdftron.PDF;
using pdftron.PDF.Tools;
using System.IO;

namespace LocalBox_iOS.Views.ItemView
{
    public class PdfItemView
    {
        public PdfItemView()
        {
        }

        public static BaseItemView Create (RectangleF frame, NodeViewController nodeViewController, TreeNode node, string path, UIColor kleur)
        {
            var view = BaseItemView.Create(frame, nodeViewController, node, kleur);

            try
            {
                PDFNet.Initialize ();
                PDFNet.SetResourcesPath("pdfnet.res");
            

            	PDFViewCtrl mPdfViewCtrl = new PDFViewCtrl(view.ContentViewSize);

           	 	view.ContentView = mPdfViewCtrl;
            	foreach (UIView v in mPdfViewCtrl.Subviews)
            	{
               	 	v.AutoresizingMask = UIViewAutoresizing.All;
            	}

            	// Associate a PDF document with the viewer from resource
				if (File.Exists (path)) {
					PDFDoc docToOpen = new PDFDoc (path);
					mPdfViewCtrl.Doc = docToOpen;
					mPdfViewCtrl.AutosizesSubviews = true;

					ToolManager toolManager = new ToolManager (mPdfViewCtrl);
					mPdfViewCtrl.ToolManager = toolManager;
				}

			} 
			catch (Exception ex) 
			{
				Console.WriteLine(ex.Message);

				new UIAlertView ("ERROR", "Het is mislukt om het pdf bestand te openen.", null, null, "Ok").Show ();

				return view;
			}
           

            return view;
        }
    }
}
*/



