using System.Windows.Input;

namespace Project_K.ViewModel.Helpers
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;

        public DelegateCommand(Action execute)
        {
            _execute = execute;
        }


        public event EventHandler CanExecuteChanged;


        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => _execute();

    }
}
