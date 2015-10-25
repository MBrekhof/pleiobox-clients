using System;
using System.Threading.Tasks;
using CoreGraphics;
using LocalBox_Common;
using UIKit;
using Foundation;
using LocalBox_iOS.Helpers;
using System.IO;
using System.Web;

namespace LocalBox_iOS.Views.ItemView
{
    public class WebItemView
    {
        public WebItemView()
        {
        }

        public static BaseItemView Create (CGRect frame, NodeViewController nodeViewController, TreeNode node, string filePath, UIColor kleur)
        {
            var view = BaseItemView.Create(frame, nodeViewController, node, kleur);
            UIWebView webView = new UIWebView();

			string extension = Path.GetExtension (filePath);
			if (extension == ".odt") {
				string viewerPath = NSBundle.MainBundle.PathForResource ("over/viewer/index", "html");

				Uri fromUri = new Uri(viewerPath);
				Uri toUri = new Uri(filePath);
				Uri relativeUri = fromUri.MakeRelativeUri(toUri);
				String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

				NSUrl finalUrl = new NSUrl ("#" + relativePath, new NSUrl(viewerPath, false));
				webView.LoadRequest(new NSUrlRequest(finalUrl));
				webView.ScalesPageToFit = true;
				view.ContentView = webView;

				return view;
			} else {
				NSUrl finalUrl = new NSUrl (filePath, false);
				webView.LoadRequest(new NSUrlRequest(finalUrl));
				webView.ScalesPageToFit = true;
				view.ContentView = webView;

				return view;
			}
		
        }
    }
}

