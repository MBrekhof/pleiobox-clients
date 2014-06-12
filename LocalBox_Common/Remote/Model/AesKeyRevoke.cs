using System;
using System.Runtime.Serialization;

namespace LocalBox_Common
{
    [DataContract]
    public class AesKeyRevoke
    {
        [DataMember(Name = "username")]
        public string Username { get; set; }

        public AesKeyRevoke()
        {
        }
    }
}

