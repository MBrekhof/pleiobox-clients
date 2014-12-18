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
using System.Collections.Generic;

using LocalBox_Common;

namespace LocalBox_Droid
{
	public class PinEnterFragment : DialogFragment
    {
		private Button[] digitButtons;
		private TextView[] digits;
		private Button buttonOk;
		private ImageButton backButton;
		private StringBuilder pin;
		private int position;
        private const string TAG = "Pin";

		public static PinEnterFragment NewInstance()
        {
			PinEnterFragment pinFragment = new PinEnterFragment();
			return pinFragment;
        }

		public PinEnterFragment()
        {
            pin = new StringBuilder();
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString("pin", pin.ToString());
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            if (savedInstanceState != null)
            {
                pin = new StringBuilder(savedInstanceState.GetString("pin", ""));
                position = pin.Length;
            }

			View view = inflater.Inflate(Resource.Layout.Pincode, container);
            Dialog.SetTitle(Resource.String.pincode_opgeven);

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
            digits = new TextView[]
            {
                view.FindViewById<TextView>(Resource.Id.pindigit1),
                view.FindViewById<TextView>(Resource.Id.pindigit2),
                view.FindViewById<TextView>(Resource.Id.pindigit3),
                view.FindViewById<TextView>(Resource.Id.pindigit4),
                view.FindViewById<TextView>(Resource.Id.pindigit5)
            };

            for (int i = 0; i < digits.Length; i++)
            {
                if (i < position)
                {
                    digits[i].SetBackgroundResource(Resource.Drawable.bg_pincode_done);
                    digits[i].Text = "*";
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
			string fullPin = PinHelper.GetPinWithDeviceId (pin.ToString());

            if (pin.Length != digits.Length)
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
                builder.SetMessage(string.Format(Resources.GetString(Resource.String.pincode_lengte), digits.Length))
                    .SetTitle(Resource.String.pincode_incorrect_titel).SetPositiveButton("Ok", ((object s, Android.Content.DialogClickEventArgs f) =>
                {
					AlertDialog alertDialog;
					if ((alertDialog = s as AlertDialog) != null)
                    {
						alertDialog.Dismiss();
                    }
                }));

                AlertDialog dialog = builder.Create();
                dialog.Show();
			}
			else if (DataLayer.Instance.UnlockDatabase(fullPin))
			{

				if (HomeActivity.shouldLockApp) {
					HomeActivity.shouldLockApp = false;
					Activity.Finish ();
				} else {
					Activity.StartActivity(typeof(HomeActivity));
				}


			}
				
            else
            {
				if (DataLayer.Instance.loginAttempts < 5) {
					AlertDialog.Builder builder = new AlertDialog.Builder (Activity);
					builder.SetMessage (Resource.String.pincode_incorrect)
                    .SetTitle (Resource.String.pincode_incorrect_titel).SetPositiveButton ("Ok", ((object s, Android.Content.DialogClickEventArgs f) => {
						AlertDialog alertDialog;
						if ((alertDialog = s as AlertDialog) != null) {
							alertDialog.Dismiss ();
						}
					}));

					AlertDialog dialog = builder.Create ();
					dialog.Show ();
				} else {

					AlertDialog.Builder builder = new AlertDialog.Builder (Activity);
					builder.SetMessage (Resource.String.pincode_incorrect_vijfmaal)
						.SetTitle (Resource.String.pincode_incorrect_titel).SetPositiveButton ("Ok", ((object s, Android.Content.DialogClickEventArgs f) => {
							AlertDialog alertDialog;
							if ((alertDialog = s as AlertDialog) != null) {
								alertDialog.Dismiss ();

								//Verander login fragment
								PinActivity pinActivity = (PinActivity)Activity;
								pinActivity.OpenCorrectPinDialog ();
							}
						}));

					AlertDialog dialog = builder.Create ();
					dialog.Show ();
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
                digits[position].SetBackgroundResource(Resource.Drawable.bg_pincode_active);
                digits[position].Text = "";
                pin.Remove(pin.Length - 1, 1);
            }
        }

        void HandleClickDigit(int digit)
        {
            if (position < digits.Length)
            {
                digits[position].SetBackgroundResource(Resource.Drawable.bg_pincode_done);
                digits[position++].Text = "*";
                if (position < digits.Length)
                {
                    digits[position].SetBackgroundResource(Resource.Drawable.bg_pincode_active);
                }
                pin.Append(digit);
            }
        }

    }
}

