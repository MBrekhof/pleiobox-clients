using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using LocalBox_Common;

namespace LocalBox_iOS.Views
{
    public partial class WachtwoordView : UIView
    {
		/*
        public static readonly UINib Nib = UINib.FromName ("WachtwoordView", NSBundle.MainBundle);
        private LocalBox _localBox;

        public WachtwoordView ()
        {

        }

        public WachtwoordView(IntPtr h): base(h)
        {
        }


        public static WachtwoordView Create (RectangleF frame, LocalBox box, UIViewController viewController)
        {
            var view = (WachtwoordView)Nib.Instantiate (null, null) [0];
            view.Frame = frame;

            view._localBox = box;

            view.okButton.TouchUpInside += delegate
            {
                BusinessLayer.Instance.Authenticate(view._localBox, view.wachtwoordTextView.Text);
                view.RemoveFromSuperview();

                ((HomeController)viewController).InitialiseMenu();
            };

            return view;
        }*/
    }

}

