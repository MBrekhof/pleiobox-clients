using System;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;
using Foundation;
using UIKit;
using LocalBox_iOS;
using LocalBox_iOS.Helpers;
using System.Threading.Tasks;
using System.Threading;

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

	UIViewController viewController = new UIViewController ();


       
	public ActionSheetDatePickerCustom (UIView owner)
	{
		picker = new UIDatePicker (CGRect.Empty);

		// save our uiview owner
		this._owner = owner;
              
		// create + configure the action sheet
		_actionSheet = new UIActionSheet () { Style = UIActionSheetStyle.BlackTranslucent };
		_actionSheet.Clicked += (s, e) => {
			Console.WriteLine ("Clicked on item {0}", e.ButtonIndex);
		};
                
		// configure the title label
		titleLabel = new UILabel (new CGRect (0, 0, _actionSheet.Frame.Width, 10));
		titleLabel.BackgroundColor = UIColor.Clear;
		titleLabel.TextColor = UIColor.Black;
		titleLabel.Font = UIFont.BoldSystemFontOfSize (18);

		// Add the toolbar
		_toolbar = new UIToolbar (new CGRect (0, 0, _actionSheet.Frame.Width, 10));
		_toolbar.BarStyle = UIBarStyle.Default;
		_toolbar.Translucent = true;

		// Add the done button
		_doneButton = new UIBarButtonItem ("Gereed", UIBarButtonItemStyle.Done, null);
                
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
		float titleBarHeight = 40;
		CGSize actionSheetSize = new CGSize (_owner.Frame.Width, picker.Frame.Height + titleBarHeight);
		CGRect actionSheetFrame = new CGRect (0, (UIScreen.MainScreen.ApplicationFrame.Width - actionSheetSize.Height), actionSheetSize.Width, actionSheetSize.Height);
                
		// show the action sheet and add the controls to it
		if (!UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
			_actionSheet.ShowInView (_owner);
		}
                
		// resize the action sheet to fit our other stuff
		_actionSheet.Frame = actionSheetFrame;
                
		// move our picker to be at the bottom of the actionsheet (view coords are relative to the action sheet)
		picker.Frame = new CGRect (picker.Frame.X, titleBarHeight, picker.Frame.Width, picker.Frame.Height);

		// move our label to the top of the action sheet
		titleLabel.Frame = new CGRect (10, 4, _owner.Frame.Width - 100, 35);

		// ipad
		if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad) {

			if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
			
				int size = (int)picker.Frame.Size.Height + 44;

				viewController.View.Frame = new CGRect (0, UIApplication.SharedApplication.KeyWindow.Frame.Height, _owner.Frame.Size.Width, UIApplication.SharedApplication.KeyWindow.Frame.Height);
				//picker.Frame = new RectangleF (new PointF (0, 44), picker.Frame.Size);
				picker.Frame = new CGRect (picker.Frame.X, 44, _owner.Frame.Width, picker.Frame.Height);
				viewController.View.AddSubview (picker);
				viewController.View.BackgroundColor = UIColor.White;

				_toolbar = new UIToolbar (new CGRect (0, 0, _owner.Frame.Size.Width, 44));

				var leftButton = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) { Width = 50 };
				var middleButton = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace) { Width = 50 };

				_toolbar.SetItems( new UIBarButtonItem[] { leftButton, middleButton, _doneButton }, false);

				nfloat xPosition = _owner.Frame.Size.Width / 2 - 150;
				UILabel labelTitle = new UILabel (new CGRect (xPosition, 0, 300, 44));
				labelTitle.Text = "Kies een vervaldatum:";
				labelTitle.TextAlignment = UITextAlignment.Center;
				FontHelper.SetFont (labelTitle);

				viewController.View.AddSubview (_toolbar);
				viewController.View.AddSubview (labelTitle);

				UIApplication.SharedApplication.KeyWindow.AddSubview (viewController.View);

				UIView.BeginAnimations ("slide");
				UIView.SetAnimationDuration (0.3);
				viewController.View.Frame = new CGRect (new CGPoint (0, UIApplication.SharedApplication.KeyWindow.Frame.Height - size), viewController.View.Frame.Size);
				UIView.CommitAnimations ();
			}


			else {
				var popover = _actionSheet.Superview.Superview;
				if (popover != null) {

					MARGIN = 50;

					var y = (UIScreen.MainScreen.ApplicationFrame.Width - _actionSheet.Frame.Height) / 2;
					var width = _actionSheet.Frame.Width - (MARGIN * 2);
					var height = _actionSheet.Frame.Height;

					popover.Frame = new CGRect (175, y, width, height);

					//_actionSheet.Frame = new RectangleF (x + 200, y, width - (CHROMEWIDTHLEFT + CHROMEWIDTHRIGHT), height - (CHROMEWIDTHLEFT + CHROMEWIDTHRIGHT));
					_actionSheet.Frame = new CGRect (400, y, 651, 239);

					picker.Frame = new CGRect (picker.Frame.X, picker.Frame.Y, _actionSheet.Frame.Width, picker.Frame.Height);

					_toolbar.SizeToFit ();
				}
			}
		}
			

	}

       
	public void Hide (bool animated)
	{
		if (UIDevice.CurrentDevice.CheckSystemVersion (8, 0)) {
			UIView.BeginAnimations ("slide");
			UIView.SetAnimationDuration (0.3);
			viewController.View.Frame = new CGRect (new CGPoint (0, UIApplication.SharedApplication.KeyWindow.Frame.Height), viewController.View.Frame.Size);
			UIView.CommitAnimations ();

			Task.Factory.StartNew (() => {
				Thread.Sleep (1000);
				_owner.InvokeOnMainThread (() => {
					viewController.View.RemoveFromSuperview ();
					viewController = null;
				});

			});
		} 
		else {
			_actionSheet.DismissWithClickedButtonIndex (0, animated);
		}
	}
        
             
}