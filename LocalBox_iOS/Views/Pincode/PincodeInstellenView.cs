using System;
using MonoTouch.UIKit;
using System.Drawing;
using LocalBox_Common;

namespace LocalBox_iOS.Views
{
    public class PincodeInstellenView : UIView
    {
        Keypad _keypad;
        PinInput _firstPinInput;
        PinInput _secondPinInput;
        PinInput _currentPinInput;

        readonly UIView _contentView;

        public event EventHandler OnPinFilled;


        public PincodeInstellenView(RectangleF frame)
        {
            Frame = frame;
            BackgroundColor = UIColor.White;
            _contentView = new UIView(new RectangleF(0, 0, frame.Width, frame.Height));
            _contentView.BackgroundColor = UIColor.Clear;

            AddComponents();

            Layer.ShadowColor = UIColor.FromRGBA(0f, 0f, 0f, .8f).CGColor;
            Layer.ShadowOpacity = 0.7f;
            Layer.ShadowRadius = 5.0f;
            Layer.ShadowPath = UIBezierPath.FromRect(new RectangleF(-5, 0, Bounds.Width, Bounds.Height)).CGPath;

            Add(_contentView);
        }

        void AddComponents()
        {
            float y = Frame.Height - 224 - 16;
            _keypad = new Keypad(new RectangleF(14, y, 342, 226));
            _keypad.OnBackSpace += HandleOnBackSpace;
            _keypad.OnDigitPressed += HandleOnDigitPressed;

            _contentView.Add(_keypad);
            UIColor fontColor = UIColor.FromRGB(103, 103, 103);
            UILabel header = new UILabel(new RectangleF(14, 48, 342, 26));
            header.Font = UIFont.BoldSystemFontOfSize(14);
            header.TextColor = fontColor;
            header.Text = "Pincode invoeren";
            _contentView.Add(header);

            UILabel labelPin1 = new UILabel(new RectangleF(14, 76, 342, 26));
            labelPin1.Font = UIFont.SystemFontOfSize(14);
            labelPin1.TextColor = fontColor;
            labelPin1.Text = "Kies een pincode om uw gegevens te beveiligen.";
            _contentView.Add(labelPin1);

            _firstPinInput = new PinInput(new RectangleF(14, labelPin1.Frame.Bottom + 8f, 342, 62));
            _contentView.Add(_firstPinInput);

            UILabel labelPin2 = new UILabel(new RectangleF(14, _firstPinInput.Frame.Bottom + 24, 342, 26));
            labelPin2.Text = "Bevestig uw zelfgekozen pincode.";
            labelPin2.Font = UIFont.SystemFontOfSize(14);
            labelPin2.TextColor = fontColor;
            _contentView.Add(labelPin2);

            _secondPinInput = new PinInput(new RectangleF(14, labelPin2.Frame.Bottom + 8, 342, 62));
            _contentView.Add(_secondPinInput);

            _currentPinInput = _firstPinInput;
            _currentPinInput.CurrentPositionVisible = true;
            _secondPinInput.Disabled = true;

            _firstPinInput.OnPinFilled += (sender, e) =>
            {
                _currentPinInput.Done = true;
                _currentPinInput = _secondPinInput;
                _secondPinInput.CurrentPositionVisible = true;
                _secondPinInput.Disabled = false;
            };
            _secondPinInput.OnPinFilled += ValidatePin;
        }

        async void ValidatePin(object sender, EventArgs e)
        {
            if (_firstPinInput.Equals(_secondPinInput))
            {
				string fullPin = PinHelper.GetPinWithDeviceId (_firstPinInput.Pin);

				bool valid = await DataLayer.Instance.ValidatePincode(fullPin);
                if (valid)
                {
					DataLayer.Instance.UnlockDatabase(fullPin);
                    RemoveFromSuperview();
                    if (OnPinFilled != null)
                    {
                        OnPinFilled(this, EventArgs.Empty);
                    }
                }
            }
            else
            {
                _firstPinInput.Reset();
                _secondPinInput.Reset();
                _firstPinInput.CurrentPositionVisible = true;
                _secondPinInput.Disabled = true;
                _currentPinInput = _firstPinInput;
            }
        }

        void HandleOnDigitPressed(int digit)
        {
            _currentPinInput.SetDigit(digit);
        }

        void HandleOnBackSpace(object sender, EventArgs e)
        {
            if (!_currentPinInput.RemoveDigit() && _currentPinInput == _secondPinInput)
            {
                _currentPinInput.CurrentPositionVisible = false;
                _currentPinInput = _firstPinInput;
                _currentPinInput.RemoveDigit();
                _currentPinInput.Disabled = false;
                _secondPinInput.Disabled = true;
            }
        }
    }
}

