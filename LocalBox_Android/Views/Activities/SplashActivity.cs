using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using LocalBox_Common;

namespace LocalBox_Droid
{	
	//Activity settings
	[Activity(Theme = 	"@style/Theme.Splash", MainLauncher = true, NoHistory = true, 
					  	ScreenOrientation = Android.Content.PM.ScreenOrientation.Landscape, 
						WindowSoftInputMode = SoftInput.AdjustPan)]

	//Intent filter to register new LocalBox
	//[IntentFilter(new[] { Intent.ActionView }, 
	//					Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = "lbox")] 

	//Intent filters to save pdf annotations made in external app
	[IntentFilter(new[] { Intent.ActionView }, 
		Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable, Intent.ActionSend}, DataMimeType="application/pdf", DataScheme="file", DataHost="*")]

	[IntentFilter(new[] { Intent.ActionSend }, Categories = new[] { Intent.CategoryDefault }, DataMimeType = "application/pdf")]  


	public class SplashActivity : Activity
	{
		public static Android.Net.Uri intentData;
		public static ClipData clipData;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			Thread.Sleep(2000); //milliseconds to show splash
			StartActivity(typeof(PinActivity));
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			//Used in HomeActivity to save pdf annotations
			if (Intent.Data != null) { 
				intentData = Intent.Data;
			} else if (Intent.ClipData != null) {
				clipData = Intent.ClipData;
			}
			else {
				intentData = null;
			}
		}

	}
}