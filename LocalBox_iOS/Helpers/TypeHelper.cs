using System;
using LocalBox_Common;

namespace LocalBox_iOS.Helpers
{
    public static class TypeHelper
    {

		public static string MapType(string path) {

			var type = MimeTypeHelper.GetMimeType(path);

			if (type == null) {
				return "icons/IcType-onbekend";
			} 
			else if (type.StartsWith ("image")) {
				return "icons/IcType-Foto";
			} 
			else if (type.StartsWith ("video")) {
				return "icons/IcType-Film";
			} 
			else if (type.Equals ("application/pdf")) {
				return "icons/IcType-PDF";
			} 
			else if (type.Equals ("application/vnd.ms-powerpoint") ||
			         type.Equals ("application/vnd.openxmlformats-officedocument.presentationml.presentation")) {
				return "icons/IcType-Presentatie";
			} 
			else if (type.Equals ("application/vnd.ms-excel") ||
			         type.Equals ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")) {
				return "icons/IcType-Berekeningen";
			} 
			else if (type.Equals ("application/msword") ||
			         type.Equals ("application/vnd.openxmlformats-officedocument.wordprocessingml.document")) {
				return "icons/IcType-Tekstdocumenten";
			}
			else if (type.Equals ("application/zip")) {
				return "icons/IcType-gecompileerd";
			}
			else if (type.Equals ("application/mp4")) {
				return "icons/IcType-Film";
			}
			else if (type.StartsWith("audio")){
                return "icons/IcType-Muziek";
            }
            else{
                return "icons/IcType-onbekend";
            }
        }

    }
}

