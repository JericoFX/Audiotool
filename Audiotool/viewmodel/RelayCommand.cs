using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Audiotool.viewmodel
{
    public class RelayCommand(Action<object> execute, Func<object, bool>? canExecute = null) : ICommand
    {
        private readonly Action<object> _execute = execute;
#pragma warning disable CS8601 // Possible null reference assignment.
        private readonly Func<object, bool> _canExecute = canExecute;
#pragma warning restore CS8601 // Possible null reference assignment.

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return _canExecute == null || _canExecute(parameter);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        public void Execute(object? parameter)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            _execute(parameter);
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}
