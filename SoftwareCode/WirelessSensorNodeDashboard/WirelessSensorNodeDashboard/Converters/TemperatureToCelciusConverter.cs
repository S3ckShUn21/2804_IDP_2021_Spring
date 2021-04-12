using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace WirelessSensorNodeDashboard.Converters
{
    class TemperatureToCelciusConverter : IValueConverter
    {
        // Input will be a float in terms of degrees celcius, output will be a string in degrees celcius
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"{value:F2} °C";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
