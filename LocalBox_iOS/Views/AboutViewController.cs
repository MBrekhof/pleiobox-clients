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

        public override void DidReceiveMemoryWarning()
        {
            // Releases the view if it doesn't have a superview.
            base.DidReceiveMemoryWarning();
			
            // Release any cached data, images, etc that aren't in use.
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            string htmlFile = NSBundle.MainBundle.PathForResource("over/LocalBoxInfo", "html");
            WebView.LoadRequest(new NSUrlRequest(NSUrl.FromFilename(htmlFile)));
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            View.SetShadow();
        }
    }
}

