using System;
using SQLite;
using System.Runtime.Serialization;

namespace LocalBox_Common
{
	[DataContract]
	public class Tokens
	{
		public Tokens ()
		{
		}

		[PrimaryKey]
		[AutoIncrement]
		public int Id { get; set; }

		[DataMember]
		public string AccessToken { get; set; }

		[DataMember]
		public string RefreshToken { get; set; }

		[DataMember]
		public string Expires { get; set; }
	}
}

