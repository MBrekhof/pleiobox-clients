using System;
using Foundation;
using UIKit;
using CoreGraphics;
using LocalBox_Common;
using System.Threading;
using LocalBox_iOS.Helpers;
using System.Linq;
using AssetsLibrary;
using System.IO;
using System.Diagnostics;
using MessageUI;

using Xamarin;
using Photos;
using Xamarin.Media;

namespace LocalBox_iOS.Views
{
	public partial class NodeView : BaseNode, IListNode
	{
		public static readonly UINib Nib = UINib.FromName ("NodeView", NSBundle.MainBundle);
		public UITableView Table { get { return NodeItemTable; } }
		private NodeViewController _nodeViewController;

		public bool PresentingDetail {
			get;
			set;
		}

		public NodeView ()
		{

		}

		public NodeView (IntPtr handle) : base (handle)
		{

		}

		public static NodeView Create (CGRect frame, NodeViewController nodeViewController, string path, UIColor kleur)
		{
			var view = (NodeView)Nib.Instantiate (null, null) [0];
			view.Frame = frame;
			view.SetShadow ();
			view.NodePath = path;
			view._nodeViewController = nodeViewController;
			view.NodeItemTableController.RefreshControl = new UIRefreshControl () {
				TintColor = UIColor.FromRGB (143, 202, 232),
				AttributedTitle = new NSAttributedString ("Map aan het verversen")
			};

			view.NodeItemTableController.RefreshControl.ValueChanged += delegate {
				view.Refresh ();
			};

			view.TerugButton.TouchUpInside += delegate {
				nodeViewController.PopView ();
			};

			view.SyncButton.TouchUpInside += delegate {
				view.Refresh (scrollToTop: true);
			};

			view.UploadFotoButton.TouchUpInside += delegate {
				view.UploadFoto ();
			};

			view.MaakFolderButton.TouchUpInside += view.CreateFolderButtonPressed;

			if (kleur != null) {
				view.kleurenBalk.BackgroundColor = kleur;
			}

			view.UploadFotoButton.Hidden = path.Equals ("/");
			view.HideBackButton ();
			view.LoadData ();
			return view;
		}

		public async void ShareFolder (string forLocation)
		{
			DialogHelper.ShowBlockingProgressDialog ("Delen voorbereiden", "Gebruikers ophalen... \nEen ogenblik geduld a.u.b.");

			try {

				var users = await DataLayer.Instance.GetLocalboxUsers ();
				var share = await BusinessLayer.Instance.GetShareSettings (forLocation);

				if (users != null) {
					InvokeOnMainThread (() => {

						var view = DelenView.Create (users, share, forLocation);

						view.OnUsersSelected += (object sender, EventArgs e) => {
							Refresh ();
						};
						DialogHelper.HideBlockingProgressDialog ();

						HomeController.homeController.View.Add (view);
					});
				} else {
					DialogHelper.ShowErrorDialog ("Fout", "Er is een fout opgetreden bij het ophalen van gebruikers. \nProbeer het a.u.b. nogmaals.");
				}

			}  catch (Exception ex){
				Insights.Report(ex);
				DialogHelper.HideBlockingProgressDialog ();
				DialogHelper.ShowErrorDialog ("Fout", "Er is een fout opgetreden. \n" +
				"Probeer het a.u.b. nogmaals.");
			}
		}


