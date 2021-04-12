using LiveCharts;
using LiveCharts.Configurations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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

            bool proceedWithApplication = true;
            try
            {
                // Make sure the Temperature Records file exists
                if (!File.Exists(TemperatureRecord.RecordFileName))
                {
                    File.CreateText(TemperatureRecord.RecordFileName).Close();
                }

                // Now make sure the file can be opened otherwise an exception will be called
                File.AppendText(TemperatureRecord.RecordFileName).Close();

            } catch (UnauthorizedAccessException)
            {
                proceedWithApplication = false;
                Popup errorPopup = new Popup();
                TextBlock errorText = new TextBlock();
                errorText.Text = "Can't access the Records file.\nThis is most likely because of your anti-virus.\nPlease diable it while you use this program.";
                errorPopup.Child = errorText;
                errorPopup.IsOpen = true;
            }
            
            if (proceedWithApplication)
            {
                // App wide serial port
                SerialInterpreter serialInterpreter = new SerialInterpreter();

                // Now set up the application
                MainWindow mainWindow = new MainWindow()
                {
                    DataContext = new MainWindowViewModel(serialInterpreter)
                };

                mainWindow.Show();
            }
            
        }
    }
}
