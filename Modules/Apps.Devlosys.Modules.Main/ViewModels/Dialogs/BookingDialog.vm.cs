using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Resources.I18N;
using Apps.Devlosys.Services.Interfaces;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Data;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Main.ViewModels.Dialogs
{
    public class BookingDialogViewModel : DialogViewModelBase, IViewLoadedAndUnloadedAware, IRequestFocus
    {
        #region Privates & Protecteds

        private readonly IDialogService _dialogService;
        private readonly IIMSApi _api;

        private string _snr;
        private string _currentSNR;
        private string _bookingState;
        private bool _taskStart;

        public event EventHandler<FocusRequestedEventArgs> FocusRequested;

        #endregion

        #region Constructors

        public BookingDialogViewModel(IContainerExtension Container)
        {
            _dialogService = Container.Resolve<IDialogService>();
            _api = Container.Resolve<IIMSApi>();

            BookingState = "I";

            BookSnrCommand = new DelegateCommand(BookSnrCommandHandler).ObservesCanExecute(() => CanBook);
        }

        #endregion

        #region Properties

        public string SNR
        {
            get => _snr;
            set => SetProperty(ref _snr, value);
        }

        public string CurrentSNR
        {
            get => _currentSNR;
            set => SetProperty(ref _currentSNR, value);
        }

        public string BookingState
        {
            get => _bookingState;
            set => SetProperty(ref _bookingState, value);
        }

        public bool TaskStart
        {
            get => _taskStart;
            set => SetProperty(ref _taskStart, value);
        }

        #endregion

        #region Observables

        public bool CanBook => !string.IsNullOrWhiteSpace(SNR);

        #endregion

        #region Commands

        public ICommand BookSnrCommand { get; set; }

        #endregion

        #region Command Handlers

        private async void BookSnrCommandHandler()
        {
            try
            {
                TaskStart = true;

                bool hasError = false;
                string errorMessage = string.Empty;

                await Task.Run(() =>
                {
                    CurrentSNR = SNR;

#if DEBUG
                    bool result = new Random().Next(0, 2) == 0;
                    if (result)
                    {
                        BookingState = "Pass";
                    }
                    else
                    {
                        BookingState = "Fail";

                        hasError = true;
                        errorMessage = "Erro while booking!";
                    }
#else
                    bool result = _api.LunchBooking("TG01-FAS-MLS-L07-10", SNR, out int code);

                    if (result)
                    {
                        BookingState = "Pass";
                    }
                    else
                    {
                        BookingState = "Fail";

                        hasError = true;
                        errorMessage =  _api.GetErrorText(code);
                    }
#endif
                });

                if (hasError)
                {
                    _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, errorMessage, OkDialogType.Error);
                }

                TaskStart = false;
            }
            catch (Exception ex)
            {
                TaskStart = false;

                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, string.Format(DialogsResource.ConnectionException, (ex.InnerException ?? ex).Message), OkDialogType.Error);
                Log.Error(ex, ex.Message);
            }

            SNR = string.Empty;
            OnFocusRequested("SNR");
        }

        #endregion

        #region Private Methodes

        #endregion

        #region Protected Methodes

        protected virtual void OnFocusRequested(string propertyName)
        {
            FocusRequested?.Invoke(this, new FocusRequestedEventArgs(propertyName));
        }

        #endregion

        #region Public Methods 

        public void OnLoaded()
        {
            OnFocusRequested("SNR");
        }

        public void OnUnloaded() { }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = BookingResource.HeaderText;

            OnFocusRequested("SNR");
        }

        #endregion
    }
}
