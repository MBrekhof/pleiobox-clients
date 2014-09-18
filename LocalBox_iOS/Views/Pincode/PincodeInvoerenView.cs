using System;
using MonoTouch.UIKit;
using System.Drawing;
using LocalBox_Common;
using LocalBox_iOS.Helpers;

namespace LocalBox_iOS.Views
{
    public class PincodeInvoerenView : UIView
    {
        Keypad _keypad;
        PinInput _pinInput;

        readonly UIView _contentView;

        public event EventHandler OnPinFilled;


        public PincodeInvoerenView(RectangleF frame)
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
            header.Text = "Pincode invoeren";
            header.TextColor = fontColor;
            _contentView.Add(header);

            UILabel labelPin1 = new UILabel(new RectangleF(14, 76, 342, 26));
            labelPin1.Font = UIFont.SystemFontOfSize(14);
            labelPin1.Text = "Voer uw pincode in om verder te gaan.";
            labelPin1.TextColor = fontColor;
            _contentView.Add(labelPin1);

            _pinInput = new PinInput(new RectangleF(14, labelPin1.Frame.Bottom + 8f, 342, 62));
            _contentView.Add(_pinInput);

            _pinInput.OnPinFilled += ValidatePin;
        }

        async void ValidatePin(object sender, EventArgs e)
        {

			string fullPin = PinHelper.GetPinWithDeviceId (_pinInput.Pin);

			bool valid = await DataLayer.Instance.ValidatePincode(fullPin);
            if (valid)
            {
                _pinInput.Done = true;

				DataLayer.Instance.UnlockDatabase(fullPin);
                if (OnPinFilled != null)
                {
                    OnPinFilled(this, EventArgs.Empty);
                }
            }
            else
            {
                DialogHelper.ShowErrorDialog("Pincode incorrect", "De opgegeven pincode is niet correct");
                _pinInput.Reset();
                _pinInput.CurrentPositionVisible = false;
            }
        }

        void HandleOnDigitPressed(int digit)
        {
            _pinInput.SetDigit(digit);
        }

        void HandleOnBackSpace(object sender, EventArgs e)
        {
            _pinInput.RemoveDigit();
        }
    }
}

