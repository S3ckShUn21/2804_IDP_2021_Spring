using LiveCharts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
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

        private int _numRecordsToShow;
        private float _currentTemperature;
        private bool _displayCelcius;

        private string _zipCode;
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

        public string  ZipCode
        {
            get { return _zipCode; }
            set 
            { 
                _zipCode = value;
                OnPropertyChanged(nameof(ZipCode));
            }
        }

        public ChartValues<TemperatureRecord> TemperatureRecords { get; set; }

        #endregion

        #region Public Commands
        private ICommand _loadDataCommand;
        public ICommand LoadDataCommand
        {
            get => _loadDataCommand;
            set => SetPropertyAndNotify(ref _loadDataCommand, value, nameof(LoadDataCommand));
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

            _zipCode = "";

            TemperatureRecords = new ChartValues<TemperatureRecord>();
            NumRecordsToShow = 30;
        }
        #endregion

        #region Events
        private void DataRecievedEvent(object sender, string str)
        {
            // Check to see if this is a data packet
            if (str.Substring(0, 6).Equals("[DATA]"))
            {
                // The property automatically converts the string to the float for us, and updates the UI
                CurrentTemperature = float.Parse(str.Substring(6));

                UpdateTemperatureHistory(_currentTemperature);
            }
        }
        #endregion

        #region Private Methods
        private void testWriteData()
        {
            Debug.WriteLine("Test Function Start");

            // Writing Data to a file
            string writtenPath = @TemperatureRecord.RecordFileName;
            if (!File.Exists(writtenPath))
            {
                using (StreamWriter sw = File.CreateText(writtenPath))
                {
                    for (int i = 0; i < 1000; i++)
                    {
                        DateTime time = DateTime.Now;
                        sw.WriteLine("{0},{1:D4}", time.ToString(TemperatureRecord.DateTimeFormat), i);
                        Thread.Sleep(10);
                    }
                }
            }

            Debug.WriteLine("Test Function End");
        }

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
            long filePointer = fileSize;
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
            int numToRead = (int)(fileSize - filePointer - 2); // There are 2 extra EOF bytes at the end of the array
            byte[] bytes = new byte[numToRead];
            file.Read(bytes, 0, numToRead);
            string[] records = Encoding.Default.GetString(bytes).Split("\r\n");
            // convert list of strings to list of temp records
            TemperatureRecords.Clear();
            foreach (string s in records)
            {
                if (!string.IsNullOrEmpty(s))
                    TemperatureRecords.Add(TemperatureRecord.ParseRecord(s));
            }
            OnPropertyChanged(nameof(TemperatureRecords));
        }

        private void UpdateTemperatureHistory(float newData)
        {
            TemperatureRecord record = new TemperatureRecord(DateTime.UtcNow, newData);
            using (StreamWriter sw = File.AppendText(TemperatureRecord.RecordFileName))
            {
                sw.WriteLine(record);
            }
            TemperatureRecords.Add(record);
            OnPropertyChanged(nameof(TemperatureRecords));
        }
        #endregion

    }
}
