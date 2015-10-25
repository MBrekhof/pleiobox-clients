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
	[Register ("AddSitesViewController")]
	partial class AddSitesViewController
	{
		[Outlet]
		UIKit.UIActivityIndicatorView ActivityIndicator { get; set; }

		[Outlet]
		UIKit.UIView BottomMenu { get; set; }

		[Outlet]
		UIKit.UIButton CancelButton { get; set; }

		[Outlet]
		UIKit.UIView DarkBackground { get; set; }

		[Outlet]
		UIKit.UIButton OKButton { get; set; }

		[Outlet]
		UIKit.UIView RegistrationExplanation { get; set; }

		[Outlet]
		UIKit.UITableView TableView { get; set; }

		[Outlet]
		UIKit.UIButton ToevoegenButton { get; set; }

		[Outlet]
		UIKit.UIView TopMenu { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ActivityIndicator != null) {
				ActivityIndicator.Dispose ();
				ActivityIndicator = null;
			}

			if (BottomMenu != null) {
				BottomMenu.Dispose ();
				BottomMenu = null;
			}

			if (CancelButton != null) {
				CancelButton.Dispose ();
				CancelButton = null;
			}

			if (DarkBackground != null) {
				DarkBackground.Dispose ();
				DarkBackground = null;
			}

			if (OKButton != null) {
				OKButton.Dispose ();
				OKButton = null;
			}

			if (TableView != null) {
				TableView.Dispose ();
				TableView = null;
			}

			if (ToevoegenButton != null) {
				ToevoegenButton.Dispose ();
				ToevoegenButton = null;
			}

			if (TopMenu != null) {
				TopMenu.Dispose ();
				TopMenu = null;
			}

			if (RegistrationExplanation != null) {
				RegistrationExplanation.Dispose ();
				RegistrationExplanation = null;
			}
		}
	}
}
