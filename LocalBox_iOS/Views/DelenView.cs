using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using LocalBox_Common;
using System.Drawing;
using System.Linq;
using LocalBox_iOS.Helpers;

namespace LocalBox_iOS
{
    public partial class DelenView :UIView
    {
        public static readonly UINib Nib = UINib.FromName("DelenView", NSBundle.MainBundle);
        DelenDataSource _dataSource;
        List<Identity> Users;
        Share Share;

        public event EventHandler OnUsersSelected;

        public DelenView(IntPtr handle) : base(handle) 
        {
        }

        public static DelenView Create(List<Identity> users, Share share, string path)
        {
            var view = (DelenView)Nib.Instantiate(null, null)[0];
            view.Layer.ShadowColor = UIColor.FromRGBA(0f, 0f, 0f, .8f).CGColor;
            view.Layer.ShadowOpacity = 0.7f;

            view.Users = users;
            view.Share = share;

            view.TitelTekst.Text = string.Format("Delen: {0}",path);

            view.SluitenButton.TouchUpInside += (object sender, EventArgs e) => {
                view.RemoveFromSuperview();
            };

            view.OkButton.TouchUpInside += async (object sender, EventArgs e) => {
                
				DialogHelper.ShowBlockingProgressDialog("Bezig", "Deelinstellingen worden bijgewerkt. \nEen ogenblik geduld a.u.b.");

				List<Identity> selectedUsers = view._dataSource.SelectedUsers;
				bool shareSucceeded = false;
				string message;

				if(selectedUsers != null) {

                    if (share == null)
                    {
						shareSucceeded = await BusinessLayer.Instance.ShareFolder(path, selectedUsers);
						message = "Map is succesvol gedeeld.";
                    }
                    else
                    {
						shareSucceeded = await BusinessLayer.Instance.UpdateSettingsSharedFolder(share, selectedUsers);
						message = "Deelinstellingen zijn succesvol bijgewerkt";
                    }

					DialogHelper.HideBlockingProgressDialog();
					if(shareSucceeded){
						UIAlertView alertSuccess = new UIAlertView("Succesvol", message, null, "OK");
						alertSuccess.Show();

						view.RemoveFromSuperview();

					}else{
						DialogHelper.ShowErrorDialog("Fout", "Er is een fout opgetreden bij delen van de map.");
					}

				}else{
					DialogHelper.HideBlockingProgressDialog();
                	view.RemoveFromSuperview();
				}

				//if (view.OnUsersSelected != null){
			 	view.OnUsersSelected(view, EventArgs.Empty);
				//}
				//view = null;
					//});
            };

            view.NaamTextField.ShouldChangeCharacters = (textField, range, replacementString) => 
            {
                string searchText = string.Empty;
                if (range.Length == 1 && string.IsNullOrEmpty(replacementString))
                {
                    if (textField.Text.Length > 0)
                    {
                        searchText = textField.Text.Substring(0, textField.Text.Length - 1);
                    }
                }
                else
                {
                    searchText = textField.Text + replacementString;
                }
                view._dataSource.UpdateData(view.Users.Where(f => f.Title.ToLower().Contains(searchText)).ToList());
                view.TableView.ReloadData();
                return true;
            };


            if (share != null)
            {
                view._dataSource = new DelenDataSource(users, share.Identities);
            }
            else{
                view._dataSource = new DelenDataSource(users);
            }
            view.TableView.Source = view._dataSource;

            return view;
        }

        private class DelenDataSource : UITableViewSource 
        {
            List<Identity> _users;
            List<Identity> _selectedUsers = new List<Identity>();

            public List<Identity> SelectedUsers {
                get { return _selectedUsers; }
            }

            public List<Identity> Users {
                get { return _users; }
                set { _users = value; }
            }

            const string _cellIdentifier = "cell";


            public DelenDataSource(List<Identity> users) {
                this._users = users;
            }

            public DelenDataSource(List<Identity> users, List<Identity> selectedUsersToShareWith) {
                this._users = new List<Identity> ();

                foreach (Identity identity in selectedUsersToShareWith) {
                    this._users.Add (identity);
                }

                foreach (Identity foundIdentity in users) 
                {
                    bool found = false;

                    foreach (Identity foundSelectedIdentity in selectedUsersToShareWith) {
                        if (foundIdentity.Id == foundSelectedIdentity.Id) {
                            found = true;
                            _selectedUsers.Add(foundSelectedIdentity);
                            break;
                        }
                    }

                    if (found == false) {
                        this._users.Add (foundIdentity);
                    }
                }
            }

            public void UpdateData(List<Identity> users)
            {
                this._users = new List<Identity> ();

                foreach (Identity identity in _selectedUsers) {
                    this._users.Add (identity);
                }

                foreach (Identity foundIdentity in users) 
                {
                    bool found = false;

                    foreach (Identity foundSelectedIdentity in _selectedUsers) {
                        if (foundIdentity.Id == foundSelectedIdentity.Id) {
                            found = true;
                            break;
                        }
                    }

                    if (found == false) {
                        this._users.Add (foundIdentity);
                    }
                }
            }

            public override int NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override int RowsInSection(UITableView tableview, int section)
            {
                if (_users == null)
                {
                    return 0;
                }
                else
                {
                    return _users.Count;
                }
            }


            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                int rowIndex = indexPath.Row;
                Identity item  = _users[rowIndex];

                UITableViewCell cell = tableView.DequeueReusableCell(_cellIdentifier);

                if (cell == null)
                {
                    cell = new UITableViewCell(UITableViewCellStyle.Subtitle, _cellIdentifier);
                }

                // Store what you want your cell to display always, not only when you are creating it.
                cell.Tag = rowIndex;
                cell.TextLabel.TextColor = UIColor.FromRGB(103, 103, 103);
                cell.TextLabel.Font = UIFont.SystemFontOfSize(16f);

                bool sel = false;
                foreach(var selected in SelectedUsers)
                {
                    if (selected.Id.Equals(_users[indexPath.Row].Id))
                    {
                        sel = true;
                        break;
                    }
                }
                if (sel)
                {
                    cell.Accessory = UITableViewCellAccessory.Checkmark;
                }
                else
                {
                    cell.Accessory = UITableViewCellAccessory.None;
                }
                cell.TextLabel.Text = item.Title;

                return cell;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                Identity sel = null;
                foreach(var selected in SelectedUsers)
                {
                    if (selected.Id.Equals(_users[indexPath.Row].Id))
                    {
                        sel = selected;
                        break;
                    }
                }


                if (sel != null)
                {
                    _selectedUsers.Remove(sel);
                }
                else
                {
                    _selectedUsers.Add(_users[indexPath.Row]);
                }
                tableView.ReloadData();
            }

            public void Back() {

            }
        }
    }

}

