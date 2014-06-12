using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using LocalBox_Common;
using System.Collections.Generic;
using System.Linq;
using LocalBox_iOS.Helpers;

namespace LocalBox_iOS.Views
{
    public partial class FavoriteNodeView : BaseNode, IListNode
    {
        public bool PresentingDetail
        {
            get;
            set;
        }

        public static readonly UINib Nib = UINib.FromName ("FavoriteNodeView", NSBundle.MainBundle);

        private NodeViewController _nodeViewController;

        public FavoriteNodeView()
        {
        }

        public FavoriteNodeView(IntPtr handle) : base(handle){}


        public static FavoriteNodeView Create (RectangleF frame, NodeViewController nodeViewController, UIColor kleur)
        {
            var view = (FavoriteNodeView)Nib.Instantiate (null, null) [0];
            view.Frame = frame;
            view.SetShadow();
            view._nodeViewController = nodeViewController;
            view.NodeItemTableController.RefreshControl = new UIRefreshControl()
            {
                TintColor = UIColor.FromRGB(143, 202, 232),
                AttributedTitle = new NSAttributedString("Map aan het verversen")
            };

            view.NodeItemTableController.RefreshControl.ValueChanged += delegate
            {
                view.Refresh();
            };

            view.SyncButton.TouchUpInside += delegate
            {;
                view.Refresh(scrollToTop: true);
            };

            if (kleur != null)
            {
                view.KleurenBalk.BackgroundColor = kleur;
            }
                
            view.Refresh(scrollToTop: true);
            return view;
        }

        public void Refresh(bool reload = true, bool scrollToTop = false) {
            if (scrollToTop)
            {
                NodeItemTable.ContentOffset = new PointF(0, -NodeItemTableController.RefreshControl.Frame.Height);
                NodeItemTableController.RefreshControl.BeginRefreshing();
            }

            if((NodeViewSource)NodeItemTable.Source == null) {
                NodeItemTable.Source = new NodeViewSource(DataLayer.Instance.GetFavorites(), _nodeViewController, this);
            } else {
                ((NodeViewSource)NodeItemTable.Source)._node = DataLayer.Instance.GetFavorites();
            }
            NodeItemTableController.RefreshControl.EndRefreshing();
            NodeItemTable.ReloadData();
        }

        public void ShareFolder(string forLocation)
        {
            throw new NotImplementedException();
        }

        public void ResetCells(params UITableViewCell[] except)
        {
            var cells = NodeItemTable.VisibleCells.Except(except);
            foreach (UITableViewCell cell in cells)
            {
                var cc = ((NodeViewCell)cell);
                cc.ResetView(true);
            }
            PresentingDetail = false;
        }

        private class NodeViewSource : UITableViewSource {

            public List<TreeNode> _node;
            private NodeViewController _nodeViewController;
            private FavoriteNodeView _nodeView;

            public NodeViewSource(List<TreeNode> node, NodeViewController nodeViewController, FavoriteNodeView nodeView) {
                _node = node;
                _nodeViewController = nodeViewController;
                _nodeView = nodeView;
            }

            public override int NumberOfSections(UITableView tableView)
            {
                return 1;
            }

            public override int RowsInSection(UITableView tableview, int section)
            {
                return _node.Count;
            }

            public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
            {
                if (_nodeView.PresentingDetail)
                {
                    _nodeView.ResetCells();
                }
                else
                {
                    _nodeViewController.ShowNode(_node[indexPath.Row], _nodeView);
                }
            }

            public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
            {
                var cell = tableView.DequeueReusableCell (NodeViewCell.Key) as NodeViewCell;
                if (cell == null)
                    cell = NodeViewCell.Create(_nodeView);

                cell.ResetView();

                cell.Node = _node[indexPath.Row] ;


                return cell;
            }

            public override NSIndexPath WillSelectRow(UITableView tableView, NSIndexPath indexPath)
            {
                if (_nodeView.PresentingDetail)
                {
                    _nodeView.ResetCells();
                    return null;
                }

                return indexPath;
            }

        }

    }
}

