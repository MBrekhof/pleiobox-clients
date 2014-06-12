// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace LocalBox_iOS.Views
{
	[Register ("WachtwoordView")]
	partial class WachtwoordView
	{
		[Outlet]
		MonoTouch.UIKit.UIButton okButton { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITextField wachtwoordTextView { get; set; }

		void ReleaseDesignerOutlets ()
		{
			if (okButton != null) {
				okButton.Dispose ();
				okButton = null;
			}

			if (wachtwoordTextView != null) {
				wachtwoordTextView.Dispose ();
				wachtwoordTextView = null;
			}
		}
	}
}
