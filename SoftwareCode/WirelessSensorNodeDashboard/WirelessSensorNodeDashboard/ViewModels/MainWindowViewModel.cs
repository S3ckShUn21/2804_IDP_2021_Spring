using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using WirelessSensorNodeDashboard.Commands;
using WirelessSensorNodeDashboard.Views;

namespace WirelessSensorNodeDashboard.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        private TerminalView _terminalViewModel;
        private MainUIView _mainUIViewModel;

        public ContentControl CurrentContentControl { get; set; }

        private ICommand _menuBarClickedCommand;
        public ICommand MenuBarClickedCommand
        {
            get => _menuBarClickedCommand;
            set => SetPropertyAndNotify(ref _menuBarClickedCommand, value, nameof(MenuBarClickedCommand));
        }

        public MainWindowViewModel()
        {
            _terminalViewModel = new TerminalView();
            _mainUIViewModel = new MainUIView();

            CurrentContentControl = _terminalViewModel;

            _menuBarClickedCommand = new RelayCommand<string>(ChangePage);
        }

        private void ChangePage(String page)
        {
            // TODO: Change this from being a user entered string to a static resource
            switch (page)
            {
                case "MainUI":
                    CurrentContentControl = _mainUIViewModel;
                    break;
                case "Terminal":
                    CurrentContentControl = _terminalViewModel;
                    break;
            }
            OnPropertyChanged(nameof(CurrentContentControl));
        }
    }
}
