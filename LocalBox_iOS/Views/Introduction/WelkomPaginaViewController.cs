﻿
using System;

using Foundation;
using UIKit;

namespace LocalBox_iOS
{
	public partial class WelkomPaginaViewController : UIViewController
	{

		public WelkomPaginaViewController () : base ("WelkomPaginaViewController", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
		}
	}
}
