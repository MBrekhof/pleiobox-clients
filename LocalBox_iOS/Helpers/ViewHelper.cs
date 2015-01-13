using System;
using CoreGraphics;
using System.Linq;

using UIKit;

using LocalBox_Common;

namespace LocalBox_iOS.Helpers
{
    public static class ViewHelper
    {
        public static void SetShadow(this UIView view) {
            view.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			view.Layer.ShadowColor = UIColor.FromRGBA(0f, 0f, 0f, .8f).CGColor;
			view.Layer.ShadowOpacity = 0.4f;
            view.Layer.ShadowRadius = 5.0f;
            view.Layer.ShadowPath = UIBezierPath.FromRect(new CGRect(-5, 0, view.Bounds.Width, view.Bounds.Height)).CGPath;
        }

        public static bool IsInRootNode(string path) {
            return path.Count(e => e == '/') == 1;
        }

        public static bool IsInRootNode(TreeNode node) {
            return IsInRootNode(node.Path);
        }
    }
}

