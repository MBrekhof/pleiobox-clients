using System;
using CoreGraphics;
using Foundation;
using UIKit;
using System.Collections.Generic;
using LocalBox_Common;
using System.Linq;
using LocalBox_iOS.Views.Table;
using System.IO;
using LocalBox_iOS.Helpers;
using System.Net;
using Xamarin;


namespace LocalBox_iOS.Views
{
    public partial class MenuViewController : UIViewController
    {
		public IHome _home;

        public MenuViewController(IHome home) : base("MenuViewController", null)
        {
            _home = home;
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            MenuTable.Source = new MenuTableDataSource(this);
            MenuFooter.Source = new FooterMenuTableDataSource(this);

			DialogHelper._indicatorViewLeftCorner = _indicatorViewLeftCorner;
        }

        public void SetLogo(string logoUrl)
        {
            if (string.IsNullOrEmpty(logoUrl))
            {
                //logo.Image = UIImage.FromBundle("RijkslogoArtboard-1");
            }
            else
            {;
				var p = Path.Combine(DocumentConstants.DocumentsPath, logoUrl.Substring(logoUrl.LastIndexOf("/") + 1));

                if (File.Exists(p))
                {
                    //logo.Image = UIImage.FromFile(p);
                }
                else
                {
                    //logo.Image = UIImage.FromBundle("RijkslogoArtboard-1");
                }
            }

        }

        public void UpdateBalkKleur(string kleur)
        {
            if (!string.IsNullOrEmpty(kleur))
            {
                float red, green, blue;
                var colorString = kleur.Replace ("#", "");
                red = Convert.ToInt32(string.Format("{0}", colorString.Substring(0, 2)), 16) / 255f;
                green = Convert.ToInt32(string.Format("{0}", colorString.Substring(2, 2)), 16) / 255f;
                blue = Convert.ToInt32(string.Format("{0}", colorString.Substring(4, 2)), 16) / 255f;
                UIColor theColor =  UIColor.FromRGBA(red, green, blue, 1.0f);
                kleurenBalk.BackgroundColor = theColor;
            }
            else
            {
                // Defaultkleur
                kleurenBalk.BackgroundColor = UIColor.FromRGB(0, 85, 158);
            }
        }

        private class MenuTableDataSource : UITableViewSource {
        
            private readonly MenuViewController _parent;

            private bool _presentingDetails;

            private List<LocalBox> boxList = null;

            public MenuTableDataSource(MenuViewController parent) {
                _parent = parent;
                boxList = DataLayer.Instance.GetLocalBoxesSync();
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
                return boxList.Count;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (_presentingDetails)
                {
                    ResetCells(tableView);
                }
                else
                {
					try
					{
						SslValidator.CertificateErrorRaised = false;

                    	Waardes.Instance.GeselecteerdeBox = boxList[indexPath.Row].Id;

						//Reset certificate validation check to default behavior
						//ServicePointManager.ServerCertificateValidationCallback = null;

//						if(boxList[indexPath.Row].OriginalSslCertificate != null){ //Selected localbox does have a ssl certificate
//							//Set ssl validator for selected LocalBox
//							SslValidator sslValidator = new SslValidator ();
//							ServicePointManager.ServerCertificateValidationCallback = sslValidator.ValidateServerCertficate;
//						}

						if(boxList[indexPath.Row].BackColor.StartsWith("#")){
                    		_parent.UpdateBalkKleur(boxList[indexPath.Row].BackColor);
						}
                    	//_parent.SetLogo(boxList[indexPath.Row].LogoUrl);
						_parent._home.UpdateDetail(new NodeViewController(boxList[indexPath.Row].BackColor, _parent._home), true);
					} catch (Exception ex){
						Insights.Report(ex);
						DialogHelper.ShowErrorDialog("Fout", "Er is een fout opgetreden bij het openen van de Pleiobox.\n" +
													 "Probeer het a.u.b. nogmaals.");
					}
                }
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell (LocalBox_iOS.Views.Table.MenuViewCell.Key) as LocalBox_iOS.Views.Table.MenuViewCell;
                if (cell == null)
                    cell = new LocalBox_iOS.Views.Table.MenuViewCell();

                cell.ResetView();
                cell.Titel = boxList[indexPath.Row].Name;
                cell.Delete = () =>
                {

					UIAlertView alertView = new UIAlertView("Waarschuwing", 
						"Weet u zeker dat u deze Pleiobox wilt verwijderen? \nDeze actie is niet terug te draaien.", 
						null, 
						"Annuleer",
						"Verwijder");
					alertView.Clicked += delegate(object a, UIButtonEventArgs eventArgs) {
						if(eventArgs.ButtonIndex == 1){
							try{
								var selectedRow = tableView.IndexPathForSelectedRow;

								DataLayer.Instance.DeleteLocalBox(boxList[indexPath.Row].Id);
								boxList.RemoveAt(indexPath.Row);
								tableView.DeleteRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
								tableView.ReloadData();
								if(selectedRow != null && selectedRow.Row == indexPath.Row) {
									_parent._home.RemoveDetail();
								}
							} catch (Exception ex){
								Insights.Report(ex);
								DialogHelper.ShowErrorDialog("Fout", "Er is een fout opgetreden bij het verwijderen van de Pleiobox.\n" +
															 "Probeer het a.u.b. nogmaals.");
							}
						}
					};
					alertView.Show();
                };

                cell.OnPresentingDetails = (bool presenting, SlideViewTableCell source) =>
                {
                    ResetCells(tableView, source);
                    _presentingDetails = presenting;
                };

                return cell;
            }

            public override NSIndexPath WillSelectRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (_presentingDetails)
                {
                    ResetCells(tableView);
                    return null;
                }

                return indexPath;
            }

            private void ResetCells(UITableView table, params UITableViewCell[] except) {
                var cells = table.VisibleCells.Except(except);
                foreach (UITableViewCell cell in cells)
                {
                    var cc = ((SlideViewTableCell)cell);
                    cc.ResetView(true);
                }
                _presentingDetails = false;
            }

        }
    
        private class FooterMenuTableDataSource : UITableViewSource {

            private readonly MenuViewController _parent;

            public FooterMenuTableDataSource(MenuViewController parent) {
                _parent = parent;
            }

            public override nint RowsInSection(UITableView tableview, nint section)
            {
				return 2;
            }

            public override nint NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                
				if (indexPath.Row == 0)
				{
					if (DataLayer.Instance.GetLocalBoxesSync().Count == 0) {
						_parent._home.ShowIntroductionView();
					} else {
						_parent._home.ShowAddSitesView();
					}
				} else if (indexPath.Row == 1) {
                    Waardes.Instance.GeselecteerdeBox = -1;
                    _parent.UpdateBalkKleur(null);
                    _parent.SetLogo(null);
                    _parent._home.UpdateDetail(new AboutViewController(), true);
                }
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell (MenuFooterViewCell.Key) as MenuFooterViewCell;
                if (cell == null)
                    cell = MenuFooterViewCell.Create();
  
                switch (indexPath.Row)
                {
                    case 0:
						cell.Titel = "Site toevoegen";
						cell.Image = UIImage.FromBundle("IcBottom-ToevoegenLB");
						break;
					case 1:
						cell.Titel = "Over de app";
						cell.Image = UIImage.FromBundle("IcBottom-Over-de-app");
						break;
                }
                
                return cell;
            }

        }
    }
}

