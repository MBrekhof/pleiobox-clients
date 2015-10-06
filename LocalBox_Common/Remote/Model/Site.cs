using System;
using System.Runtime.Serialization;

namespace LocalBox_Common
{
	[DataContract]
	public class Site
	{
		[DataMember(Name = "Name")]
		public string Name { set; get; }

		[DataMember(Name = "Url")]
		public string Url { set; get; }
	}
}

