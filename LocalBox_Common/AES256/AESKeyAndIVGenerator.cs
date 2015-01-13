using System;
using System.Security.Cryptography;

namespace LocalBox_Common
{
	public class AESKeyAndIVGenerator
	{
		private RijndaelManaged rijndaelManaged;
		public 	Byte[] aesKey 	{ get; private set; }
		public 	Byte[] aesIV 	{ get; private set; }

		public AESKeyAndIVGenerator ()
		{
            rijndaelManaged = new RijndaelManaged();
            rijndaelManaged.KeySize = 256;

			rijndaelManaged.GenerateKey ();
			rijndaelManaged.GenerateIV ();

			aesKey = rijndaelManaged.Key;
			aesIV = rijndaelManaged.IV;
		}
	}
}

