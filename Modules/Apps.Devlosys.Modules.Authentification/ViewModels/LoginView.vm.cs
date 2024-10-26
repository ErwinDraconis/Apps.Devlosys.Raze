using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Modules.Authentification.Views;
using Apps.Devlosys.Resources.I18N;
using Apps.Devlosys.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Authentification.ViewModels
{
    public class LoginViewModel : ViewModelBase, IViewLoadedAndUnloadedAware<LoginView>, INotificable, IRequestFocus
    {
        #region Privates & Protecteds

        private readonly IRegionManager _regionManager;
        private readonly IDialogService _dialogService;
        private readonly IIMSApi _api;

        private string _userName;
        private bool _isConnected;
        private string _message;
        private string _state;
        private bool _taskStart;

        private PasswordBox _password;
        private AppSession _session;

        public event EventHandler<FocusRequestedEventArgs> FocusRequested;

        #endregion

        #region Constructors

        public LoginViewModel(IContainerExtension container) : base(container)
        {
            _regionManager = Container.Resolve<IRegionManager>();
            _dialogService = Container.Resolve<IDialogService>();
            _api = Container.Resolve<IIMSApi>();

            LoginCommand = new DelegateCommand(LoginCommandHandler).ObservesCanExecute(() => CanLogin);

            _session = new();

            ConnectionAsync();
        }

        #endregion

        #region Properties

        public ISnackbarMessageQueue GlobalMessageQueue { get; set; }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value, () => RaisePropertyChanged(nameof(CanLogin)));
        }

        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        public bool TaskStart
        {
            get => _taskStart;
            set => SetProperty(ref _taskStart, value);
        }

        #endregion

        #region Observables

        public bool CanLogin => IsConnected && _password != null && !string.IsNullOrWhiteSpace(_password.Password) && !string.IsNullOrWhiteSpace(UserName);

        #endregion

        #region Commands

        public ICommand LoginCommand { get; set; }

        #endregion

        #region Command Handlers

        private async void LoginCommandHandler()
        {
            try
            {
                TaskStart = true;
                bool result = false;

#if DEBUG
                await Task.Run(() => result = true);
#else
                await Task.Run(() => { result = _api.CheckUser(_session.Station, UserName, _password.Password); }); 
#endif

                if (!result)
                {
                    _dialogService.ShowOkDialog(LoginResource.WrongCredentialsTitle, LoginResource.WrongCredentialsMessage, OkDialogType.Warning);
                }
                else
                {
#if DEBUG
                    int level = UserName == "a" ? 1 : 0;
#else
                    int level = _api.GetUserLevel(_session.Station, UserName);
#endif

                    _session.UserName = UserName;
                    _session.Password = _password.Password;
                    _session.Level = level;

                    Container.RegisterInstance(_session);

                    _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.MainView);
                }

                TaskStart = false;
            }
            catch (Exception ex)
            {
                TaskStart = false;

                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, string.Format(DialogsResource.ConnectionException, (ex.InnerException ?? ex).Message), OkDialogType.Error);
                Log.Error(ex, ex.Message);
            }
        }

        #endregion

        #region Private Methods

        private async void ConnectionAsync()
        {
            try
            {
                TaskStart = true;

                Message = LoginResource.ITACConnectionText;
                State = "S";

                await Task.Run(() =>
                {
#if DEBUG
                    IsConnected = true;
                    Message = LoginResource.ITACConnectedText;
                    State = "T";
#else
                    if (_api.ItacConnection(_session))
                    {
                        IsConnected = true;
                        Message = LoginResource.ITACConnectedText;
                        State = "T";
                    }
                    else
                    {
                        IsConnected = false;
                        Message = LoginResource.ITACNotConnectedText;
                        State = "F";
                    }
#endif
                });

                OnFocusRequested("UserName");

                TaskStart = false;
            }
            catch (Exception ex)
            {
                TaskStart = false;

                Message = LoginResource.ITACNotConnectedText;
                Log.Error(ex, (ex.InnerException ?? ex).Message);
            }
        }

        private void Password_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            RaisePropertyChanged(nameof(CanLogin));
        }

        #endregion

        #region Protected Methodes

        protected virtual void OnFocusRequested(string propertyName)
        {
            FocusRequested?.Invoke(this, new FocusRequestedEventArgs(propertyName));
        }

        #endregion

        #region Public Methods

        public void OnLoaded(LoginView view)
        {
            _password = view.PasswordBox;
            _password.PasswordChanged += Password_PasswordChanged;
        }

        public void OnUnloaded(LoginView view)
        {
            _password.PasswordChanged -= Password_PasswordChanged;
            _password = null;
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        #endregion
    }
}
