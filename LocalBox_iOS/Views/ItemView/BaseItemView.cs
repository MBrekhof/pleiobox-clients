using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using LocalBox_Common;
using System.IO;
using System.Drawing;
using System.Threading.Tasks;
using LocalBox_iOS.Helpers;
using System.Linq;

namespace LocalBox_iOS.Views.ItemView
{
    public partial class BaseItemView : BaseNode
    {
        public static readonly UINib Nib = UINib.FromName ("BaseItemView", NSBundle.MainBundle);


        public UIView ContentView {

            get {
                return ContentViewContainer.Subviews.Length > 0 ? ContentViewContainer.Subviews[0] : null;
            }

            set {
                Array.ForEach(ContentViewContainer.Subviews, _ => _.RemoveFromSuperview());
                value.AutoresizingMask = UIViewAutoresizing.All;
                value.Frame = ContentViewContainer.Bounds;
                ContentViewContainer.Add(value);
            }
        }

        public RectangleF ContentViewSize {
            get {
                return ContentViewContainer.Bounds;
            }
        }

        public BaseItemView()
        {
        }

        public BaseItemView (IntPtr handle) : base (handle)
        {

        }

        public static BaseItemView Create (RectangleF frame, NodeViewController nodeViewController, TreeNode node, UIColor kleur)
        {
            var view = (BaseItemView)Nib.Instantiate (null, null) [0];
            view.Frame = frame;
            view.Node = node;
            view.NodePath = node.Path;
            view.SetShadow();

            view.TerugButton.TouchUpInside += delegate
            {
                nodeViewController.PopView();
            };

            view.TitelLabel.Text = node.Name;
            view.TitelLabel.SetFont();
            view.FullScreenButton.TouchUpInside += delegate {
                nodeViewController.ToggleFullScreen();
            };

            if (kleur != null)
            {
                view.kleurenBalk.BackgroundColor = kleur;
            }
            view.HideBackButton();
            return view;
        }

        public override void ViewWillResize() {
            if (UIApplication.SharedApplication.KeyWindow.RootViewController.View.Subviews.Contains(this))
            {
                FullScreenButton.SetImage(UIImage.FromBundle("IcTop_Minimaliseren"), UIControlState.Normal);
            }
            else
            {
                FullScreenButton.SetImage(UIImage.FromBundle("IcTop_Maximaliseren"), UIControlState.Normal);
            }
        }

        public override void HideBackButton()
        {
            TerugButton.Layer.Opacity = 0;
        }

        public override void ShowBackButton()
        {
            TerugButton.Layer.Opacity = 1;
        }
    }
}

