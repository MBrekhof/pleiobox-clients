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
	[Register ("MenuViewController")]
	partial class MenuViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIActivityIndicatorView _indicatorViewLeftCorner { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIView kleurenBalk { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIImageView logo { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView MenuFooter { get; set; }

		[Outlet]
		MonoTouch.UIKit.UITableView MenuTable { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (_indicatorViewLeftCorner != null) {
				_indicatorViewLeftCorner.Dispose ();
				_indicatorViewLeftCorner = null;
			}

			if (kleurenBalk != null) {
				kleurenBalk.Dispose ();
				kleurenBalk = null;
			}

			if (logo != null) {
				logo.Dispose ();
				logo = null;
			}

			if (MenuFooter != null) {
				MenuFooter.Dispose ();
				MenuFooter = null;
			}

			if (MenuTable != null) {
				MenuTable.Dispose ();
				MenuTable = null;
			}
		}
	}
}
