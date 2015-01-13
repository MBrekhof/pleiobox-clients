// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace LocalBox_iOS.Views
{
	[Register ("MenuFooterViewCell")]
	partial class MenuFooterViewCell
	{
		[Outlet]
		UIKit.UIImageView LogoImage { get; set; }

		[Outlet]
		UIKit.UILabel TitelLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LogoImage != null) {
				LogoImage.Dispose ();
				LogoImage = null;
			}

			if (TitelLabel != null) {
				TitelLabel.Dispose ();
				TitelLabel = null;
			}
		}
	}
}
