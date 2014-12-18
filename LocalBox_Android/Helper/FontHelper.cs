using System;
using Android.Widget;
using Android.Graphics;
using Android.App;

namespace LocalBox_Droid
{
	public class FontHelper
	{
		private static Typeface typeFace = Typeface.CreateFromAsset(Application.Context.Assets, "Roboto-Regular.ttf");

		public static void SetFont(Button button){
			button.SetTypeface(typeFace, TypefaceStyle.Normal);
		}

		public static void SetFont(TextView textView){
			textView.SetTypeface(typeFace, TypefaceStyle.Normal);
		}


	}
}

