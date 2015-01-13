// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace LocalBox_iOS
{
	[Register ("IntroductionViewController")]
	partial class IntroductionViewController
	{
		[Outlet]
		UIKit.UIButton buttonCancel { get; set; }

		[Outlet]
		UIKit.UIButton buttonOpenInternetBrowser { get; set; }

		[Outlet]
		UIKit.UIImageView imageViewInfoGraphic { get; set; }

		[Outlet]
		UIKit.UILabel labelDescription { get; set; }

		[Outlet]
		UIKit.UILabel labelImageDescription { get; set; }

		[Outlet]
		UIKit.UILabel labelTitle { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (labelImageDescription != null) {
				labelImageDescription.Dispose ();
				labelImageDescription = null;
			}

			if (buttonCancel != null) {
				buttonCancel.Dispose ();
				buttonCancel = null;
			}

			if (buttonOpenInternetBrowser != null) {
				buttonOpenInternetBrowser.Dispose ();
				buttonOpenInternetBrowser = null;
			}

			if (imageViewInfoGraphic != null) {
				imageViewInfoGraphic.Dispose ();
				imageViewInfoGraphic = null;
			}

			if (labelDescription != null) {
				labelDescription.Dispose ();
				labelDescription = null;
			}

			if (labelTitle != null) {
				labelTitle.Dispose ();
				labelTitle = null;
			}
		}
	}
}
