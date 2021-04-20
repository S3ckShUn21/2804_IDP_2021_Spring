using LiveCharts;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using WirelessSensorNodeDashboard.Commands;
using WirelessSensorNodeDashboard.Util;

namespace WirelessSensorNodeDashboard.ViewModels
{
    public sealed class MainUIViewModel : BaseViewModel
    {
        #region Private Fields
        private SerialInterpreter _serialInterpreter;

        private bool _canLoadData;

        private int _maxTickMarks;
        private int _numRecordsToShow;      // Number of records we want to show on the screen
        private int _numRecords;            // The number of recrods currently being shown on the screen
        private float _currentTemperature;
        private bool _displayCelcius;

        private int _zipCode;
        private string _weatherIconSource;
        private string _weatherDescription;
        private float _weatherTemperature;
        #endregion

        #region Public Properties
        public int NumRecordsToShow
        {
            get => _numRecordsToShow;
            set
            {
                _numRecordsToShow = value;
                OnPropertyChanged(nameof(NumRecordsToShow));
            }
        }

        public float CurrentTemperature
        {
            get { return _currentTemperature; }
            set
            {
                _currentTemperature = value;
                OnPropertyChanged(nameof(CurrentTemperature));
            }
        }

        public bool DisplayCelcius
        {
            get { return _displayCelcius; }
            set
            {
                _displayCelcius = value;
                OnPropertyChanged(nameof(DisplayCelcius));
                OnPropertyChanged(nameof(DisplayFahrenheit));
            }
        }

        public bool DisplayFahrenheit
        {
            get { return !_displayCelcius; }
            set
            {
                _displayCelcius = !value;
                OnPropertyChanged(nameof(DisplayCelcius));
                OnPropertyChanged(nameof(DisplayFahrenheit));
            }
        }

        public int ZipCode
        {
            get { return _zipCode; }
            set
            {
                _zipCode = value;
                OnPropertyChanged(nameof(ZipCode));
            }
        }

        public string WeatherIconSource
        {
            get { return _weatherIconSource; }
            set 
            { 
                _weatherIconSource = value;
                OnPropertyChanged(nameof(WeatherIconSource));
            }
        }

        public string WeatherDescription
        {
            get { return _weatherDescription; }
            set 
            { 
                _weatherDescription = value;
                OnPropertyChanged(nameof(WeatherDescription));
            }
        }

        public float WeatherTemperature
        {
            get { return _weatherTemperature; }
            set
            {
                _weatherTemperature = value;
                OnPropertyChanged(nameof(WeatherTemperature));
            }
        }

        // Properties for making the graph look pretty
        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }
        public double XAxisMin { get; set; }
        public double XAxisMax { get; set; }
        public double YAxisMax { get; set; }
        public double YAxisMin { get; set; }

        public ChartValues<TemperatureRecord> TemperatureRecords { get; set; }

        #endregion

        #region Public Commands
        private ICommand _loadDataCommand;
        private ICommand _getWeatherDataCommand;

        public ICommand LoadDataCommand
        {
            get => _loadDataCommand;
            set => SetPropertyAndNotify(ref _loadDataCommand, value, nameof(LoadDataCommand));
        }

        public ICommand GetWeatherDataCommand
        {
            get => _getWeatherDataCommand;
            set => SetPropertyAndNotify(ref _getWeatherDataCommand, value, nameof(GetWeatherDataCommand));
        }
        #endregion

        #region CTOR
        public MainUIViewModel(SerialInterpreter serialInterpreter)
        {
            _serialInterpreter = serialInterpreter;
            _serialInterpreter.DataReceived += DataRecievedEvent;

            _canLoadData = true;
            _loadDataCommand = new RelayCommand(LoadRecordData, () => _canLoadData);

            _currentTemperature = 0;
            _displayCelcius = true;

            _zipCode = -1;
            _weatherIconSource = WeatherAPIProcessor.ImageMapping["Clear"];
            _weatherDescription = "Sunny";
            _weatherTemperature = 10;
            _getWeatherDataCommand = new RelayCommand<string>(GetWeatherInfo);

            TemperatureRecords = new ChartValues<TemperatureRecord>();
            NumRecordsToShow = 30;

            _maxTickMarks = 10;
            DateTimeFormatter = (value) => new DateTime((long)value).ToString("HH:mm:ss");
            AxisStep = TimeSpan.FromMinutes(2).Ticks;
            AxisUnit = TimeSpan.TicksPerSecond;
            XAxisMin = TimeSpan.FromMinutes(0).Ticks;
            XAxisMax = TimeSpan.FromMinutes(30).Ticks;

            YAxisMax = 50;
            YAxisMin = -10;
        }
        #endregion

