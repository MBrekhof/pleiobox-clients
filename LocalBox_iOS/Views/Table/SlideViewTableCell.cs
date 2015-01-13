using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace LocalBox_iOS.Views.Table
{
    public abstract class SlideViewTableCell : UITableViewCell
    {
        protected UIView TopView { get; set; } 
        protected UIView BottomView { get; set; }

        protected UIColor CellBackgroundColor { get; set; }
        protected UIColor CellHighlightColor { get; set; }

        public Action<bool, SlideViewTableCell>  OnPresentingDetails;

        private NodeGestureRecognizer _lpg;


        protected SlideViewTableCell(string reuseIdentifier) : base(UITableViewCellStyle.Default, reuseIdentifier)
        {
            _lpg = new NodeGestureRecognizer((Action<UIPanGestureRecognizer>)((e) =>
            {
                CGPoint change = e.TranslationInView(this);
                if (e.State == UIGestureRecognizerState.Changed)
                {
                    e.CancelsTouchesInView |= Math.Abs(change.X) > 0;
                    if (OnPresentingDetails != null)
                    {
                        OnPresentingDetails(false, this);
                    }
                    if(TopView != null)
                        TopView.Frame = new  CGRect(new CGPoint(change.X <= 0 ? change.X : 0, TopView.Frame.Y), TopView.Frame.Size);
                }
                else if (e.State == UIGestureRecognizerState.Ended)
                {
                    var width = -TopView.Frame.Width;
                    var presenting = change.X < width / 2;

                    if(OnPresentingDetails != null) {
                        OnPresentingDetails(presenting, this);
                    }

                    var newX = presenting ? width : 0;
                    if(TopView != null) {
                        UIView.Animate(.65d, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                        {
                            TopView.Frame = new CGRect(new CGPoint(newX, TopView.Frame.Y), TopView.Frame.Size);
                        }, null);
                    }
                    e.CancelsTouchesInView = false;
                }
            }));
            _lpg.CancelsTouchesInView = false;
            AddGestureRecognizer(_lpg);

        }


        public void ResetView(bool animated = false)
        {
            if (TopView == null)
                return;

            CGRect frame = new CGRect(new CGPoint(0, 0), TopView.Frame.Size);
            if (animated)
            {
                UIView.Animate(.65d, 0, UIViewAnimationOptions.CurveEaseOut, () =>
                {
                    TopView.Frame = frame;
                }, null);
            }
            else
            {
                TopView.Frame = frame;
            }
        }


        public override void SetHighlighted(bool highlighted, bool animated)
        {
            if (TopView == null)
                return;

            if (highlighted)
            {
                TopView.BackgroundColor = CellHighlightColor;
            }
            else
            {
                TopView.BackgroundColor = CellBackgroundColor;
            }
        }

        public override void SetSelected(bool selected, bool animated)
        {
            if (TopView == null)
                return;

            if (selected)
            {
                TopView.BackgroundColor = CellHighlightColor;
            }
            else
            {
                TopView.BackgroundColor = CellBackgroundColor;
            }
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            TopView.Frame = ContentView.Bounds;
            BottomView.Frame = ContentView.Bounds;
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

