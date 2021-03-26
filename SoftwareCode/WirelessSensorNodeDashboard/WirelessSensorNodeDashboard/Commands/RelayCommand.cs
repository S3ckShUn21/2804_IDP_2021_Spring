using System;
using System.Windows.Input;

namespace WirelessSensorNodeDashboard.Commands
{
    public class RelayCommand<T> : ICommand
    {
        protected readonly Func<T, bool> _canExecute;
        protected readonly Action<T> _execute;

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
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

    public sealed class RelayCommand : RelayCommand<object>
    {
        public RelayCommand(Action execute)
            : base(x => execute(), x => true)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute) : base(x => execute(), x => canExecute())
        {
        }
    }
}
