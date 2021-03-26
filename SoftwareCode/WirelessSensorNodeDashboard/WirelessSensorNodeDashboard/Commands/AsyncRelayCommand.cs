using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WirelessSensorNodeDashboard.Commands
{
    public class AsyncRelayCommand<T> : RelayCommand<T>
    {
        private bool _isExecuting;
        private readonly Action<Exception> _errorHandler;

        public AsyncRelayCommand(
            Action<T> execute,
            Func<T, bool> canExecute = null,
            Action<Exception> errorHandler = null)
            : base(execute, canExecute)
        {
            _errorHandler = errorHandler;
            _isExecuting = false;
        }

        /*public async Task ExecuteAsync()
        {
            if (CanExecute())
            {
                try
                {
                    _isExecuting = true;
                    await _execute();
                }
                finally
                {
                    _isExecuting = false;
                }
            }

            RaiseCanExecuteChanged();
        }*/

        /*public bool CanExecute()
        {
            return !_isExecuting && (_canExecute?.Invoke() ?? true);
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        void ICommand.Execute(object parameter)
        {
            ExecuteAsync().FireAndForgetSafeAsync(_errorHandler);
        }*/
    }
}
