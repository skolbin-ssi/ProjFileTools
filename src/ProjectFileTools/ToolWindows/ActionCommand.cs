using System;
using System.Windows.Input;

namespace ProjectFileTools.ToolWindows
{
    internal class ActionCommand : ICommand
    {
        private readonly Action<object> _execute;
        private Func<object, bool> _canExecute;
        private bool _currentCanExecute;

        private ActionCommand(Action<object> execute, Func<object, bool> canExecute, bool initialCanExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
            _currentCanExecute = initialCanExecute;
        }

        public event EventHandler CanExecuteChanged;

        public static ICommand From(Action execute, Func<bool> canExecute = null, bool initialCanExecute = true)
        {
            return new ActionCommand(o => execute(), o => canExecute?.Invoke() ?? initialCanExecute, initialCanExecute);
        }

        public static ICommand From<T>(Action<T> execute, Func<T, bool> canExecute = null, bool initialCanExecute = true)
        {
            return new ActionCommand(o => execute((T)o), o => canExecute?.Invoke((T)o) ?? initialCanExecute, initialCanExecute);
        }

        public bool CanExecute(object parameter)
        {
            bool oldCanExecute = _currentCanExecute;
            _currentCanExecute = _canExecute(parameter);

            if (oldCanExecute ^ _currentCanExecute)
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            return _currentCanExecute;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
