using System;
using CoreGraphics;
using Foundation;
using UIKit;
using LocalBox_iOS.Helpers;

namespace LocalBox_iOS.Views
{
    public partial class LocationChooserViewCell : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("LocationChooserViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("LocationChooserViewCell");


        public UIImage Type { 
            get  {
                return TypeIcon.Image;
            } set {
                TypeIcon.Image = value;
            }
        }

        public UIImage Deel { 
            get  {
                return DeelIcon.Image;
            } set {
                DeelIcon.Image = value;
            }
        }

        public string Titel { 
            get  {
                return TitelLabel.Text;
            } set {
                TitelLabel.Text = value;
            }
        }

        public LocationChooserViewCell(IntPtr handle) : base(handle)
        {
        }

        public static LocationChooserViewCell Create()
        {
            var cell = (LocationChooserViewCell)Nib.Instantiate(null, null)[0];
            cell.TitelLabel.SetFont();
            return cell;
        }
    }
}

