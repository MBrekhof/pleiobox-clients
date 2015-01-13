using System;
using System.Runtime.Serialization;

namespace LocalBox_Common
{ 
    [DataContract]
    public class UserPost
    {
        [DataMember(Name = "public_key")]
        public String PublicKey { get; set; }

        [DataMember(Name = "private_key")]
        public String PrivateKey { get; set; }
    }
}

