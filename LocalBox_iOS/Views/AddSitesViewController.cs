using System;

using Foundation;
using UIKit;
using System.Collections.Generic;
using LocalBox_Common.Remote;
using LocalBox_Common;

using XLabs.Platform.Services;
using System.Text;
using LocalBox_iOS.Views;

namespace LocalBox_iOS
{
	public partial class AddSitesViewController : UIViewController
	{

		TableSource _tableSource;
		private HomeController homeController;

		public AddSitesViewController (HomeController homeController) : base ("AddSitesViewController", null)
		{
			this.homeController = homeController;
		}


		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			// Perform any additional setup after loading the view, typically from a nib.
			var remoteExplorer = new RemoteExplorer();
			var sites = remoteExplorer.GetSites().Result;

			for (int i = sites.Count - 1; i >= 0; i--) {
				foreach (LocalBox box in DataLayer.Instance.GetLocalBoxesSync()) {
					if (box.BaseUrl == sites[i].Url) {
						sites.RemoveAt(i);
						continue;
					}
				}
			}

			_tableSource = new TableSource (sites);
			TableView.Source = _tableSource;

			CancelButton.TouchUpInside += async (o, e) => {
				this.View.RemoveFromSuperview();
			};
				
			OKButton.TouchUpInside += async(o, e) => {
				var localBox = DataLayer.Instance.GetSelectedOrDefaultBox ();
				foreach (Site site in _tableSource.selectedItems) {
					LocalBox box = new LocalBox();
					box.BackColor = localBox.BackColor;
					box.BaseUrl = site.Url;
					box.DatumTijdTokenExpiratie = localBox.DatumTijdTokenExpiratie;
					box.LogoUrl = localBox.LogoUrl;
					box.Name = site.Name;
					box.OriginalServerCertificate = null;
					box.PassPhrase = null;
					box.PrivateKey = null;
					box.PublicKey = null;
					box.User = localBox.User;
					DataLayer.Instance.AddOrUpdateLocalBox(box);

					homeController.InitialiseMenu();
				}

				this.View.RemoveFromSuperview();
			};
		}
	}

	public class TableSource: UITableViewSource {
		List<Site> _items = new List<Site>();
		List<int> _selectedItems = new List<int>();
		string CellIdentifier = "TableCell";

		public List<Site> selectedItems {
			get {
				var returnItems = new List<Site>();
				foreach (int i in _selectedItems) { returnItems.Add(_items[i]); }
				return returnItems;
			}
		}

		public TableSource (List<Site> items)
		{
			_items = items;
		}
			
		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			if (!_selectedItems.Contains (indexPath.Row)) {
				_selectedItems.Add (indexPath.Row);
			}
		}

		public override void RowDeselected(UITableView tableView, NSIndexPath indexPath)
		{
			if (_selectedItems.Contains (indexPath.Row)) {
				_selectedItems.Remove (indexPath.Row);
			}
		}

		public override nint RowsInSection (UITableView tableview, nint section)
		{
			return _items.Count;
		}

		public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
		{
			UITableViewCell cell = tableView.DequeueReusableCell (CellIdentifier);
			string item = _items[indexPath.Row].Name;

			//---- if there are no cells to reuse, create a new one
			if (cell == null)
			{ cell = new UITableViewCell (UITableViewCellStyle.Default, CellIdentifier); }

			cell.TextLabel.Text = item;

			return cell;
		}
	}
}

