using System;
using System.Threading.Tasks;
using CoreGraphics;
using LocalBox_Common;
using UIKit;
using Foundation;
using LocalBox_iOS.Helpers;

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

            webView.LoadRequest(new NSUrlRequest(new NSUrl(filePath, false)));
            webView.ScalesPageToFit = true;
            view.ContentView = webView;

            return view;
        }
    }
}

