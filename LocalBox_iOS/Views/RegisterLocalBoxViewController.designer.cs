// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace LocalBox_iOS
{
	[Register ("RegisterLocalBoxViewController")]
	partial class RegisterLocalBoxViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIView viewActivityIndicator { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIWebView webViewRegisterLocalBox { get; set; }

		[Action ("CloseView:")]
		partial void CloseView (MonoTouch.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (viewActivityIndicator != null) {
				viewActivityIndicator.Dispose ();
				viewActivityIndicator = null;
			}

			if (webViewRegisterLocalBox != null) {
				webViewRegisterLocalBox.Dispose ();
				webViewRegisterLocalBox = null;
			}
		}
	}
}
