using System;
using System.Runtime.Serialization;

namespace LocalBox_Common
{
	[DataContract]
	public class APIKeys
	{
		public APIKeys ()
		{
		}

		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public string Key { get; set; }

		[DataMember]
		public string Secret { get; set; }
	}
}

