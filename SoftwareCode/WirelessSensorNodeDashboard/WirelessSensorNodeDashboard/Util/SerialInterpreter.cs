﻿using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace WirelessSensorNodeDashboard.Util
{
    public class SerialInterpreter
    {
        private SerialPort _serialPort;

        public SerialInterpreter()
        {
            // Set up the asics with the serial port
            // The rest will be done externally by the ViewModels
            _serialPort = new SerialPort();
            _serialPort.BaudRate = 9600;
            _serialPort.DtrEnable = true;
            _serialPort.DataBits = 8;
            _serialPort.Handshake = Handshake.None;

            // Setup the serial port callback that will invoke out event handleer
            _serialPort.DataReceived += (sender, args) =>
            {
                // Parse Data
                byte[] data = new byte[_serialPort.BytesToRead];
                _serialPort.Read(data, 0, data.Length);
                string line = Encoding.Default.GetString(data);

                // Invoke our event with that data
                onDataRecieved(line);
            };
        }

        #region Serial Port Wrapper Functions
        public void Open()
        {
            _serialPort.Open();
            _serialPort.DiscardInBuffer();
        }

        public bool IsOpen()
        {
            return _serialPort.IsOpen;
        }

        public void Close()
        {
            _serialPort.Close();
        }

        public void Dispose()
        {
            _serialPort.Dispose();
        }

        public void setComPort(string comPort)
        {
            _serialPort.PortName = comPort;
        }

        public void Write(string text)
        {
            _serialPort.Write(text);
        }
        #endregion

        #region Event Handler
        // The event that the ViewModels will subscribe to
        public event EventHandler<string> DataReceived;

        // This function will run all the subscribed callbacks passing in the string that was receved
        // from the serialPort
        protected virtual void onDataRecieved(string str)
        {
            DataReceived?.Invoke(this, str);
        }
        #endregion
    }

    
}
