using System;

using Foundation;
using UIKit;
using System.Collections.Generic;
using LocalBox_Common.Remote;
using LocalBox_Common;

using XLabs.Platform.Services;
using System.Text;
using LocalBox_iOS.Views;
using Xamarin;
using LocalBox_iOS.Helpers;

namespace LocalBox_iOS
{
	public partial class AddSitesViewController : UIViewController
	{

		TableSource _tableSource;
		private HomeController _home;
		private bool _introduction;

		public AddSitesViewController (HomeController homeController) : base ("AddSitesViewController", null)
		{
			this._home = homeController;
		}

		public AddSitesViewController (HomeController homeController, bool introduction) : base ("AddSitesViewController", null)
		{
			this._home = homeController;
			this._introduction = introduction;

			// hide top and bottom when in introduction mode
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

			if (_introduction == true) {
				View.AddSubview(_home.GetIntroductionProgressView(1));

				RegistrationExplanation.Hidden = false;
				TopMenu.Hidden = true;

				BottomMenu.BackgroundColor = null;
				DarkBackground.BackgroundColor = null;
			}

			ActivityIndicator.Hidden = true;

			CancelButton.TouchUpInside += async (o, e) => {
				this.View.RemoveFromSuperview();
			};
				
			ToevoegenButton.TouchUpInside += async(o, e) => {
				if (DataLayer.Instance.GetLocalBoxesSync ().Count > 0) {
					try {						
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
						}

						_home.InitialiseMenu();
						this.View.RemoveFromSuperview();
					}catch (Exception ex){
						Insights.Report(ex);
					}
				}
			};
		}

		public override async void ViewDidAppear(bool didAppear) 
		{
			ActivityIndicator.Hidden = false;
			ActivityIndicator.StartAnimating ();

			// Perform any additional setup after loading the view, typically from a nib.
			if (DataLayer.Instance.GetLocalBoxesSync ().Count > 0) {
				var remoteExplorer = new RemoteExplorer ();

				try {
					List<Site> sites = await remoteExplorer.GetSites ();
					for (int i = sites.Count - 1; i >= 0; i--) {
						foreach (LocalBox box in DataLayer.Instance.GetLocalBoxesSync()) {
							if (box.BaseUrl == sites [i].Url) {
								sites.RemoveAt (i);
								continue;
							}
						}
					}

					ActivityIndicator.StopAnimating ();
					ActivityIndicator.Hidden = true;

					_tableSource = new TableSource (sites);
					TableView.Source = _tableSource;
					TableView.ReloadData();
				} catch (Exception ex) {
					Insights.Report (ex);
					DialogHelper.ShowErrorDialog ("Fout", "Er is een fout opgetreden tijdens het ophalen van de sites.");
				}
			}
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

