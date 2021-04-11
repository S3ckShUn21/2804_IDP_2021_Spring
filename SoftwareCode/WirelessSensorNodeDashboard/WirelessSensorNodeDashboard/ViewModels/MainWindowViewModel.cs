using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using WirelessSensorNodeDashboard.Commands;
using WirelessSensorNodeDashboard.Views;

namespace WirelessSensorNodeDashboard.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        private TerminalViewModel _terminalViewModel;
        private MainUIViewModel _mainUIViewModel;

        public BaseViewModel CurrentViewModel { get; set; }

        private ICommand _menuBarClickedCommand;
        public ICommand MenuBarClickedCommand
        {
            get => _menuBarClickedCommand;
            set => SetPropertyAndNotify(ref _menuBarClickedCommand, value, nameof(MenuBarClickedCommand));
        }

        public MainWindowViewModel(SerialPort serialPort)
        {
            _terminalViewModel = new TerminalViewModel(serialPort);
            _mainUIViewModel = new MainUIViewModel(serialPort);

            CurrentViewModel = _mainUIViewModel;

            _menuBarClickedCommand = new RelayCommand<string>(ChangePage);
        }

        private void ChangePage(String page)
        {
            // TODO: Change this from being a user entered string to a static resource
            switch (page)
            {
                case "MainUI":
                    CurrentViewModel = _mainUIViewModel;
                    break;
                case "Terminal":
                    CurrentViewModel = _terminalViewModel;
                    break;
            }
            Debug.Print("Page Button Pressed: " + page);
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
