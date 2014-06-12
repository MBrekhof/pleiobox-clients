using SQLite;

namespace LocalBox_Common
{
    public class Waarde
    {
        public Waarde()
        {
        }

        [PrimaryKey]
        public string Key { get; set; }

        public string Value { get; set; }

    }
}

