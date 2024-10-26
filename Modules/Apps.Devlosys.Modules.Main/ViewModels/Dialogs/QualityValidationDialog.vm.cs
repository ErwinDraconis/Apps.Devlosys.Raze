using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Modules.Main.Views.Dialogs;
using Apps.Devlosys.Resources.I18N;
using Apps.Devlosys.Services.Interfaces;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Main.ViewModels.Dialogs
{
    public class QualityValidationDialogViewModel : DialogViewModelBase, IViewLoadedAndUnloadedAware<QualityValidationDialog>, IRequestFocus
    {
        #region Privates & Protecteds

        private readonly IDialogService _dialogService;
        private readonly IIMSApi _api;
        private readonly AppSession _session;

        private string _userName;
        private string _description;
        private string _message;
        private bool _taskStart;

        private PasswordBox _password;

        public event EventHandler<FocusRequestedEventArgs> FocusRequested;
        public override event Action<IDialogResult> RequestClose;

        #endregion

        #region Constructors

        public QualityValidationDialogViewModel(IContainerExtension Container)
        {
            _dialogService = Container.Resolve<IDialogService>();
            _api = Container.Resolve<IIMSApi>();
            _session = Container.Resolve<AppSession>();

            LoginCommand = new DelegateCommand(LoginCommandHandler).ObservesCanExecute(() => CanLogin);

            _session.AppState = Infrastructure.AppStates.BLOCKED;
        }

        #endregion

        #region Properties

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value, () => RaisePropertyChanged(nameof(CanLogin)));
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public bool TaskStart
        {
            get => _taskStart;
            set => SetProperty(ref _taskStart, value);
        }

        public bool Result { get; private set; }

        #endregion

        #region Observables

        public bool CanLogin => _password != null && !string.IsNullOrWhiteSpace(_password.Password) && !string.IsNullOrWhiteSpace(UserName);

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
                Result = false;

#if DEBUG
                await Task.Run(() =>
                {
                    Result = true;
                });

                _session.AppState = Infrastructure.AppStates.NORMAL;
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
#else
                await Task.Run(() => { Result = _api.CheckUser(_session.Station, UserName, _password.Password); });

                if (Result)
                {
                    int level = _api.GetUserLevel(_session.Station, UserName);
                    if (level is 0 or 1)
                    {
                        _dialogService.ShowConfirmation(QualityValidationResource.ConfirmationTitle, QualityValidationResource.ConfirmationMessage, () =>
                        {
                            _session.AppState = Infrastructure.AppStates.NORMAL;
                            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
                        });
                    }
                    else
                    {
                        _dialogService.ShowOkDialog(QualityValidationResource.AccessDeniedTitle, QualityValidationResource.AccessDeniedMessage, OkDialogType.Warning);
                    }
                }
                else
                {
                    _dialogService.ShowOkDialog(QualityValidationResource.WrongCredentialsTitle, QualityValidationResource.WrongCredentialsMessage, OkDialogType.Warning);
                    OnFocusRequested("UserName");
                }
#endif

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

        public void OnLoaded(QualityValidationDialog view)
        {
            _password = view.PasswordBox;
            _password.PasswordChanged += Password_PasswordChanged;

            OnFocusRequested("UserName");
        }

        public void OnUnloaded(QualityValidationDialog view)
        {
            _password.PasswordChanged -= Password_PasswordChanged;
            _password = null;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>("title");
            Description = parameters.GetValue<string>("description");
        }

        public override bool CanCloseDialog()
        {
            return Result;
        }

        #endregion
    }
}
