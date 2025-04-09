using SQLite;

namespace Project_K.Model
{
    public class MeterData
    {
        //  public MeterData() { }
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string MeterName { get; set; }
        public double Voltage { get; set; }
        public double Current { get; set; }
        public double PF { get; set; }
        public double KWH { get; set; }
        public double KVA { get; set; }
        public string Date { get; set; }
        public string Timestamp { get; set; }
    }
}