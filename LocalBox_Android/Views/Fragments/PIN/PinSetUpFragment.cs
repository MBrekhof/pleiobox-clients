using System;
using System.Text;
using System.Threading;

using Android.App;
using Android.Views;
using Android.OS;
using Android.Widget;
using Android.Content;
using Android.Preferences;
using Android.Util;

using LocalBox_Common;

namespace localbox.android
{
	public class PinSetUpFragment : DialogFragment
    {
		Button[] digitButtons;
		TextView[] digits;
		Button buttonOk;
		ImageButton backButton;
		int position;
		StringBuilder currentPin;
		StringBuilder pin;
		StringBuilder pinConfirm;
	
        private const string TAG = "Pin";

		public static PinSetUpFragment NewInstance()
        {
			PinSetUpFragment pinFragment = new PinSetUpFragment();
			return pinFragment;

        }

		public PinSetUpFragment()
        {
            pin = new StringBuilder();
            pinConfirm = new StringBuilder();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString("pin", pin.ToString());
            outState.PutString("pinConfirm", pin.ToString());
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            currentPin = pin;
            if (savedInstanceState != null)
            {
                pin = new StringBuilder(savedInstanceState.GetString("pin", ""));
                pinConfirm = new StringBuilder(savedInstanceState.GetString("pinConfirm", ""));
                position = pin.Length + pinConfirm.Length;
            }

			View view = inflater.Inflate(Resource.Layout.Pincode_instellen, container);
            Dialog.SetTitle(Resource.String.pincode_instellen);

			InitializeDigits(view);
			InitializeButtons(view);
            Cancelable = false;

			return view;
        }

        public override void OnResume()
        {
            base.OnResume();
            Dialog.Window.SetLayout((int)Resources.GetDimension(Resource.Dimension.pin_dialog_width), (int)Resources.GetDimension(Resource.Dimension.pin_dialog_height));
        }
        private void InitializeDigits(Android.Views.View view)
        {
            var firstRow = view.FindViewById<LinearLayout>(Resource.Id.digits);
            var secondRow = view.FindViewById<LinearLayout>(Resource.Id.digits_confirm);
            digits = new []
            {
                firstRow.FindViewById<TextView>(Resource.Id.pindigit1),
                firstRow.FindViewById<TextView>(Resource.Id.pindigit2),
                firstRow.FindViewById<TextView>(Resource.Id.pindigit3),
                firstRow.FindViewById<TextView>(Resource.Id.pindigit4),
                firstRow.FindViewById<TextView>(Resource.Id.pindigit5),
                secondRow.FindViewById<TextView>(Resource.Id.pindigit1),
                secondRow.FindViewById<TextView>(Resource.Id.pindigit2),
                secondRow.FindViewById<TextView>(Resource.Id.pindigit3),
                secondRow.FindViewById<TextView>(Resource.Id.pindigit4),
                secondRow.FindViewById<TextView>(Resource.Id.pindigit5)
            };

            for (int i = 0; i < digits.Length; i++)
            {
                if (i < position)
                {
                    digits[i].SetBackgroundResource(Resource.Drawable.bg_pincode_done);
                    digits[i].Text = "*";
                    if (i == 5)
                    {
                        currentPin = pinConfirm;
                    }
                }
            }
            if (position < digits.Length)
            {
                digits[position].SetBackgroundResource(Resource.Drawable.bg_pincode_active);
            }
        }

        private void InitializeButtons(Android.Views.View view)
        {
            digitButtons = new Button[]
            {
                view.FindViewById<Button>(Resource.Id.button0),
                view.FindViewById<Button>(Resource.Id.button1),
                view.FindViewById<Button>(Resource.Id.button2),
                view.FindViewById<Button>(Resource.Id.button3),
                view.FindViewById<Button>(Resource.Id.button4),
                view.FindViewById<Button>(Resource.Id.button5),
                view.FindViewById<Button>(Resource.Id.button6),
                view.FindViewById<Button>(Resource.Id.button7),
                view.FindViewById<Button>(Resource.Id.button8),
                view.FindViewById<Button>(Resource.Id.button9)
            };
            backButton = view.FindViewById<ImageButton>(Resource.Id.back);
            buttonOk = view.FindViewById<Button>(Resource.Id.ok);

			backButton.Click += HandleClickBack;
            buttonOk.Click += HandleClickOk;

            for (int i = 0; i < digitButtons.Length; i++)
            {
                int j = i;
                digitButtons[i].Click += delegate
                {
                    HandleClickDigit(j);
                };
            }
        }

        void HandleClickOk(object sender, EventArgs e)
        {
            if (pin.Length + pinConfirm.Length != digits.Length)
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                builder.SetMessage(string.Format(Resources.GetString(Resource.String.pincode_lengte), digits.Length))
                    .SetTitle(Resource.String.pincode_incorrect_titel).SetPositiveButton("Ok", ((object s, Android.Content.DialogClickEventArgs f) =>
                {
                    AlertDialog ad;
                    if ((ad = s as AlertDialog) != null)
                    {
                        ad.Dismiss();
                    }
                }));

                AlertDialog dialog = builder.Create();
                dialog.Show();
			}
            else 
			{
                if(pin.ToString().Equals(pinConfirm.ToString())) {

					bool unlockSucceeded = DataLayer.Instance.UnlockDatabase(pin.ToString());

					if (unlockSucceeded)
                    {
                        Activity.StartActivity(typeof(HomeActivity));
                    }
                    else
					{ 
						Toast.MakeText (Activity, "Er is iets fout gegaan", ToastLength.Short).Show ();
					}
                }
				else {
                    AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                    builder.SetMessage(string.Format(Resources.GetString(Resource.String.pincode_niet_overeen), digits.Length))
                        .SetTitle(Resource.String.pincode_incorrect_titel).SetPositiveButton("Ok", ((object s, Android.Content.DialogClickEventArgs f) =>
                        {
                            AlertDialog ad;
                            if ((ad = s as AlertDialog) != null)
                            {
                                ad.Dismiss();
                            }
                        }));

                    AlertDialog dialog = builder.Create();
                    dialog.Show();
				}
            }
        }


        void HandleClickBack(object sender, EventArgs e)
        {
            if (position > 0)
            {
                if (position < digits.Length)
                {
                    digits[position].SetBackgroundResource(Resource.Drawable.bg_pincode);
                }
                position--;
                if (position == 4)
                {
                    currentPin = pin;
                }

                digits[position].SetBackgroundResource(Resource.Drawable.bg_pincode_active);
                digits[position].Text = "";
                currentPin.Remove(currentPin.Length - 1, 1);
            }
        }

        void HandleClickDigit(int digit)
        {
            if (position < digits.Length)
            {
                digits[position].SetBackgroundResource(Resource.Drawable.bg_pincode_done);
                digits[position++].Text = "*";
                if (position < digits.Length)
                    digits[position].SetBackgroundResource(Resource.Drawable.bg_pincode_active);
                if (position == 6)
                {
                    currentPin = pinConfirm;

                }
                currentPin.Append(digit);
            }
        }

    }
}

