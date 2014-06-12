using System;
using System.Runtime.Serialization;

namespace LocalBox_Common
{
    [DataContract]
    public class AesKeyPost
    {
        public AesKeyPost()
        {
        }

        [DataMember(Name = "username")]
        public String User { get; set; }

        [DataMember(Name = "iv")]
        public String IV { get; set; }

        [DataMember(Name = "key")]
        public String Key { get; set; }
    }
}

