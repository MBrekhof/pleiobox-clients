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
	[Register ("FavoriteNodeView")]
	partial class FavoriteNodeView
	{
		[Outlet]
		UIKit.UIView KleurenBalk { get; set; }

		[Outlet]
		UIKit.UITableView NodeItemTable { get; set; }

		[Outlet]
		UIKit.UITableViewController NodeItemTableController { get; set; }

		[Outlet]
		UIKit.UIButton SyncButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (KleurenBalk != null) {
				KleurenBalk.Dispose ();
				KleurenBalk = null;
			}

			if (NodeItemTable != null) {
				NodeItemTable.Dispose ();
				NodeItemTable = null;
			}

			if (NodeItemTableController != null) {
				NodeItemTableController.Dispose ();
				NodeItemTableController = null;
			}

			if (SyncButton != null) {
				SyncButton.Dispose ();
				SyncButton = null;
			}
		}
	}
}
