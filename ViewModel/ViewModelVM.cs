using Project_K.ViewModel.Helpers;

namespace Project_K.ViewModel
{
    internal class ViewModelVM : NotifyPropertyChangedBase
    {
        private object _viewModel;
        public object ViewModel
        {
            get => _viewModel;
            set => _UpdateField(ref _viewModel, value);
        }

        // public readonly SwitchViewVM _switchViewVM;
        //public readonly LogInVM _logInVM;
        //public readonly RegisterVM _registerVM;
       // public LogInVM LogInVM { get; }
      //  public RegisterVM RegisterVM { get; }

        public RoboWorksVM RoboWorksVM { get; }

        public ViewModelVM()
        {
            RoboWorksVM = new RoboWorksVM();

            //LogInVM = new LogInVM
            //{
            //    // BackCommand = new DelegateCommand(() => ViewModel = _switchViewVM),
            //    ShowLoginCommand = new DelegateCommand(() => ViewModel = LogInVM),
            //    ShowRegisterCommand = new DelegateCommand(() => ViewModel = RegisterVM)

            //};

            //RegisterVM = new RegisterVM
            //{
            //    BackCommand = new DelegateCommand(() => ViewModel = LogInVM),
            //    //ShowLoginCommand = new DelegateCommand(() => ViewModel = _logInVM),
            //    ShowRegisterCommand = new DelegateCommand(() => ViewModel = RegisterVM)

            //};

            //_switchViewVM = new SwitchViewVM
            //{
            //    ShowLoginCommand = new DelegateCommand(() => ViewModel = _logInVM),
            //    ShowRegisterCommand = new DelegateCommand(() => ViewModel = _registerVM)
            //};

            ViewModel = RoboWorksVM;
        }

    }
}
