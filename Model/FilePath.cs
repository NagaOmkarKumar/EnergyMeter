using SQLite;

namespace Project_K.Model
{
    class FilePath
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string MeterName { get; set; }

        public string Fileurl { get; set; }

        public string TimeStamp { get; set; }
        public string EmailAddress { get; set; }

    }
}
