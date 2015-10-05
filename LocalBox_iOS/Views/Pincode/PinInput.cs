using System;
using UIKit;
using System.Linq;
using CoreGraphics;

namespace LocalBox_iOS
{
    public class PinInput : UIView
    {
        private PinDigit[] _pinDigits;
        private readonly nfloat DigitHeight;
        private nfloat DigitWidth = 62f; //
        public int _currentIndex = 0;

        public static readonly UIColor DigitBackgroundColor = UIColor.FromRGB(170, 170, 170);
        public static readonly UIColor DigitDisabledBackgroundColor = UIColor.FromRGB(170, 170, 170);
        public static readonly UIColor DigitDoneBackgroundColor = UIColor.FromRGB(113, 113, 113);
        public static readonly UIColor DigitFilledBackgroundColor = UIColor.FromRGB(113, 113, 113);
        public static readonly UIColor BorderColor = UIColor.FromRGB(0x3d, 0x79, 0x96);

        public event EventHandler OnPinFilled;

        public string Pin
        {
            get
            {
                return _pinDigits.Select(e => e.Digit.HasValue ? e.Digit.Value.ToString() : "").Aggregate((current, next) => current + next);
            }
        }

        public bool CurrentPositionVisible
        {
            get
            {
                return _currentIndex >= _pinDigits.Length && _pinDigits[_currentIndex].CurrentPositionVisible;
            }
            set
            {
                if (_currentIndex < _pinDigits.Length)
                {
                    _pinDigits[_currentIndex].CurrentPositionVisible = value;
                }
            }
        }

        public bool Disabled
        {
            get
            {
                return _pinDigits[0].Disabled;
            }
            set
            {
                Array.ForEach(_pinDigits, (e) => e.Disabled = value);
            }
        }

        public bool Done
        {
            get
            {
                return _pinDigits[0].Done;
            }
            set
            {
                Array.ForEach(_pinDigits, (e) => e.Done = value);
            }
        }

        public PinInput(CGRect frame)
        {
            Frame = frame;
            DigitHeight = frame.Height;
            _pinDigits = new PinDigit[5];

            for (int i = 0; i < 5; i++)
            {
                var digit = new PinDigit(new CGRect(((Frame.Width - DigitWidth) / 4) * i, 0, DigitWidth, DigitHeight));
                Add(digit);
                _pinDigits[i] = digit;
            }
        }

        public void SetDigit(int digit)
        {
            if (_currentIndex < _pinDigits.Length)
            {
                _pinDigits[_currentIndex].Digit = digit;
                CurrentPositionVisible = false;

                _currentIndex++;
                if (_currentIndex + 1 <= _pinDigits.Length)
                {
                    CurrentPositionVisible = true;
                }
                else
                {
                    if (OnPinFilled != null)
                    {
                        OnPinFilled(this, EventArgs.Empty);
                    }
                }
            }
        }

        public bool RemoveDigit()
        {
            if (_currentIndex > 0)
            {
                CurrentPositionVisible = false;
                _currentIndex--;
                _pinDigits[_currentIndex].Digit = null;
                CurrentPositionVisible = true;
                return true;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            PinInput pi;
            if ((pi = obj as PinInput) != null)
            {
                return pi.Pin.Equals(Pin);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Pin.GetHashCode();
        }

        public void Reset()
        {
            Array.ForEach(_pinDigits, (e) =>
            {
                e.Digit = null;
            });
            CurrentPositionVisible = false;
            _currentIndex = 0;
        }

        private class PinDigit : UIView
        {
            private int? _digit;
            private UILabel _bullet;

            public int? Digit
            {
                get
                {
                    return _digit;
                }
                set
                {
                    _digit = value;
                    _bullet.Hidden = value == null;
                    BackgroundColor = value == null ? DigitBackgroundColor : DigitFilledBackgroundColor;
                }
            }

            public bool CurrentPositionVisible
            {
                get
                {
                    return Layer.BorderWidth == 1f;
                }
                set
                {
                    Layer.BorderWidth = value ? 1f : 0f;
                }
            }

            public bool Disabled
            {
                get
                {
                    return BackgroundColor.Equals(DigitDisabledBackgroundColor);
                }
                set
                {
                    BackgroundColor = value ? DigitDisabledBackgroundColor : DigitBackgroundColor;
                }
            }

            public bool Done
            {
                get
                {
                    return BackgroundColor.Equals(DigitDoneBackgroundColor);
                }
                set
                {
                    BackgroundColor = value ? DigitDoneBackgroundColor : DigitBackgroundColor;
                }
            }

            public PinDigit(CGRect frame)
            {
                Frame = frame;
                Layer.CornerRadius = 3;
                Layer.BorderColor = BorderColor.CGColor;
                BackgroundColor = DigitBackgroundColor;
                AutoresizingMask = UIViewAutoresizing.FlexibleLeftMargin;
                // Verticale positie van de * kan alleen getweaked worden (niet berekend).
                // Het font bepaald de positie binnen het label.
                nfloat topOffset = frame.Height / 2.9f;

                _bullet = new UILabel(new CGRect(0f, topOffset, Bounds.Width, 26));
                _bullet.TextAlignment = UITextAlignment.Center;
                _bullet.Font = UIFont.BoldSystemFontOfSize(22);
                _bullet.BackgroundColor = UIColor.Clear;
                _bullet.Hidden = true;
                _bullet.Text = "*";

                Add(_bullet);
            }
        }
    }}

