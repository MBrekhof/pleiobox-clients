using System;

using UIKit;

namespace LocalBox_iOS
{
	public class PinHelper
	{
		public PinHelper ()
		{
		}

		public static string GetPinWithDeviceId(string pin)
		{
			#if DEBUG
			return "6169E326-FE17-446B-AF6A-370D4536340A" + pin;
			#else
			return UIDevice.CurrentDevice.IdentifierForVendor.AsString () + pin;
			#endif
		}
	}
}

