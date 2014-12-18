using System;

namespace LocalBox_Droid
{
	public class PinHelper
	{
		public PinHelper ()
		{
		}

		public static string GetPinWithDeviceId(string pin)
		{
			return global::Android.Provider.Settings.Secure.GetString (
				LocalBoxApp.GetAppContext ().ContentResolver, global::Android.Provider.Settings.Secure.AndroidId) + pin;
		}
	}
}

