using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using LocalBox_Common;
using System.IO;
using System.Collections.Generic;
using LocalBox_iOS.Helpers;
using System.Linq;

using Xamarin;

namespace LocalBox_iOS.Views
{
    public partial class LocationChooserView : UIView
    {
        public static readonly UINib Nib = UINib.FromName("LocationChooserView", NSBundle.MainBundle);
        private LocationChooseDataSource _dataSource;
		private IListNode _parentNodeView;
		public bool NewFileUpload { get; private set; }

        public LocationChooserView() 
        {
        }

        public LocationChooserView(IntPtr handle) : base(handle) 
        {
        }

		public static LocationChooserView Create(string path, bool newFileUploadChooser, IListNode nodeView)
        {

			int selectedBox = Waardes.Instance.GeselecteerdeBox;

            var view =  (LocationChooserView)Nib.Instantiate(null, null)[0];
			view._BestandsnaamLabel.Text = path.Substring(path.LastIndexOf('/') + 1);
			view._parentNodeView = nodeView;
			view.NewFileUpload = newFileUploadChooser;
			view.OpslaanButton.TouchUpInside += async(object sender, EventArgs e) => {
				try{
					if(view.NewFileUpload == true){ //Nieuw bestand uploaden

						view.RemoveFromSuperview();
						DialogHelper.ShowBlockingProgressDialog("Uploaden", "Bezig met het uploaden van een bestand"); 

						string filename = Path.Combine(view._dataSource.NodeStack.Peek().Path, path.Substring(path.LastIndexOf('/') + 1));
						bool uploadSucceeded = await DataLayer.Instance.UploadFile(filename, path);
                	
						if(uploadSucceeded){
                        	var ef = DataLayer.Instance.GetFolder(view._dataSource.NodeStack.Peek().Path, true).Result;
							UIAlertView alertSuccess = new UIAlertView("Succesvol", "Het bestand is succesvol geupload", null, "OK");
							alertSuccess.Show();

						}else{
							DialogHelper.ShowErrorDialog("Fout", "Er is een fout opgetreden bij het uploaden van het bestand" +
													 	 "\nProbeer het a.u.b. opnieuw");
						}
						DialogHelper.HideBlockingProgressDialog();

						File.Delete(path);
                	
						Waardes.Instance.GeselecteerdeBox = selectedBox;

					}else{ //Verplaatsen van bestand

						DialogHelper.ShowBlockingProgressDialog("Verplaatsen", "Bezig met het verplaatsen van het bestand"); 

						string destinationPath = System.IO.Path.Combine(view._dataSource.NodeStack.Peek().Path, path.Substring(path.LastIndexOf('/') + 1));

						if(path.Equals(destinationPath)){
							DialogHelper.HideBlockingProgressDialog();

							DialogHelper.ShowErrorDialog("Fout", "Gekozen map mag niet hetzelfde zijn als de oorspronkelijke locatie van het bestand");	
						}
						else{
							bool uploadedSucceeded = false;
							bool deleteSucceeded = false;

							//decrypt file to filesystem and get path of decrypted file
							string filePath = await DataLayer.Instance.GetFilePath (path);

							//upload file to selected destination folder
							uploadedSucceeded = await DataLayer.Instance.UploadFile (destinationPath, filePath);

							//remove file from old destination
							if(uploadedSucceeded){
								deleteSucceeded = await DataLayer.Instance.DeleteFileOrFolder (path);

								//Update folder where file is moved to
								DataLayer.Instance.GetFolder(view._dataSource.NodeStack.Peek().Path, true); 
							}

							//close this dialog en show succeeded message
							if(deleteSucceeded){
								DialogHelper.HideBlockingProgressDialog();
								view.RemoveFromSuperview();
								view._parentNodeView.Refresh(true, true);
								UIAlertView alertSuccess = new UIAlertView("Succesvol", "Het bestand is succesvol verplaatst", null, "OK");
								alertSuccess.Show();
							}else{
								DialogHelper.HideBlockingProgressDialog();
								view.RemoveFromSuperview();
								view._parentNodeView.Refresh(true, true);
								DialogHelper.ShowErrorDialog("Fout", "Er is een fout opgetreden bij het verplaatsen van het bestand" +
															 		 "\n Ververs a.u.b. de map en probeer het opnieuw.");					
							}
						}
					}
				} catch (Exception ex){
					Insights.Report(ex);
					DialogHelper.HideBlockingProgressDialog();
					string message;

					if(view.NewFileUpload){
						message = "uploaden van het bestand. \nProbeer het a.u.b. opnieuw.";
					}else{
						message = "verplaatsen van het bestand. \nVervers a.u.b. de map en probeer het opnieuw.";
					}
					DialogHelper.ShowErrorDialog("Fout", "Er is een fout opgetreden bij het "+ message);
				}

            };

            view.TerugButton.TouchUpInside += (object sender, EventArgs e) => {
                view._dataSource.Back();
                view.TableView.ReloadData();
            };

            view.SluitenButton.TouchUpInside += (object sender, EventArgs e) => {
				if(view.NewFileUpload){
                	Waardes.Instance.GeselecteerdeBox = selectedBox;
                	File.Delete(path);
				}
                view.RemoveFromSuperview();
            };

			view._dataSource = new LocationChooseDataSource(view, view.NewFileUpload);
            view.TableView.Source = view._dataSource;
            view.ShowSaveButton(false);

            return view;
			
        }

