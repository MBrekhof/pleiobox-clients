using System;
using System.Runtime.Serialization;

namespace LocalBox_Common
{
    [DataContract]
    public class AesKeyResponse
    {
        public AesKeyResponse()
        {
        }

        [DataMember(Name = "iv")]
        public String IV { get; set; }

        [DataMember(Name = "key")]
        public String Key { get; set; }
    }
}

