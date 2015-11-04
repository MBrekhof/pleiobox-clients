using System;
using System.IO;

using Java.Lang;
using Java.IO;

using Android.Content;
using Android.Webkit;
using Android.OS;
using Android.Content.Res;

using LocalBox_Common;

using Xamarin;

namespace LocalBox_Droid
{
	[ContentProvider(new string[] { "com.pleio.pleiobox" }, Exported = true)]
	public class CustomContentProvider : ContentProvider
	{
		public static readonly string AUTHORITY = "com.pleio.pleiobox";
		public static readonly Android.Net.Uri CONTENT_URI = Android.Net.Uri.Parse("content://" + AUTHORITY);

		public CustomContentProvider()
		{
			System.Console.WriteLine ("Custom Content Provider Initialized");
		}


		public override int Delete (Android.Net.Uri uri, string selection, string[] selectionArgs)
		{
			return 0;
		}

		public override string GetType (Android.Net.Uri uri)
		{
			var mimeType = MimeTypeHelper.GetMimeType(uri.ToString());

			return mimeType;
		}

		public override Android.Net.Uri Insert (Android.Net.Uri uri, ContentValues values)
		{
			return null;
		}

		public override bool OnCreate ()
		{
			return true;
		}

		public override ParcelFileDescriptor OpenFile(Android.Net.Uri uri, string mode)
		{
			string fileName = uri.ToString ().Substring (CONTENT_URI.ToString ().Length);

			System.Console.WriteLine (fileName + " - File to open in external app");
			if (System.IO.File.Exists (fileName)) {
				return ParcelFileDescriptor.Open(new Java.IO.File(fileName), ParcelFileMode.ReadOnly);
			}

			return null;
		}

		public override Android.Database.ICursor Query (Android.Net.Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
		{
			return null;
		}

		public override int Update (Android.Net.Uri uri, ContentValues values, string selection, string[] selectionArgs)
		{
			return 0;
		}

	}
}