        private void ShowSaveButton(bool show) {
            OpslaanButton.Enabled = show;
        }












        private class LocationChooseDataSource : UITableViewSource {

            public Stack<TreeNode> NodeStack { get; private set; }

            private List<TreeNode> _listItems;

            private LocationChooserView _parent;

			private bool _newFileUpload;


            readonly List<LocalBox> _boxes = DataLayer.Instance.GetLocalBoxesSync();


			public LocationChooseDataSource(LocationChooserView parent, bool newFileUpload) {
                NodeStack = new Stack<TreeNode>();
                _parent = parent;
				_newFileUpload = newFileUpload;
            }

            public override int NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override int RowsInSection(UITableView tableview, int section)
            {
				if (_listItems == null)
                {
					if (_parent.NewFileUpload) {
						return _boxes.Count;
					} else {
						NodeStack.Push(DataLayer.Instance.GetFolder("/").Result);
						_listItems = NodeStack.Peek().Children.Where(e => e.IsDirectory).ToList();
						return _listItems.Count;
					}
                }
                else
                {
                    return _listItems.Count;
                }
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell (LocationChooserViewCell.Key) as LocationChooserViewCell;
                if (cell == null)
                    cell = LocationChooserViewCell.Create();
                    

				if (_listItems == null)
                {
                    cell.Titel = _boxes[indexPath.Row].Name;
                    cell.Type = null;
                }
                else
                {
                    var node = _listItems[indexPath.Row];
                    cell.Titel = node.Name;

                    bool encrypted;
                    if (node.Path.Count(e => e == '/') > 1)
                    {
                        int index = node.Path.IndexOf('/', node.Path.IndexOf('/') + 1);
                        var rootFolder = node.Path.Substring(0, index);
                        var folder = DataLayer.Instance.GetFolder(rootFolder).Result;
                        encrypted = folder.HasKeys;
                    }
                    else
                    {
                        encrypted = node.HasKeys;
                    }

					cell.Type = UIImage.FromBundle(node.IsDirectory ?  encrypted ? "icons/IcType-Map-versleuteld" : "icons/IcType-Map" : TypeHelper.MapType(node.Path));
                }

                return cell;
            }

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				try {
					if (_listItems == null) {
						Waardes.Instance.GeselecteerdeBox = _boxes [indexPath.Row].Id;
						NodeStack.Push (DataLayer.Instance.GetFolder ("/").Result);
						_listItems = NodeStack.Peek ().Children.Where (e => e.IsDirectory).ToList ();
					} else {
						NodeStack.Push (DataLayer.Instance.GetFolder (_listItems [indexPath.Row].Path).Result);
						_listItems = NodeStack.Peek ().Children.Where (e => e.IsDirectory).ToList ();
					}
					_parent.TerugButton.Hidden = false;
					_parent.ShowSaveButton (!NodeStack.Peek ().Path.Equals ("/"));
					tableView.ReloadData ();
				} catch (Exception ex){
					Insights.Report(ex);
					new UIAlertView ("Fout", "Er is een fout opgetreden. Controleer uw internet verbinding en probeer het a.u.b. opnieuw.", null, "OK").Show ();
				}
			}


            public void Back() {

				if (NodeStack.Count == 0) {
					return;
				}

                NodeStack.Pop();

				if (NodeStack.Count > 0)
                {
					//Hide backbutton
					if (_newFileUpload == false) {
						if (NodeStack.Count == 1) {
							_parent.TerugButton.Hidden = true;
						}
					}

                    _listItems = NodeStack.Peek().Children.Where(e => e.IsDirectory).ToList();
                    _parent.ShowSaveButton(false);
                }
                else
                {
					//Hide backbutton
					_parent.TerugButton.Hidden = true;

                    _listItems = null;
                }



            }
        }
    }
}