		public void ShareFile (string forLocation)
		{
			DateTime selectedExpirationDate = DateTime.Now.AddDays (7); //Selectie standaard op 7 dagen na vandaag
		
			ActionSheetDatePickerCustom actionSheetDatePicker;
			actionSheetDatePicker = new ActionSheetDatePickerCustom (HomeController.homeController.View);
			actionSheetDatePicker.Title = "Kies een vervaldatum:";
			actionSheetDatePicker.Picker.Mode = UIDatePickerMode.Date;
			actionSheetDatePicker.Picker.MinimumDate = NSDateHelper.DateTimeToNSDate (DateTime.Today.AddDays (1));

			//Zet selectie standaard op 7 dagen na vandaag
			actionSheetDatePicker.Picker.SetDate (NSDateHelper.DateTimeToNSDate (DateTime.Today.AddDays (7)), true);

			actionSheetDatePicker.Picker.ValueChanged += (sender, e) => {

				var nsDate = (sender as UIDatePicker).Date;

				DateTime selectedDate = NSDateHelper.NSDateToDateTime (nsDate);
				selectedExpirationDate = selectedDate.AddDays (1);

				Console.WriteLine (selectedExpirationDate.ToString ());
			};

			actionSheetDatePicker.DoneButton.Clicked += (object sender, EventArgs e) => {

				//Dismiss actionsheet
				actionSheetDatePicker.Hide (true);

				//Show progress dialog
				DialogHelper.ShowProgressDialog ("Delen", "Publieke url aan het ophalen", async () => {
					try {
						PublicUrl publicUrl = await DataLayer.Instance.CreatePublicFileShare (forLocation, selectedExpirationDate.Date);

						MFMailComposeViewController mvc = new MFMailComposeViewController ();
						mvc.SetSubject ("Publieke URL naar gedeeld LocalBox bestand");

						string bodyText = "Mijn gedeelde bestand: \n" +
						                   publicUrl.publicUri + "\n \n" +
						                   "Deze link is geldig tot: " + selectedExpirationDate.ToString ("dd-MM-yyyy");

						mvc.SetMessageBody (bodyText, false);
						mvc.Finished += (object s, MFComposeResultEventArgs args) => {
							args.Controller.DismissViewController (true, null);
						};
						_nodeViewController.PresentViewController (mvc, true, null);

						DialogHelper.HideProgressDialog ();

					}  catch (Exception ex){
						Insights.Report(ex);
						DialogHelper.HideProgressDialog ();
						DialogHelper.ShowErrorDialog ("Fout", "Er is een fout opgetreden bij het delen van het bestand." +
						"\nVervers a.u.b. de map en probeer het opnieuw.");
					}
				});
			};

			actionSheetDatePicker.Show ();
		}



		private async void LoadData ()
		{
			NodeItemTable.ContentOffset = new CGPoint (0, -NodeItemTableController.RefreshControl.Frame.Height);
			NodeItemTableController.RefreshControl.BeginRefreshing ();

			try {
				Node = await DataLayer.Instance.GetFolder (NodePath);
				NodeItemTable.Source = new NodeViewSource (Node, _nodeViewController, this);
			}  catch (Exception ex){
				Insights.Report(ex);
				Debug.WriteLine ("Exception in " + ex.Source + ": " + ex.Message);
				ShowErrorRetrievingList ();
			}
			NodeItemTableController.RefreshControl.EndRefreshing ();
		}

		void CreateFolderButtonPressed (object sender, EventArgs e)
		{
			UIAlertView createFolderAlert = new UIAlertView ("Nieuwe map", "Voer een naam in voor de nieuwe map", null, "Annuleer", "Maak map");
			createFolderAlert.AlertViewStyle = UIAlertViewStyle.PlainTextInput;
			createFolderAlert.GetTextField (0).Placeholder = "Mapnaam";
			createFolderAlert.Clicked += (object s, UIButtonEventArgs args) => {
				if (args.ButtonIndex == 1) {
					DialogHelper.ShowProgressDialog ("Map maken", "De map wordt gemaakt", async () => {
						bool result = await DataLayer.Instance.CreateFolder (System.IO.Path.Combine (NodePath, ((UIAlertView)s).GetTextField (0).Text));
						if (!result) {
							ShowErrorCreatingFolder ();
						} else {
							Refresh (scrollToTop: true);
						}
						DialogHelper.HideProgressDialog ();
					});
				}
			};

			createFolderAlert.Show ();
		}
			

		private void UploadFoto ()
		{
			try {
				MediaPicker mediaPicker = new MediaPicker ();
				Camera.PictureFromGallery (mediaPicker, CallbackPhotoMade);
			} catch (Exception ex) {
				Insights.Report (ex);
			}
		}

		void CallbackPhotoMade (MediaFile obj)
		{
			try {
				InvokeOnMainThread (() => {

					UIImage uiImage = UIImage.FromFile (obj.Path);

					NSData data;
					string filename = Path.GetFileName (obj.Path);

					if (filename.EndsWith (".png", StringComparison.OrdinalIgnoreCase)) {
						data = uiImage.AsPNG ();
					} else {
						data = uiImage.AsJPEG ();
					}

					string destination = Path.Combine (NodePath, filename);
					filename = Path.Combine (DocumentConstants.DocumentsPath, filename);

					NSError err = null;
					if (data.Save (filename, false, out err)) {
						UploadFile (destination, filename);
					} else {
						DialogHelper.HideProgressDialog ();
						DialogHelper.ShowErrorDialog ("Fout", "Er is een fout opgetreden bij het opslaan van het bestandsnaam");
					}
				});
			} catch (Exception ex) {
				Console.WriteLine (ex.Message);
			}
		}

