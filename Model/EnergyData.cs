using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_K.Model
{
    public class EnergyData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string MeterName { get; set; }
        public double? KWH { get; set; }
        public string Date { get; set; }
       
    }
}
