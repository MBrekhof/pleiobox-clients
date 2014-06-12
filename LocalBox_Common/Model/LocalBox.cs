using System;
using SQLite;
using System.Collections.Generic;
using System.Runtime.Serialization;

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
        //[Unique]

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

