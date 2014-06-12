
using System;
using System.IO;
using System.Security.Cryptography;

namespace LocalBox_Common
{
	public class AESEncrypter
	{

		//Encrypt a byte array into a byte array using a key and an IV
		public static byte[] Encrypt (byte[] clearData, byte[] Key, byte[] IV)
		{ 
			//Create a MemoryStream to accept the encrypted bytes 
			MemoryStream memoryStream = new MemoryStream (); 

			Rijndael rijndael = Rijndael.Create (); 
			rijndael.Key = Key; 
			rijndael.IV = IV; 
			CryptoStream cryptoStream = new CryptoStream (memoryStream, rijndael.CreateEncryptor (), CryptoStreamMode.Write); 

			//Write the data and make it do the encryption 
			cryptoStream.Write (clearData, 0, clearData.Length); 

			cryptoStream.Close (); 
		
			byte[] encryptedData = memoryStream.ToArray ();

			return encryptedData; 
		}




		//Decrypt a byte array into a byte array using a key and an IV
		public static byte[] Decrypt (byte[] cipherData, byte[] Key, byte[] IV)
		{ 
			//Create a MemoryStream to accept the decrypted bytes 
			MemoryStream memoryStream = new MemoryStream (); 

			Rijndael rijndael = Rijndael.Create (); 
			rijndael.Key = Key; 
			rijndael.IV = IV; 

			CryptoStream cryptoStream = new CryptoStream (memoryStream, rijndael.CreateDecryptor (), CryptoStreamMode.Write); 

			//Write the data and make it do the decryption 
			cryptoStream.Write (cipherData, 0, cipherData.Length); 

			cryptoStream.Close (); 

			byte[] decryptedData = memoryStream.ToArray (); 

			return decryptedData; 
		}

	}
}