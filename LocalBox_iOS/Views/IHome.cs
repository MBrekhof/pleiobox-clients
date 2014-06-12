using System;
using MonoTouch.UIKit;
using LocalBox_iOS.Views.ItemView;

namespace LocalBox_iOS.Views
{
    public interface IHome
    {
        void UpdateDetail(UIViewController vc, bool animate);

        void RemoveDetail();

        void Lock();

		void ShowIntroductionView ();

		//void OpenPDFViewer (PdfItemView pdfItemView);
    }
}

