using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using WirelessSensorNodeDashboard.Commands;

namespace WirelessSensorNodeDashboard.ViewModels
{
    public sealed class TerminalViewModel : BaseViewModel
    {
        private string _terminalText;
        private string _inputText;
        private ICommand _lineEnteredCommand;

        public string TerminalText
        {
            get => _terminalText; 
            set { _terminalText = value; OnPropertyChanged(nameof(TerminalText)); }
        }

        public string InputText
        {
            get => _inputText;
            set { _inputText = value; OnPropertyChanged(nameof(InputText)); }
        }

        public ICommand LineEnteredCommand 
        {
            get => _lineEnteredCommand;
            set => SetPropertyAndNotify(ref _lineEnteredCommand, value, nameof(LineEnteredCommand));
        }

        public TerminalViewModel()
        {
            _terminalText = "This is the first test of the terminal\n";
            _inputText = "";

            _lineEnteredCommand = new RelayCommand<string>(LineEntered);
        }

        private void LineEntered(string line)
        {
            InputText = "";
            TerminalText += line += '\n';
            
        }


    }
}
