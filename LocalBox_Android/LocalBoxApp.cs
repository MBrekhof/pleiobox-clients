using System;

using Android.App;
using Android.Runtime;
using Android.Content;

namespace LocalBox_Droid
{
	[Application(Label = "LocalBox", Theme = "@android:style/Theme.Holo.Light")]
    public class LocalBoxApp : Application
    {
		private static Context context;

        public LocalBoxApp (IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public LocalBoxApp ()
        {
        }

        public override void OnCreate ()
        {
            base.OnCreate();

			context = ApplicationContext;
        }

		public static Context GetAppContext()
		{
			return context;
		}
    }
}

