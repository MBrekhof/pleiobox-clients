using System;
using System.Runtime.Serialization;

namespace LocalBox_Common.Remote.Model
{
    [DataContract]
    public class LinkItem
    {
        public LinkItem()
        {
        }

        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}

