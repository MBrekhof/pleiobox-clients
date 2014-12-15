using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using LocalBox_iOS.Helpers;
using System.IO;

namespace LocalBox_iOS
{
    public partial class AboutViewController : UIViewController
    {
        public AboutViewController() : base("AboutViewController", null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            string htmlFile = NSBundle.MainBundle.PathForResource("over/LocalBoxInfo", "html");
            WebView.LoadRequest(new NSUrlRequest(NSUrl.FromFilename(htmlFile)));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            View.SetShadow();
        }
    }
}

