using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;

namespace LocalBox_iOS.Views.Table
{
    [Register("MenuViewCellDetail")]
    public partial class MenuViewCellDetail : UIView
    {
        public static readonly UINib Nib = UINib.FromName("MenuViewCellDetail", NSBundle.MainBundle);


        public Action DeleteButtonPressed;

        public MenuViewCellDetail(IntPtr handle) : base(handle)
        {
        }

        public static MenuViewCellDetail Create()
        {
            var view = (MenuViewCellDetail)Nib.Instantiate(null, null)[0];
            view.VerwijderButton.TouchUpInside += (object sender, EventArgs e) => {
                if(view.DeleteButtonPressed != null) {
                    view.DeleteButtonPressed();
                }
            };

            return view;
        }
    }
}

