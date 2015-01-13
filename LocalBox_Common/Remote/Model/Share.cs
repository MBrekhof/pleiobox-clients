using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace LocalBox_Common
{
    [DataContract]
	public class Share
	{
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "item")]
        public TreeNode Item { get; set; }

        [DataMember(Name = "identities")]
        public List<Identity> Identities { get; set; }

		public Share ()
		{

		}
	}
}

