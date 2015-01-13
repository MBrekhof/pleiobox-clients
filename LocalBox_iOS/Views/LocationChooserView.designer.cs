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
	[Register ("LocationChooserView")]
	partial class LocationChooserView
	{
		[Outlet]
		UIKit.UILabel _BestandsnaamLabel { get; set; }

		[Outlet]
		UIKit.UIView ContentView { get; set; }

		[Outlet]
		UIKit.UIButton OpslaanButton { get; set; }

		[Outlet]
		UIKit.UIButton SluitenButton { get; set; }

		[Outlet]
		UIKit.UITableView TableView { get; set; }

		[Outlet]
		UIKit.UIButton TerugButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ContentView != null) {
				ContentView.Dispose ();
				ContentView = null;
			}

			if (OpslaanButton != null) {
				OpslaanButton.Dispose ();
				OpslaanButton = null;
			}

			if (SluitenButton != null) {
				SluitenButton.Dispose ();
				SluitenButton = null;
			}

			if (TableView != null) {
				TableView.Dispose ();
				TableView = null;
			}

			if (TerugButton != null) {
				TerugButton.Dispose ();
				TerugButton = null;
			}

			if (_BestandsnaamLabel != null) {
				_BestandsnaamLabel.Dispose ();
				_BestandsnaamLabel = null;
			}
		}
	}
}
