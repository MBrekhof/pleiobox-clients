using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using LocalBox_iOS.Helpers;
using LocalBox_Common;
using System.Linq;

namespace LocalBox_iOS.Views
{
    public partial class NodeViewCell : UITableViewCell
    {
        public static readonly UINib Nib = UINib.FromName("NodeViewCell", NSBundle.MainBundle);
        public static readonly NSString Key = new NSString("NodeViewCell");
        private NodeViewCellDefault _defaultView;
        private NodeViewCellDetail _detail;
        private NodeGestureRecognizer _lpg;
        private TreeNode _treeNode;

        public TreeNode Node
        {
            get
            {
                return _treeNode;
            }
            set
            {
                _treeNode = value;
                UpdateCell();
            }
        }

        public NodeViewCell(IntPtr handle) : base(handle)
        {
        }

        public static NodeViewCell Create(IListNode nodeView)
        {
            var view = (NodeViewCell)Nib.Instantiate(null, null)[0];
            view._defaultView = NodeViewCellDefault.Create();
            view._defaultView.Frame = view.Frame;
            view._detail = NodeViewCellDetail.Create(nodeView);
            view._detail.Frame = view.Frame;
            view.Add(view._detail);
            view.Add(view._defaultView);

            view._lpg = new NodeGestureRecognizer((Action<UIPanGestureRecognizer>)((e) =>
            {
                PointF change = e.TranslationInView(view);
                if (e.State == UIGestureRecognizerState.Changed)
                {
                    e.CancelsTouchesInView |= Math.Abs(change.X) > 0;
                    if (nodeView.PresentingDetail)
                    {
                        nodeView.ResetCells(view);
                    }
                    view._defaultView.Frame = new RectangleF(new PointF(change.X <= 0 ? change.X : 0, view._defaultView.Frame.Y), view._defaultView.Frame.Size);
                }
                else if (e.State == UIGestureRecognizerState.Ended)
                {
                    var width = -view._defaultView.Frame.Width;
                    nodeView.PresentingDetail = change.X < width / 2;

                    var newX = nodeView.PresentingDetail ? width : 0;

                    UIView.Animate(.65d, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        view._defaultView.Frame = new RectangleF(new PointF(newX, view._defaultView.Frame.Y), view._defaultView.Frame.Size);
                    }, null);
                    e.CancelsTouchesInView = false;
                }
            }));
            view._lpg.CancelsTouchesInView = false;
            view.AddGestureRecognizer(view._lpg);
            return view;
        }

        private void UpdateCell()
        {
            _defaultView.Titel = Node.Name;

            if (Node.IsFavorite && string.IsNullOrEmpty(Node.Path))
            {
                _lpg.Enabled = false;
                _detail.Node = null;
                _defaultView.Type = UIImage.FromBundle("icons/IcLijst-favorieten");
                _detail.Hidden = true;
            }
            else
            {
                bool encrypted;
                if (Node.Path.Count(e => e == '/') > 1)
                {
                    int index = Node.Path.IndexOf('/', Node.Path.IndexOf('/') + 1);
                    var rootFolder = Node.Path.Substring(0, index);
                    var folder = DataLayer.Instance.GetFolder(rootFolder).Result;
                    encrypted = folder.HasKeys;
                }
                else
                {
                    encrypted = Node.HasKeys;
                }

				_defaultView.Type = UIImage.FromBundle(Node.IsDirectory ?  encrypted ? "icons/IcType-Map-versleuteld" : "icons/IcType-Map" : TypeHelper.MapType(Node.Path));
                if (Node.IsFavorite == true)
                {
                    _defaultView.Deel = UIImage.FromBundle("icons/IcInd-favorieten");
                }
                else if (Node.IsShare == true)
                {
                    _defaultView.Deel = UIImage.FromBundle("icons/IcInd-Met-mij-gedeeld");
                }
                else if (Node.IsShared == true)
                {
                    _defaultView.Deel = UIImage.FromBundle("icons/IcInd_Zelf-gedeeld");
                }
                else
                {
                    _defaultView.Deel = null;
                }
                _lpg.Enabled = true;
                _detail.Node = Node;
                _detail.Hidden = false;
            }

        }

        public void ResetView(bool animated = false)
        {
            RectangleF frame = new RectangleF(new PointF(0, 0), _defaultView.Frame.Size);
            if (animated)
            {
                UIView.Animate(.65d, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                {
                    _defaultView.Frame = frame;
                }, null);
            }
            else
            {
                _defaultView.Frame = frame;
            }
        }

        public override void SetHighlighted(bool highlighted, bool animated)
        {
            if (highlighted)
            {
                _defaultView.BackgroundColor = UIColor.FromRGB(226, 226, 226);
            }
            else
            {
                _defaultView.BackgroundColor = UIColor.FromRGB(255, 255, 255);
            }
        }

        public override void SetSelected(bool selected, bool animated)
        {
            if (selected)
            {
                _defaultView.BackgroundColor = UIColor.FromRGB(226, 226, 226);
            }
            else
            {
                _defaultView.BackgroundColor = UIColor.FromRGB(255, 255, 255);
            }
        }

        private class NodeGestureRecognizer : UIPanGestureRecognizer
        {
            public NodeGestureRecognizer(Action<UIPanGestureRecognizer> recognizer) : base(recognizer)
            {
            }

            public override bool CanBePreventedByGestureRecognizer(UIGestureRecognizer preventingGestureRecognizer)
            {
                return !CancelsTouchesInView;
            }

            public override bool CanPreventGestureRecognizer(UIGestureRecognizer preventedGestureRecognizer)
            {
                return CancelsTouchesInView;
            }
        }
    }
}

