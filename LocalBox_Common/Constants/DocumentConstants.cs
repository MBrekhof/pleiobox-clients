using System;
using System.IO;

#if __IOS__
using UIKit;
using Foundation;
#endif

namespace LocalBox_Common
{
	public class DocumentConstants
	{
	
		public static string DocumentsPath
		{
			get {

#if __IOS__
				if (UIDevice.CurrentDevice.CheckSystemVersion (8,0)) // Code that uses features from Xamarin.iOS 8.0
				{
					var documents = NSFileManager.DefaultManager.GetUrls (NSSearchPathDirectory.DocumentDirectory, NSSearchPathDomain.User) [0].Path;
					return Path.Combine (documents, "..", "Library/Caches");

				} else {// Code to support earlier iOS versions
					var documents = Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
					return Path.Combine (documents, "..", "Library/Caches");
				}
#else
					return Environment.GetFolderPath (Environment.SpecialFolder.MyDocuments);
#endif
			}
		}



	}
}

