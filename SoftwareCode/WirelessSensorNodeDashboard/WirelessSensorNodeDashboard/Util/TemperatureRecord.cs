using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace WirelessSensorNodeDashboard.Util
{
    public sealed class TemperatureRecord
    {
        public const string DateTimeFormat = "yyyy-MM-ddTHH:mm:ss:ff";
        public const string RecordFileName = @"Records.csv";
        public DateTime RecordDateTime { get; set; }
        public float RecordTemperature { get; set; }

        public TemperatureRecord(DateTime dateTime, float temperature)
        {
            RecordDateTime = dateTime;
            RecordTemperature = temperature;
        }

        public override string ToString()
        {
            return $"{RecordDateTime.ToString(DateTimeFormat)}, {RecordTemperature:F2}";
        }

        // This method is used if you are pulling a line from the CSV
        public static TemperatureRecord ParseRecord(string str)
        {
            string[] fields = str.Split(',');
            DateTime tempDT = DateTime.ParseExact(fields[0], DateTimeFormat, null);
            float tempT = float.Parse(fields[1]);
            return new TemperatureRecord(tempDT, tempT);
        }
    }
}
