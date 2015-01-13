using System;
using System.Runtime.Serialization;

namespace LocalBox_Common
{
    [DataContract]
	public class ShareInventation
	{
        [DataMember(Name = "id")]
        public int Id { set; get; }

        [DataMember(Name = "created_at")]
        public DateTime CreatedAt { set; get; }

        [DataMember(Name = "state")]
        public string State { set; get; }

        [DataMember(Name = "share")]
        public Share Share { set; get; }

        public ShareInventation(){}
	}
}

