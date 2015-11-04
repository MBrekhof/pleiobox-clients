using System;

using Android.App;
using Android.Runtime;
using Android.Content;

using Xamarin;

namespace LocalBox_Droid
{
	[Application(Label = "@string/app_name", Theme = "@android:style/Theme.Holo.Light")]
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

			//Needed to call ServicePointManager.ServerCertificateValidationCallback for all sll web requests
			Environment.SetEnvironmentVariable ("MONO_TLS_SESSION_CACHE_TIMEOUT", "0");

			context = ApplicationContext;


			//Initialize Xamarin Insights => API key can be replaced with your own Xamarin Insights API key
			Insights.Initialize("91f27e3df29c7c6f973068c514b1d64cfd895fff", context);
        }

		public static Context GetAppContext()
		{
			return context;
		}
    }
}

