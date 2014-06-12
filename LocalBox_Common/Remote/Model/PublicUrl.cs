using System;
using System.Runtime.Serialization;

namespace LocalBox_Common
{
	[DataContract]
	public class PublicUrl
	{
		[DataMember(Name = "id")]
		public int id { get; set; }

		[DataMember(Name = "public_id")]
		public string publicId { get; set; }

		[DataMember(Name = "created_at")]
		public DateTime createdAt{ get; set; }

		[DataMember(Name = "uri")]
		public string publicUri{ get; set; }


		public PublicUrl()
		{
		}

	}
}

