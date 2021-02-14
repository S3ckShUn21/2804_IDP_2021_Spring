using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using WirelessSensorNodeDashboard.Commands;

namespace WirelessSensorNodeDashboard.ViewModels
{
    public sealed class TerminalViewModel : BaseViewModel
    {
        private ObservableCollection<string> _lineItems;
        private ICommand _lineEnteredCommand;

        public ObservableCollection<string> LineItems
        {
            get { return _lineItems; }
            set 
            { 
                _lineItems = value;
                OnPropertyChanged(nameof(LineItems));
            }
        }

        public ICommand LineEnteredCommand 
        {
            get => _lineEnteredCommand;
            set => SetPropertyAndNotify(ref _lineEnteredCommand, value, "LineEnteredCommand");
        }

        public TerminalViewModel()
        {
            _lineItems = new ObservableCollection<string>();
            _lineEnteredCommand = new RelayCommand<string>(LineEntered);

            _lineItems.Add("This is the first test of the terminal...");
        }

        private void LineEntered(string line)
        {
            _lineItems.Add(line);
            Debug.Print("Added Line: " + line);
        }


    }
}
