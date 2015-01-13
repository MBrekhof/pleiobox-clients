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
	[Register ("LocationChooserViewCell")]
	partial class LocationChooserViewCell
	{
		[Outlet]
		UIKit.UIImageView DeelIcon { get; set; }

		[Outlet]
		UIKit.UILabel TitelLabel { get; set; }

		[Outlet]
		UIKit.UIImageView TypeIcon { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TitelLabel != null) {
				TitelLabel.Dispose ();
				TitelLabel = null;
			}

			if (TypeIcon != null) {
				TypeIcon.Dispose ();
				TypeIcon = null;
			}

			if (DeelIcon != null) {
				DeelIcon.Dispose ();
				DeelIcon = null;
			}
		}
	}
}
