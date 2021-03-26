using System;
using System.Collections.Generic;
using System.Text;

namespace WirelessSensorNodeDashboard.Util
{
    public sealed class TemperatureRecord
    {
        public const string DateTimeFormat = "yyyy-MM-ddThh:mm:ss:ff";
        public const string RecordFileName = "Records.csv";
        public DateTime RecordDateTime { get; set; }
        public int RecordTemperature { get; set; }

        public TemperatureRecord(DateTime dateTime, int temperature)
        {
            RecordDateTime = dateTime;
            RecordTemperature = temperature;
        }

        public static TemperatureRecord CreateRecord(string str)
        {
            string[] fields = str.Split(',');
            DateTime tempDT = DateTime.ParseExact(fields[0], DateTimeFormat, null);
            int tempT = int.Parse(fields[1]);
            return new TemperatureRecord(tempDT, tempT);
        }
    }
}
