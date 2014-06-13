using System;

using Java.Lang;
using Java.IO;

using Android.Content;
using Android.Webkit;
using Android.OS;
using Android.Content.Res;

using LocalBox_Common;

namespace localbox.android
{
	[ContentProvider(new string[] { "com.belastingdienst.localbox" })]
	public class CustomContentProvider : ContentProvider
    {
		public static readonly string AUTHORITY = "com.belastingdienst.localbox";
        public static readonly Android.Net.Uri CONTENT_URI = Android.Net.Uri.Parse("content://" + AUTHORITY);

		public CustomContentProvider()
		{
		}

        public override bool OnCreate()
        {
            return true;
        }

        public override string GetType(Android.Net.Uri uri)
        {
			return MimeTypeHelper.GetMimeType(uri.ToString());
        }

		public override ParcelFileDescriptor OpenFile(Android.Net.Uri uri, string mode)
        {
            ParcelFileDescriptor[] pipe = null;

            try
            {
                pipe = ParcelFileDescriptor.CreatePipe();
				byte[] data = System.IO.File.ReadAllBytes(uri.Path);

               
                if (data == null)
                {
                    throw new Java.Lang.SecurityException();
                }

                new TransferThread(data,
                    new ParcelFileDescriptor.AutoCloseOutputStream(pipe[1])).Start();
            }
            catch (IOException e)
            {
                #if DEBUG
                Android.Util.Log.Error(GetType().Name, "Exception opening pipe", e);
                #endif
                throw new FileNotFoundException("Could not open pipe for: " + uri.ToString());
            }

            return(pipe[0]);

        }

        public override Android.Database.ICursor Query(Android.Net.Uri uri, string[] projection, string selection, string[] selectionArgs, string sortOrder)
        {

            return null;
        }

        public override Android.Net.Uri Insert(Android.Net.Uri uri, ContentValues values)
        {
            throw new Java.Lang.UnsupportedOperationException();
        }

        public override int Update(Android.Net.Uri uri, ContentValues values, string selection, string[] selectionArgs)
        {
            throw new Java.Lang.UnsupportedOperationException();
        }

        public override int Delete(Android.Net.Uri uri, string selection, string[] selectionArgs)
        {
            throw new Java.Lang.UnsupportedOperationException();
        }




        public class TransferThread : Thread
        {
            Java.IO.InputStream _in;
            Java.IO.OutputStream _out;

            public TransferThread(byte[] inData, Java.IO.OutputStream outStream)
            {
                this._in = new Java.IO.ByteArrayInputStream(inData);
                this._out = outStream;
                Daemon = true;
            }

            public override void Run()
            {

                byte[] buf = new byte[8192];
                int len;

                try {
                    while ((len = _in.Read(buf)) > 0) {
                        _out.Write(buf, 0, len);
                    }

                    _in.Close();
                    _out.Flush();
                    _out.Close();
                }
                catch (Java.IO.IOException e)
                {
                    Android.Util.Log.Error(GetType().Name, "Exception transferring file", e);
                }
            }
        }


    }
}