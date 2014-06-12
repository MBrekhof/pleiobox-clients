// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;
using System.CodeDom.Compiler;

namespace LocalBox_iOS.Views.Table
{
    [Register("MenuViewCellTop")]
	partial class MenuViewCellTop
	{
		[Outlet]
		MonoTouch.UIKit.UILabel TitelLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (TitelLabel != null) {
				TitelLabel.Dispose ();
				TitelLabel = null;
			}
		}
	}
}
