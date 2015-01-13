using System;
using CoreGraphics;
using Foundation;
using UIKit;
using LocalBox_iOS.Helpers;

namespace LocalBox_iOS.Views
{
    public partial class MenuViewCell : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("MenuViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("MenuViewCell");

        public string Titel {
            get {
                return TitelLabel.Text;
            } set {
                TitelLabel.Text = value;
            }
        }

        public MenuViewCell(IntPtr handle) : base(handle)
        {
        }

        public static MenuViewCell Create()
        {
            var cell =  (MenuViewCell)Nib.Instantiate(null, null)[0];
            cell.BackgroundColor = UIColor.Clear;
            cell.TitelLabel.SetFont();
            cell.Editing = true; // Make the cell deletable
            cell.ShouldIndentWhileEditing = true;
            return cell;
        }

    }
}

