using System;
using MonoTouch.UIKit;

namespace LocalBox_iOS.Views
{
    public interface IListNode
    {
        void Refresh(bool reload = true, bool scrollToTop = false);

        bool PresentingDetail
        {
            get;
            set;
        }

        void ResetCells(params UITableViewCell[] except);

        void ShareFolder(string forLocation);

		void ShareFile(string forLocation);
    }
}