        #region Events
        private void DataRecievedEvent(object sender, string str)
        {
            // Check to see if this is a data packet
            if ( str.Length >= 6 && str.Substring(0, 6).Equals("[DATA]"))
            {
                // The property automatically converts the string to the float for us, and updates the UI
                CurrentTemperature = float.Parse(str.Substring(6));

                UpdateTemperatureHistory(_currentTemperature);
            }
        }
        #endregion

        #region Private Methods
        private void LoadRecordData()
        {
            _canLoadData = false;
            using (FileStream fs = File.OpenRead(TemperatureRecord.RecordFileName))
            {
                pullLastNRecords(fs, NumRecordsToShow);
            }
            _canLoadData = true;
        }

        // TODO: This code is nowhere near safe to execute on anthing other than the record data
        private void pullLastNRecords(FileStream file, int numRecords)
        {
            // setup vars
            int newLineCount = numRecords + 1; // Add 1 because of split adds an extra record
            long fileSize = file.Length;
            long filePointer = fileSize - 1;
            // find how many butes to read
            while (newLineCount > 0 && filePointer > 0)
            {
                file.Seek(filePointer, SeekOrigin.Begin);
                int character = file.ReadByte();
                if ((char)character == '\n')
                {
                    newLineCount--;
                }
                filePointer--;
            }
            // read those bytes and get a list of strings
            int numToRead = (int)(fileSize - filePointer);
            byte[] bytes = new byte[numToRead];
            file.Seek(filePointer, SeekOrigin.Begin);
            file.Read(bytes, 0, numToRead);
            string[] records = Encoding.Default.GetString(bytes).Split("\r\n");
            // convert list of strings to list of temp records
            TemperatureRecords.Clear();
            _numRecords = 0;
            foreach (string s in records)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    TemperatureRecords.Add(TemperatureRecord.ParseRecord(s));
                    _numRecords++;
                }
            }
        }

        private void UpdateTemperatureHistory(float newData)
        {
            TemperatureRecord record = new TemperatureRecord(DateTime.UtcNow, newData);
            try
            {
                using (StreamWriter sw = File.AppendText(TemperatureRecord.RecordFileName))
                {
                    sw.WriteLine(record);
                }
                
            }
            catch (IOException)
            {
                MessageBox.Show("Could not write temperature data to file. This record will be lost.\nPlease close the CSV if you have it open.",
                                "Wireless Sensor Node Dashboard Error",
                                MessageBoxButton.OK);
            }

            if (_numRecords >= NumRecordsToShow)
            {
                TemperatureRecords.RemoveAt(0);
                _numRecords--;
            }
            TemperatureRecords.Add(record);
            _numRecords++;

            updateAxes(record.RecordDateTime);
        }

        private void updateAxes(DateTime currentTime)
        {
            // Handle the X-Axis

            // If we don't have enough data points, the display a large x axis on the screen
            if( _numRecords < 6 )
            {
                XAxisMax = currentTime.Ticks + TimeSpan.FromMinutes(1).Ticks;
                XAxisMin = currentTime.Ticks - TimeSpan.FromMinutes(15).Ticks;
            } 
            // Otherwise just use the min and max values in the Record List
            else
            {
                DateTime maxDT = TemperatureRecords[_numRecords - 1].RecordDateTime;
                DateTime minDT = TemperatureRecords[0].RecordDateTime;
                // The most recent will have the MAX x axis val
                XAxisMax = maxDT.Ticks;
                // The oldest will have the min
                XAxisMin = minDT.Ticks;

                // Modify the tick marks
                int numTickMarks = Math.Min(_maxTickMarks, _numRecords);
                AxisStep = (maxDT - minDT).Ticks / numTickMarks;
                OnPropertyChanged(nameof(AxisStep));
            }
            OnPropertyChanged(nameof(XAxisMax));
            OnPropertyChanged(nameof(XAxisMin));
        }

        private async void GetWeatherInfo(string zipcodeStr)
        {
            int zipcode = -1;
            int.TryParse(zipcodeStr, out zipcode);

            if (zipcode > 0)
            {
                WeatherAPIResponse weatherData = await WeatherAPIProcessor.LoadWeatherInfo(zipcode);

                string weatherDesc = weatherData.Weather[0].main;
                WeatherIconSource = (WeatherAPIProcessor.ImageMapping.ContainsKey(weatherDesc)) ?
                    WeatherAPIProcessor.ImageMapping[weatherData.Weather[0].main] : 
                    WeatherAPIProcessor.ImageMapping["Clear"];

                WeatherDescription = weatherData.Weather[0].description;

                WeatherTemperature = weatherData.Main.temp;

                ZipCode = zipcode;
            }
        }
        #endregion

    }
}
