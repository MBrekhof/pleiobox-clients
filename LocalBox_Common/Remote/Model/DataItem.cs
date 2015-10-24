using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Synchronization.Models
{
    [DataContract]
    public class DataGroup
    {
        public DataGroup()
        {

        }

        [DataMember(Name = "hash")]
        public string UniqueId { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "modified_at")]
        public DateTime Modified { get; set; }

        [DataMember(Name = "is_dir")]
        public bool IsFolder { get; set; }

        [DataMember(Name = "is_shared")]
        public bool Shared { get; set; }

        [DataMember(Name = "is_share")]
        public bool Share { get; set; }

		[DataMember(Name = "is_writable")]
		public bool IsWritable { get; set; }

        [DataMember(Name = "children")]
        public List<DataItem> Items { get; set; }

		[DataMember(Name = "has_keys")]
		public bool HasKeys { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    [DataContract]
    public class DataItem
    {
        public DataItem()
        {

        }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }

        [DataMember(Name = "mime_type")]
        public string FileType { get; set; }

        [DataMember(Name = "modified_at")]
        public DateTime ModifiedDate { get; set; }

        [DataMember(Name = "is_dir")]
        public bool IsFolder { get; set; }

        [DataMember(Name = "is_shared")]
        public bool Shared { get; set; }

        [DataMember(Name = "is_share")]
        public bool Share { get; set; }

		[DataMember(Name = "is_writable")]
		public bool IsWritable { get; set; }

        [DataMember(Name = "icon")]
        private string _thumbnail;
        public string Thumbnail
        {
            get
            {

                return "Images/" + _thumbnail + ".png";

            }
            set
            {
                _thumbnail = value;
            }
        }

        [DataMember(Name = "size")]
        public long Size { get; set; }

        [DataMember(Name = "revision")]
        public int Revision { get; set; }

		[DataMember(Name = "has_keys")]
		public bool HasKeys { get; set; }

        public override string ToString()
        {
            return this.Title;
        }
    }
}
