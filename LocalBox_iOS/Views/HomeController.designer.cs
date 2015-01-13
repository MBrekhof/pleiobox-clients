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
	[Register ("HomeController")]
	partial class HomeController
	{
		[Outlet]
		UIKit.UIImageView imageViewSplash { get; set; }

		[Outlet]
		UIKit.UIView kleurenBalk { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (imageViewSplash != null) {
				imageViewSplash.Dispose ();
				imageViewSplash = null;
			}

			if (kleurenBalk != null) {
				kleurenBalk.Dispose ();
				kleurenBalk = null;
			}
		}
	}
}
