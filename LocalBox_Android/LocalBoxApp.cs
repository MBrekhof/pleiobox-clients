using System;

using Android.App;
using Android.Runtime;

namespace localbox.android
{
	[Application(Label = "LocalBox", Theme = "@android:style/Theme.Holo.Light")]
    public class LocalBoxApp : Application
    {
        public LocalBoxApp (IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public LocalBoxApp ()
        {
        }

        public override void OnCreate ()
        {
            base.OnCreate();
        }

    }
}

