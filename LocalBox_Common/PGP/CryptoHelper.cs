using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
// using Bouncy Castle library: http://www.bouncycastle.org/csharp/
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Math;

namespace LocalBox_Common
{
    public static class CryptoHelper
    {

        public static readonly string PRIVATE_KEY_PATH = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "private.asc");
        public static readonly string PUBLIC_KEY_PATH = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), "public.asc");

        private static readonly int KEY_STRENGTH = 2048;

        private static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);
            if (pgpSecKey == null)
            {
                return null;
            }

            return pgpSecKey.ExtractPrivateKey(pass);
        }

        private static PgpPublicKey ReadPublicKey(Stream inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(inputStream);

            foreach (PgpPublicKeyRing keyRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey key in keyRing.GetPublicKeys())
                {
                    if (key.IsEncryptionKey)
                    {
                        return key;
                    }
                }
            }

            throw new ArgumentException("Can't find encryption key in key ring.");
        }

        public static bool ValidatePassPhrase(byte[] privateKey, string passPhrase) {
            using (MemoryStream privateKeyStream = new MemoryStream(privateKey))
            {
                PgpSecretKeyRingBundle pgpKeyRing = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));
                var items = pgpKeyRing.GetKeyRings();
                foreach (var item in items)
                {
                    try {
                        var i = (PgpSecretKeyRing) item;
                        var key = i.GetSecretKey().ExtractPrivateKey(passPhrase.ToCharArray());
                        return true;
                    } catch {
                        return false;
                    }
                }
            }

            return true;
        }


        public static byte[] EncryptPgp(byte[] input, byte[] publicKey) {
            using (MemoryStream publicKeyStream = new MemoryStream(publicKey))
            using (MemoryStream outputStream = new MemoryStream())
            using (MemoryStream encryptedBytes = new MemoryStream())
            {


                using (Stream s = new PgpLiteralDataGenerator().Open(outputStream, PgpLiteralData.Binary, PgpLiteralDataGenerator.Console, input.Length, DateTime.Now))
                using (Stream inputStream = new MemoryStream(input))
                {
                    s.Write(input, 0, input.Length);
                }

                PgpPublicKey pubKey = ReadPublicKey(publicKeyStream);

                PgpEncryptedDataGenerator dataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256, true, new SecureRandom());

                dataGenerator.AddMethod(pubKey);

                byte[] output = outputStream.ToArray();

                using(Stream dgenStream = dataGenerator.Open(encryptedBytes, output.Length)) {

                    dgenStream.Write(output, 0, output.Length);

                }

                dataGenerator.Close();

                return encryptedBytes.ToArray();

            }
        }


        public static byte[] DecryptPgp(byte[] input, byte[] privateKeyStream, string passPhrase)
        {
            byte[] output;
            using(MemoryStream inputStream = new MemoryStream(input)) {
                inputStream.Position = 0;

                PgpObjectFactory pgpFactory = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));
                // find secret key
                PgpSecretKeyRingBundle pgpKeyRing = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(new MemoryStream(privateKeyStream)));

                PgpObject pgp = null;
                if (pgpFactory != null)
                {
                    pgp = pgpFactory.NextPgpObject();
                }

                // the first object might be a PGP marker packet.
                PgpEncryptedDataList encryptedData = null;
                if (pgp is PgpEncryptedDataList)
                {
                    encryptedData = (PgpEncryptedDataList)pgp;
                }
                else
                {
                    encryptedData = (PgpEncryptedDataList)pgpFactory.NextPgpObject();
                }

                // decrypt
                PgpPrivateKey privateKey = null;
                PgpPublicKeyEncryptedData pubKeyData = null;
                foreach (PgpPublicKeyEncryptedData pubKeyDataItem in encryptedData.GetEncryptedDataObjects())
                {
                    privateKey = FindSecretKey(pgpKeyRing, pubKeyDataItem.KeyId, passPhrase.ToCharArray());

                    if (privateKey != null)
                    {
                        pubKeyData = pubKeyDataItem;
                        break;
                    }
                }

                if (privateKey == null)
                {
                    throw new ArgumentException("Secret key for message not found.");
                }

                PgpObjectFactory plainFact = null;
                using (Stream clear = pubKeyData.GetDataStream(privateKey))
                {
                    plainFact = new PgpObjectFactory(clear);
                }

                PgpObject message = plainFact.NextPgpObject();

                if (message is PgpLiteralData)
                {
                    PgpLiteralData literalData = (PgpLiteralData)message;
                    using (Stream unc = literalData.GetInputStream())
                    using (MemoryStream outputStream = new MemoryStream())
                    {
                        unc.CopyTo(outputStream);
                        output = outputStream.ToArray();
                    }
                } else
                {
                    throw new PgpException("Message is not a simple encrypted file - type unknown.");
                }

            }
            return output;
        }


        public static void GenerateKeyPair(string identity, string password, out byte[] publicKey, out byte[] privateKey)
        {
            IAsymmetricCipherKeyPairGenerator kpg = new RsaKeyPairGenerator();
            kpg.Init(new RsaKeyGenerationParameters(BigInteger.ValueOf(0x13), new SecureRandom(), KEY_STRENGTH, 8));
            AsymmetricCipherKeyPair kp = kpg.GenerateKeyPair();

            using (MemoryStream privateKeyStream = new MemoryStream())
            using (MemoryStream publicKeyStream = new MemoryStream())
            {
                ExportKeyPair(privateKeyStream, publicKeyStream, kp.Public, kp.Private, identity, password.ToCharArray(), false);

                publicKey = publicKeyStream.ToArray();
                privateKey = privateKeyStream.ToArray();
            }

        }

        private static void ExportKeyPair(
            Stream secretOut,
            Stream publicOut,
            AsymmetricKeyParameter publicKey,
            AsymmetricKeyParameter privateKey,
            string identity,
            char[] passPhrase,
            bool armor)
        {
            if (armor)
            {
                secretOut = new ArmoredOutputStream(secretOut);
            }

            PgpSecretKey secretKey = new PgpSecretKey(
                PgpSignature.DefaultCertification,
                PublicKeyAlgorithmTag.RsaGeneral,
                publicKey,
                privateKey,
                DateTime.Now,
                identity,
                SymmetricKeyAlgorithmTag.Aes256,
                passPhrase,
                null,
                null,
                new SecureRandom()
            );

            secretKey.Encode(secretOut);


            if (armor)
            {
                publicOut = new ArmoredOutputStream(publicOut);
            }

            PgpPublicKey key = secretKey.PublicKey;

            key.Encode(publicOut);

        }
    }
}
    