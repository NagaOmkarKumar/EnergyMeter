using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_K.Model
{
    public class MinMaxValues
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string MeterName { get; set; }
        public string Date { get; set; }
        public string Timestamp { get; set; }
        public double MaxVoltage { get; set; }
        public double MaxCurrent { get; set; }
        public double MaxEnergy { get; set; }
        public double MaxPF { get; set; }
        public double MinVoltage { get; set; }
        public double MinCurrent { get; set; }
        public double MinEnergy { get; set; }
        public double MinPF { get; set; }
        public string MaxVoltageTime { get; set; }
        public string MinVoltageTime { get; set; }
        public string MaxCurrentTime { get; set; }
        public string MinCurrentTime { get; set; }
        public string MaxEnergyTime { get; set; }
        public string MinEnergyTime { get; set; }
        public string MaxPFTime { get; set; }
        public string MinPFTime { get; set; }

    }
}
