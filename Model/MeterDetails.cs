using SQLite;

namespace Project_K.Model
{

    public class MeterDetails
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string MeterName { get; set; }
        public string MeterDescription { get; set; }
        public string MeterType { get; set; }
        public string Make { get; set; }
        public string IPAddress { get; set; }
        public double PollingInterval { get; set; }
        public string ModbusAddress { get; set; }    // Stored as a comma-separated string
        public string TimeStamp { get; set; }

    }
}
