using System;
using System.Windows.Input;

namespace WirelessSensorNodeDashboard.Commands
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Func<T, bool> _canExecute;
        private readonly Action<T> _execute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public RelayCommand(Action<T> execute)
        {
            _execute = execute;
            _canExecute = null;
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter)) _execute((T)parameter);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
