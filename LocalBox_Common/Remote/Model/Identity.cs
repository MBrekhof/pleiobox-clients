using System.Runtime.Serialization;

namespace LocalBox_Common
{
    [DataContract]
	public class Identity
	{
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }
		
		[DataMember(Name = "has_keys")]
		public bool HasKeys { get; set; }
	}
}

