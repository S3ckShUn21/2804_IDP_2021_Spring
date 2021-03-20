using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Windows.Input;
using WirelessSensorNodeDashboard.Commands;

namespace WirelessSensorNodeDashboard.ViewModels
{
    public sealed class TerminalViewModel : BaseViewModel
    {
        #region Private Fields
        // Text Fields
        private StringBuilder _terminalText;

        // Serial Port info
        private SerialPort _serialPort;

        // Commands
        private ICommand _lineEnteredCommand;
        private ICommand _openSerialPortCommand;
        private ICommand _closeSerialPortCommand;
        private ICommand _reloadComPortListCommand;
        private ICommand _clearTeminalCommand;

        #endregion

        #region Public Properties
        public string TerminalText
        {
            get => _terminalText.ToString(); 
            set 
            { 
                _terminalText.Clear().Append(value); 
                OnPropertyChanged(nameof(TerminalText)); 
            }
        }

        public string InputText
        {
            get; set;
        }

        public List<string> ComPorts { get; private set; }
        public string SelectedComPort { get; set; }
        #endregion

        #region Public Commands
        public ICommand LineEnteredCommand
        {
            get => _lineEnteredCommand;
            set => SetPropertyAndNotify(ref _lineEnteredCommand, value, nameof(LineEnteredCommand));
        }

        public ICommand OpenSerialPortCommand
        {
            get => _openSerialPortCommand;
            set => SetPropertyAndNotify(ref _openSerialPortCommand, value, nameof(OpenSerialPortCommand));
        }

        public ICommand CloseSerialPortCommand
        {
            get => _closeSerialPortCommand;
            set => SetPropertyAndNotify(ref _closeSerialPortCommand, value, nameof(CloseSerialPortCommand));
        }

        public ICommand ReloadComPortListCommand
        {
            get => _reloadComPortListCommand;
            set => SetPropertyAndNotify(ref _reloadComPortListCommand, value, nameof(ReloadComPortListCommand));
        }

        public ICommand ClearTerminalCommand
        {
            get => _clearTeminalCommand;
            set => SetPropertyAndNotify(ref _clearTeminalCommand, value, nameof(ClearTerminalCommand));
        }
        #endregion

        #region CTOR
        public TerminalViewModel()
        {
            // Set the initial capacity to be 4096 characters
            _terminalText = new StringBuilder("", 4096);
            InputText = String.Empty;

            // Load COM stuff
            _serialPort = new SerialPort();
            LoadComPortList();
            if( ComPorts.Count == 0 )
            {
                SelectedComPort = null;
            } else
            {
                SelectedComPort = ComPorts[0];
            }
            OnPropertyChanged(nameof(SelectedComPort));

            _lineEnteredCommand = new RelayCommand<string>(LineEntered, param => _serialPort.IsOpen);
            _openSerialPortCommand = new RelayCommand(OpenSerialPort, () => SelectedComPort != null);
            _closeSerialPortCommand = new RelayCommand(CloseSerialPort, () => _serialPort.IsOpen);
            _reloadComPortListCommand = new RelayCommand(LoadComPortList);
            _clearTeminalCommand = new RelayCommand(ClearTerminal);
            
        }
        #endregion

        #region Events
        private void DataRecievedEvent(object sender, SerialDataReceivedEventArgs e)
        {
            if (_serialPort.IsOpen)
            {
                byte[] data = new byte[_serialPort.BytesToRead];
                _serialPort.Read(data, 0, data.Length);
                string line = Encoding.Default.GetString(data);
                AppendToTerminal(line);
            }
        }
        #endregion

        #region Private Methods

        private void OpenSerialPort()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Dispose();
                _serialPort.Close();
            }

            _serialPort.PortName = SelectedComPort;
            _serialPort.BaudRate = 9600;
            _serialPort.DtrEnable = true;
            _serialPort.DataBits = 8;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Open();
            _serialPort.DiscardInBuffer();

            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataRecievedEvent);
        }

        private void CloseSerialPort()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Dispose();
                _serialPort.Close();
            }
        }

        private void LineEntered(string line)
        {
            InputText = String.Empty;
            OnPropertyChanged(nameof(InputText));
            _serialPort.Write(line + '\n');
        }

        private void AppendToTerminal(string line)
        {
            _terminalText.Append(line);
            OnPropertyChanged(nameof(TerminalText));
        }

        private void LoadComPortList()
        {
            ComPorts = new List<string>(SerialPort.GetPortNames());
            OnPropertyChanged(nameof(ComPorts));
        }

        private void ClearTerminal()
        {
            TerminalText = "";
        }
        #endregion

    }
}
