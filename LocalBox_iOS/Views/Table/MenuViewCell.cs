using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace LocalBox_iOS.Views.Table
{
    public class MenuViewCell : SlideViewTableCell
    {

        private MenuViewCellTop _topCell;
        private MenuViewCellDetail _backCell;

        public string Titel { 
            get { 
                return _topCell.Titel;
            } 
            set {
                _topCell.Titel = value;
            }
        }

        public Action Delete {
            get {
                return _backCell.DeleteButtonPressed;
            } set {
                _backCell.DeleteButtonPressed = value;
            }
        }



        public static readonly NSString Key = new NSString("MenuViewCell");


        public MenuViewCell() : base("MenuViewCell")
        {
            CellBackgroundColor = UIColor.FromRGB(239, 239, 239);
            CellHighlightColor = UIColor.FromRGB(226, 226, 226);
            TopView = _topCell = MenuViewCellTop.Create();
            BottomView = _backCell = MenuViewCellDetail.Create();
            Add(BottomView);
            Add(TopView);
        }
    }
}

