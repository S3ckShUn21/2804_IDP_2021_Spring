using LiveCharts;
using LiveCharts.Configurations;
using System;
using System.IO;
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
                .X(model => (model.RecordDateTime.Ticks))
                .Y(model => model.RecordTemperature);

            // Save the mapper globally.
            Charting.For<TemperatureRecord>(mapper);

            // Check to see if we can access the Records CSV file
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
            }
            catch (UnauthorizedAccessException)
            {
                proceedWithApplication = false;
                MessageBox.Show("Can't access the Temperature Records file.\nThis is most likely because of your anti-virus.\nPlease diable it while you use this program.",
                                "Wireless Sensor Node Dashboard Error",
                                MessageBoxButton.OK);
                // Close the application
                Current.Shutdown();
            }
            catch (IOException)
            {
                MessageBox.Show("Could not access the Records File\nPlease close the CSV if you have it open.",
                                "Wireless Sensor Node Dashboard Error",
                                MessageBoxButton.OK);
            }

            // If startup is okay then we can continue
            if (proceedWithApplication)
            {
                // App wide serial port
                SerialInterpreter serialInterpreter = new SerialInterpreter();

                // App wide HttpClient
                ApiHelper.InitializeClient();

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
