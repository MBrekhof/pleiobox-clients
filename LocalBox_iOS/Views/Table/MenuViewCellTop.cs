using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using LocalBox_iOS.Helpers;

namespace LocalBox_iOS.Views.Table
{
    public partial class MenuViewCellTop : UIView
    {
        public static readonly UINib Nib = UINib.FromName("MenuViewCellTop", NSBundle.MainBundle);


        public string Titel { 
            get { 
                return TitelLabel.Text;
            } set { 
                TitelLabel.Text = value;
            } 
        }

        public MenuViewCellTop(IntPtr handle) : base(handle)
        {
        }

        public static MenuViewCellTop Create()
        {
            var view = (MenuViewCellTop)Nib.Instantiate(null, null)[0];
            view.TitelLabel.SetFont();
            return view;
        }
    }
}

