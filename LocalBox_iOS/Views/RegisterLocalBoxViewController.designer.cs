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
	[Register ("RegisterLocalBoxViewController")]
	partial class RegisterLocalBoxViewController
	{
		[Outlet]
		UIKit.UIView viewActivityIndicator { get; set; }

		[Outlet]
		UIKit.UIWebView webViewRegisterLocalBox { get; set; }

		[Action ("CloseView:")]
		partial void CloseView (Foundation.NSObject sender);
		
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
