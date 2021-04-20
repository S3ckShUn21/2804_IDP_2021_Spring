using System;
using System.Collections.Generic;
using System.Text;

namespace WirelessSensorNodeDashboard.Util
{
    public sealed class WeatherAPIResponse
    {
        public MainWeatherData Main { get; set; }
        public WeatherDescriptionData[] Weather { get; set; }

        public class MainWeatherData
        {
            public float temp { get; set; }
        }

        public class WeatherDescriptionData
        {
            public int id { get; set; }
            public string main { get; set; }
            public string description { get; set; }
        }
    }

    
}
