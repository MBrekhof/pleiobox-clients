using System;
using System.Runtime.Serialization;

namespace LocalBox_Common
{
    [DataContract]
    public class UserResponse
    {
        public UserResponse()
        {
        }

        [DataMember(Name = "name")]
        public String Name { get; set; }

        [DataMember(Name = "public_key")]
        public String PublicKey { get; set; }

        [DataMember(Name = "private_key")]
        public String PrivateKey { get; set; }
    }
}

