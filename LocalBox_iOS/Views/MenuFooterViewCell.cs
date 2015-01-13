using System;
using CoreGraphics;
using Foundation;
using UIKit;
using LocalBox_iOS.Helpers;

namespace LocalBox_iOS.Views
{
    public partial class MenuFooterViewCell : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("MenuFooterViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("MenuFooterViewCell");

        public string Titel {
            get {
                return TitelLabel.Text;
            } set {
                TitelLabel.Text = value;
            }
        }

        public UIImage Image {
            get {
                return LogoImage.Image;
            } set {
                LogoImage.Image = value;
            }
        }


        public MenuFooterViewCell(IntPtr handle) : base(handle)
        {
        }

        public static MenuFooterViewCell Create()
        {
            var view = (MenuFooterViewCell)Nib.Instantiate(null, null)[0];
            view.BackgroundColor = UIColor.Clear;
            view.TitelLabel.SetFont();
            return view;
        }
    }
}

