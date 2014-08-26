using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

public class ActionSheetDatePickerCustom
{
	const int CHROMEWIDTHLEFT = 9;
	const int CHROMEWIDTHRIGHT = 8;

	int MARGIN = 10;

	UIActionSheet _actionSheet;
	UIBarButtonItem _doneButton;
	UILabel titleLabel = new UILabel ();
        
	UIView _owner;
	UIToolbar _toolbar;
	UIDatePicker picker;

	public string Title {
		get { return titleLabel.Text; }
		set { titleLabel.Text = value; }
	}

	public UIDatePicker Picker {
		get { return picker; }
		set { picker = value; }
	}

	public UIBarButtonItem DoneButton {
		get { return _doneButton; }
		set { _doneButton = value; }
	}

	public UIActionSheet ActionSheet
	{
		get { return _actionSheet; }
		set { _actionSheet = value; }
	}


       
	public ActionSheetDatePickerCustom (UIView owner)
	{
		picker = new UIDatePicker (RectangleF.Empty);

		// save our uiview owner
		this._owner = owner;
              
		// create + configure the action sheet
		_actionSheet = new UIActionSheet () { Style = UIActionSheetStyle.BlackTranslucent };
		_actionSheet.Clicked += (s, e) => {
			Console.WriteLine ("Clicked on item {0}", e.ButtonIndex);
		};
                
		// configure the title label
		titleLabel = new UILabel (new RectangleF (0, 0, _actionSheet.Frame.Width, 10));
		titleLabel.BackgroundColor = UIColor.Clear;
		titleLabel.TextColor = UIColor.Black;
		titleLabel.Font = UIFont.BoldSystemFontOfSize (18);

		// Add the toolbar
		_toolbar = new UIToolbar (new RectangleF (0, 0, _actionSheet.Frame.Width, 10));
		_toolbar.BarStyle = UIBarStyle.Default;
		_toolbar.Translucent = true;

		// Add the done button
		_doneButton = new UIBarButtonItem (UIBarButtonSystemItem.Done, null, null);
                
		_toolbar.Items = new UIBarButtonItem[] {
			new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace, null, null),
			_doneButton
		};
                
		_toolbar.SizeToFit ();
               
		_actionSheet.AddSubview (picker);
		_actionSheet.Add (_toolbar);
		_actionSheet.AddSubview (titleLabel);
	}

       
	public void Show ()
	{
		// declare vars
		float titleBarHeight = 40;
		SizeF actionSheetSize = new SizeF (_owner.Frame.Width, picker.Frame.Height + titleBarHeight);
		RectangleF actionSheetFrame = new RectangleF (0, (UIScreen.MainScreen.ApplicationFrame.Width - actionSheetSize.Height), actionSheetSize.Width, actionSheetSize.Height);
                
		// show the action sheet and add the controls to it
		_actionSheet.ShowInView (_owner);
                
		// resize the action sheet to fit our other stuff
		_actionSheet.Frame = actionSheetFrame;
                
		// move our picker to be at the bottom of the actionsheet (view coords are relative to the action sheet)
		picker.Frame = new RectangleF (picker.Frame.X, titleBarHeight, picker.Frame.Width, picker.Frame.Height);

		// move our label to the top of the action sheet
		titleLabel.Frame = new RectangleF (10, 4, _owner.Frame.Width - 100, 35);

		// ipad
		if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {
			var popover = _actionSheet.Superview.Superview;
			if (popover != null) {

				MARGIN = 50;

				//var x = _actionSheet.Frame.X + MARGIN;
				//var y = (UIScreen.MainScreen.ApplicationFrame.Width - _actionSheet.Frame.Height) / 2;
				//var width = _actionSheet.Frame.Width - (MARGIN * 2);
				//var height = _actionSheet.Frame.Height;

				//popover.Frame = new RectangleF (x, y, width, height);

				//var centerWidth = UIScreen.MainScreen.ApplicationFrame.Height / 2;
				/*
				var centerHeight = UIScreen.MainScreen.ApplicationFrame.Width / 2;

				var width = UIScreen.MainScreen.ApplicationFrame.Height - (100);
				var height = picker.Frame.Height + 35;

				_actionSheet.Frame = new RectangleF (50, centerHeight, width , height);

				picker.Frame = new RectangleF (picker.Frame.X, picker.Frame.Y, _actionSheet.Frame.Width, picker.Frame.Height);

				_toolbar.SizeToFit ();
				*/

				var x = _actionSheet.Frame.X + MARGIN;
				var y = (UIScreen.MainScreen.ApplicationFrame.Width - _actionSheet.Frame.Height) / 2;
				var width = _actionSheet.Frame.Width - (MARGIN * 2);
				var height = _actionSheet.Frame.Height;

				popover.Frame = new RectangleF (x, y, width, height);
				_actionSheet.Frame = new RectangleF (x + 200, y, width - (CHROMEWIDTHLEFT + CHROMEWIDTHRIGHT), height - (CHROMEWIDTHLEFT + CHROMEWIDTHRIGHT));

				picker.Frame = new RectangleF (picker.Frame.X, picker.Frame.Y, _actionSheet.Frame.Width, picker.Frame.Height);

				_toolbar.SizeToFit ();
			}
		}
	}

       
	public void Hide (bool animated)
	{
		_actionSheet.DismissWithClickedButtonIndex (0, animated);
	}
        
             
}