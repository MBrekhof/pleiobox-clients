using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using SQLite;
using Xamarin;
using System.Linq;


namespace LocalBox_Common
{
	[DataContract]
    public class LocalBox
    {
        public LocalBox()
        {
        }

        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

		[DataMember]
        public string Name { get; set; }

		[DataMember]
		public string BaseUrl { get; set; }

		[DataMember]
        public string User { get; set; }

		[DataMember]
        public string BackColor { get; set; }

		[DataMember]
		public string FontColor { get; set; }

		[DataMember]
        public string LogoUrl { get; set; }

		[DataMember]
		public string pin_cert { get; set; }

		[DataMember]
		public byte[] OriginalServerCertificate {
			get
			{
				try{
					if(!string.IsNullOrEmpty (pin_cert)){

						//Remove invalid characters and words from PEM string
						pin_cert = pin_cert.Replace ("BEGIN CERTIFICATE", "");
						pin_cert = pin_cert.Replace ("END CERTIFICATE", "");
						pin_cert = pin_cert.Replace("-", "");
						pin_cert = pin_cert.Replace("\n", "");

						var certByte = Convert.FromBase64String(pin_cert);
						return certByte;
					}else {
						return null;
					}
				} catch (Exception ex){
					Insights.Report(ex);
					return null;
				}
			}
			set {
				try{
					pin_cert = System.Convert.ToBase64String(OriginalServerCertificate);
				} catch (Exception ex){
					Insights.Report(ex);
				}
			}
		}

		public static string StringWordsRemove(string stringToClean)
		{
			return string.Join(" ", stringToClean.Split(' ').Except(wordsToRemove));
		}

		private static List<string> wordsToRemove = "- BEGIN CERTIFICATE END CERTIFICATE \n".Split(' ').ToList();




		public string ApiKey { get; set; }
		public string ApiSecret { get; set; }

        public byte[] PublicKey { get; set; }
        public byte[] PrivateKey { get; set; }
        public string PassPhrase { get; set; }

        public string DatumTijdTokenExpiratie { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        [Ignore]
        public bool HasCryptoKeys { 
            get { 
                return PublicKey != null && PublicKey.Length > 0 && PrivateKey != null && PrivateKey.Length > 0;
            } 
        }

        [Ignore]
        public bool HasPassPhrase {
            get {
                return !string.IsNullOrWhiteSpace(PassPhrase);
            }
        }

    }
}

