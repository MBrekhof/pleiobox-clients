using System;
using UIKit;
using CoreGraphics;
using System.Collections.Generic;

namespace LocalBox_iOS
{
    public class Keypad : UIView
    {
        public delegate void DigitPressed(int digit);

        public event DigitPressed OnDigitPressed;
        public event EventHandler OnBackSpace;

        private nfloat _buttonHeight;
        private nfloat _buttonWidth;
        public static UIColor GridBackgroundColor = UIColor.FromRGB(202, 207, 211);
//        public static UIColor GridBackgroundColor = UIColor.FromRGB(103, 103, 103);
        public static UIColor ButtonBackgroundColor = UIColor.FromRGB(0x8f, 0xca, 0xe7);
        public static UIColor ButtonDownBackgroundColor = UIColor.FromRGB(0x68, 0xa8, 0xc8);
        public static UIColor DigitColor = UIColor.FromRGB(103, 103, 103);

        public Keypad(CGRect bounds)
        {
            BackgroundColor = GridBackgroundColor;
            Frame = bounds;

            _buttonHeight = ((Frame.Height - 2) / 4f) - (3f / 4f);
            _buttonWidth = ((Frame.Width - 2) / 3f) - (2f / 3f);
            AutoresizingMask = UIViewAutoresizing.FlexibleDimensions;

            CreateGrid();
        }

        private void CreateGrid()
        {
            UIView[] gridButtons = CreateGridButtons();
            for (int i = 0; i < gridButtons.Length; i++)
            {
                var button = gridButtons[i];
                nfloat x = 1 + (i % 3) * _buttonWidth + 1f * (i % 3);
                nfloat y = 1 + ((i / 3) % 4) * _buttonHeight + 1f * ((i / 3) % 4);
                button.Frame = new CGRect(x, y, _buttonWidth, _buttonHeight);
                button.AutoresizingMask = UIViewAutoresizing.All;
                Add(button);
            }
        }

        private UIView[] CreateGridButtons()
        {
            List<UIView> gridButtons = new List<UIView>();
            for (int i = 1; i <= 9; i++)
            {
                gridButtons.Add(CreateButton(i));
            }

            gridButtons.Add(CreateClearButton());
            gridButtons.Add(CreateButton(0));
            gridButtons.Add(CreateBackButton());
            return gridButtons.ToArray();
        }

        private UIButton CreateButton(int digit)
        {
            var button = UIButton.FromType(UIButtonType.Custom);
            StyleButton(button);
            button.Font = UIFont.SystemFontOfSize(20);
            button.SetTitle(digit.ToString(), UIControlState.Normal);
            button.SetTitleColor(DigitColor, UIControlState.Normal);
            button.TouchUpInside += (object sender, EventArgs e) =>
            {
                if (OnDigitPressed != null)
                {
                    OnDigitPressed(digit);
                }
            };

            return button;
        }

        private UIButton CreateBackButton()
        {
            var button = UIButton.FromType(UIButtonType.Custom);
            StyleButton(button);
            button.Font = UIFont.SystemFontOfSize(20);
            button.SetTitleColor(DigitColor, UIControlState.Normal);
//            button.SetImage(UIImage.FromBundle("Assets/icons/icon_backspace"), UIControlState.Normal);
//            button.ImageView.ContentMode = UIViewContentMode.Center;
            button.SetTitle("<", UIControlState.Normal);
            button.TouchUpInside += (object sender, EventArgs e) =>
            {
                if (OnBackSpace != null)
                {
                    OnBackSpace(this, EventArgs.Empty);
                }
            };

            return button;
        }

        private UIView CreateClearButton()
        {
            var button = new UIView();
            button.BackgroundColor = ButtonBackgroundColor;

            return button;
        }

        private void StyleButton(UIButton button)
        {
            button.ExclusiveTouch = true;
            button.BackgroundColor = ButtonBackgroundColor;
            button.TouchDown += delegate
            {
                button.BackgroundColor = ButtonDownBackgroundColor;
            };

            button.TouchUpInside += delegate
            {
                button.BackgroundColor = ButtonBackgroundColor;
            };

            button.TouchUpOutside += delegate
            {
                button.BackgroundColor = ButtonBackgroundColor;
            };

        }
    }

}