		private void UploadFile (string destination, string filename)
		{
			try{
				DialogHelper.ShowProgressDialog ("Uploaden", "Bezig met het uploaden van een bestand", async () => {
					bool result = await DataLayer.Instance.UploadFile (destination, filename);
					if (!result) {
						DialogHelper.ShowErrorDialog ("Fout", "Er is iets fout gegaan bij het uploaden van het bestand");
					} else {
						UIAlertView alertSuccess = new UIAlertView ("Succesvol", "Het bestand is succesvol geupload", null, "OK");
						alertSuccess.Show ();
					}
					DialogHelper.HideProgressDialog ();
					File.Delete (filename);
					Refresh (scrollToTop: true);
				});
			}catch (Exception ex) {
				Console.WriteLine (ex.Message);
				Insights.Report (ex);
			}
		}

		public async void Refresh (bool refresh = true, bool scrollToTop = false)
		{
			if (scrollToTop) {
				NodeItemTable.ContentOffset = new CGPoint (0, -NodeItemTableController.RefreshControl.Frame.Height);
				NodeItemTableController.RefreshControl.BeginRefreshing ();
			}

			try {
				Node = await DataLayer.Instance.GetFolder (NodePath, refresh);
				if ((NodeViewSource)NodeItemTable.Source == null) {
					NodeItemTable.Source = new NodeViewSource (Node, _nodeViewController, this);
				} else {
					((NodeViewSource)NodeItemTable.Source)._node = Node;
				}
				NodeItemTableController.RefreshControl.EndRefreshing ();
				NodeItemTable.ReloadData ();
				_nodeViewController.Refresh ();
			} catch (Exception e) {
				Debug.WriteLine ("Exception in " + e.Source + ": " + e.Message);
				NodeItemTableController.RefreshControl.EndRefreshing ();
				ShowErrorRetrievingList ();
			}
		}

		private void ShowErrorRetrievingList ()
		{
			if (!SslValidator.CertificateErrorRaised) {
				DialogHelper.ShowErrorDialog ("Fout", "Er is een fout opgetreden bij het ophalen van de map");
			}
		}

		private void ShowErrorCreatingFolder ()
		{
			DialogHelper.ShowErrorDialog ("Fout", "Er is een fout opgetreden bij het maken van de map");
		}

		public override void HideBackButton ()
		{
			TerugButton.Layer.Opacity = 0;
		}

		public override void ShowBackButton ()
		{
			TerugButton.Layer.Opacity = 1;
		}


		public void ResetCells (params UITableViewCell[] except)
		{
			var cells = NodeItemTable.VisibleCells.Except (except);
			foreach (UITableViewCell cell in cells) {
				var cc = ((NodeViewCell)cell);
				cc.ResetView (true);
			}
			PresentingDetail = false;
		}

		private class NodeViewSource : UITableViewSource
		{

			public TreeNode _node;
			private NodeViewController _nodeViewController;
			private NodeView _nodeView;
			private bool _root;

			public NodeViewSource (TreeNode node, NodeViewController nodeViewController, NodeView nodeView)
			{
				_node = node;
				_nodeViewController = nodeViewController;
				_nodeView = nodeView;
				_root = _node.ParentId == 0;
			}

			public override nint NumberOfSections (UITableView tableView)
			{
				return 1;
			}

			public override nint RowsInSection (UITableView tableview, nint section)
			{
				return _node.Children.Count + (_root ? 1 : 0);
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (_nodeView.PresentingDetail) {
					_nodeView.ResetCells ();
				} else {
					if (_root && indexPath.Row == 0) {
						_nodeViewController.ShowFavorite ();
					} else {
						_nodeViewController.ShowNode (_node.Children [indexPath.Row - (_root ? 1 : 0)], _nodeView);
					}
				}
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell (NodeViewCell.Key) as NodeViewCell;
				if (cell == null)
					cell = NodeViewCell.Create (_nodeView);

				cell.ResetView ();

				if (_root && indexPath.Row == 0) {
					cell.Node = new TreeNode () {
						IsFavorite = true,
						Name = "Lokale favorieten"
					};
				} else {
					cell.Node = _node.Children [indexPath.Row - (_root ? 1 : 0)];
				}


				return cell;
			}

			public override NSIndexPath WillSelectRow (UITableView tableView, NSIndexPath indexPath)
			{
				if (_nodeView.PresentingDetail) {
					_nodeView.ResetCells ();
					return null;
				}

				return indexPath;
			}

		}
	}
}

