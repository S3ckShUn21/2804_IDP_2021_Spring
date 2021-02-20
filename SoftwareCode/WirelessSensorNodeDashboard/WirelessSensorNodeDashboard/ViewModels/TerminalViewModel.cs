using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using WirelessSensorNodeDashboard.Commands;

namespace WirelessSensorNodeDashboard.ViewModels
{
    public sealed class TerminalViewModel : BaseViewModel
    {
        private StringBuilder _terminalText;
        private string _inputText;
        private ICommand _lineEnteredCommand;

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
            // Set the initial capacity to be 2048 characters
            _terminalText = new StringBuilder("This is the first test of the terminal\n", 2048);
            _inputText = "";

            _lineEnteredCommand = new RelayCommand<string>(LineEntered);
        }

        private void LineEntered(string line)
        {
            InputText = "";
            _terminalText.AppendFormat("{0}\n", line);
            OnPropertyChanged(nameof(TerminalText));
            
        }


    }
}
