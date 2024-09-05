using System.Windows.Input;

namespace ViewModels.Base
{
    public class BaseCommand<T> : ICommand
    {
        private readonly Predicate<T> _canExecute;
        private readonly Action<T>? _execute;
        private readonly Func<T, Task>? _executeAsync;

        public BaseCommand(Predicate<T> canExecute, Action<T> execute)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _canExecute = canExecute;
            _execute = execute;
        }

        public BaseCommand(Predicate<T> canExecute, Func<T, Task> executeAsync)
        {
            ArgumentNullException.ThrowIfNull(executeAsync);

            _canExecute = canExecute;
            _executeAsync = executeAsync;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public bool CanExecute(object parameter)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            try
            {
                return _canExecute == null || _canExecute((T)parameter);
            }
            catch
            {
                return true;
            }
        }

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        public async void Execute(object parameter)
#pragma warning restore CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).
        {
            _execute?.Invoke((T)parameter);
            if (_executeAsync != null)
                await _executeAsync((T)parameter);
        }

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        private async Task ExecuteAsync(object parameter) => await _executeAsync((T)parameter);
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
}
