using System;
using CoreGraphics;
using Foundation;
using UIKit;
using LocalBox_iOS.Helpers;
using System.IO;

namespace LocalBox_iOS
{
    public partial class AboutViewController : UIViewController
    {
        public AboutViewController() : base("AboutViewController", null)
        {
        }

		public int Index = 1;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            string htmlFile = NSBundle.MainBundle.PathForResource("over/LocalBoxInfo", "html");
			WebView.ShouldStartLoad = HandleShouldStartLoad;
			WebView.LoadRequest(new NSUrlRequest(NSUrl.FromFilename(htmlFile)));

        }

		bool HandleShouldStartLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
		{
			// Filter out clicked links
			if(navigationType == UIWebViewNavigationType.LinkClicked) {
				if(UIApplication.SharedApplication.CanOpenUrl(request.Url)) {
					// Open in Safari instead
					UIApplication.SharedApplication.OpenUrl(request.Url);
					return false;
				}
			}

			return true;
		}

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            View.SetShadow();
        }
    }
}

