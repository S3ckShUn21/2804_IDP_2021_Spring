using LiveCharts;
using LiveCharts.Configurations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WirelessSensorNodeDashboard.Util;
using WirelessSensorNodeDashboard.ViewModels;

namespace WirelessSensorNodeDashboard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Setup Live Charts to be able to use my data types
            var mapper = Mappers.Xy<TemperatureRecord>()
                .X(model => model.RecordDateTime.Ticks)
                .Y(model => model.RecordTemperature);

            // Save the mapper globally.
            Charting.For<TemperatureRecord>(mapper);

            // TODO: Date Time formatter

            // App wide serial port
            SerialPort serialPort = new SerialPort();

            // Now set up the application
            MainWindow mainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel(serialPort)
            };

            mainWindow.Show();
        }
    }
}
