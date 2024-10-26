using Apps.Devlosys.Core;
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
using System.Threading.Tasks;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Main.ViewModels.Dialogs
{
    public class LeakCheckDialogViewModel : DialogViewModelBase, IViewLoadedAndUnloadedAware, IRequestFocus
    {
        #region Privates & Protecteds

        private readonly IDialogService _dialogService;
        private readonly IIMSApi _api;
        private readonly AppSession _session;

        private readonly double minutes;

        private string _snr;
        private string _currentSNR;
        private string _timeSpentPerHours;
        private string _timeSpentPerMinutes;
        private string _timeState;
        private string _message;
        private bool _taskStart;

        public event EventHandler<FocusRequestedEventArgs> FocusRequested;
        public override event Action<IDialogResult> RequestClose;

        #endregion

        #region Constructors

        public LeakCheckDialogViewModel(IContainerExtension Container)
        {
            _dialogService = Container.Resolve<IDialogService>();
            _api = Container.Resolve<IIMSApi>();
            _session = Container.Resolve<AppSession>();

            minutes = double.Parse(_session.LeakHours) * 60;

            TimeState = "I";

            CheckSnrCommand = new DelegateCommand(CheckSnrCommandHandler).ObservesCanExecute(() => CanCheck);
            OpenBookingDialogCommand = new DelegateCommand(OpenBookingDialogCommandHandler);
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

        public string TimeSpentPerHours
        {
            get => _timeSpentPerHours;
            set => SetProperty(ref _timeSpentPerHours, value);
        }

        public string TimeSpentPerMinutes
        {
            get => _timeSpentPerMinutes;
            set => SetProperty(ref _timeSpentPerMinutes, value);
        }

        public string TimeState
        {
            get => _timeState;
            set => SetProperty(ref _timeState, value);
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

        #endregion

        #region Observables

        public bool CanCheck => !string.IsNullOrWhiteSpace(SNR);

        #endregion

        #region Commands

        public ICommand CheckSnrCommand { get; set; }

        public ICommand OpenBookingDialogCommand { get; set; }

        #endregion

        #region Command Handlers

        private async void CheckSnrCommandHandler()
        {
            try
            {
                TaskStart = true;

                bool hasError = false;
                string errorTitle = DialogsResource.GlobalWarningTitle;
                string errorMessage = string.Empty;
                OkDialogType errorType = OkDialogType.Warning;

                await Task.Run(() =>
                {
#if DEBUG
                    long unixTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                    int hours = new Random().Next(0, int.Parse(_session.LeakHours) + 6);

                    DateTime date = UnixTimeStampToDateTime(unixTime).AddHours(-hours);
                    DateTime datenow = DateTime.Now;

                    if (datenow.Subtract(date).TotalMinutes < minutes)
                    {
                        CurrentSNR = SNR;
                        TimeSpentPerHours = Math.Round(datenow.Subtract(date).TotalHours, 2) + " Hours";
                        TimeSpentPerMinutes = Math.Round(datenow.Subtract(date).TotalMinutes, 2) + " Minutes";
                        TimeState = "Fail";

                        hasError = true;
                        errorMessage = LeakCheckResource.LeakPartMessage;
                    }
                    else
                    {
                        CurrentSNR = SNR;
                        TimeSpentPerHours = Math.Round(datenow.Subtract(date).TotalHours, 2) + " Hours";
                        TimeSpentPerMinutes = Math.Round(datenow.Subtract(date).TotalMinutes, 2) + " Minutes";
                        TimeState = "Pass";
                    }
#else
                    bool result = _api.GetBookDateMLS("TG01-SMT-LAS-L01-10", SNR, out string station, out string dateUnix, out int code);

                    if (result)
                    {
                        if (station.Contains("MLS"))
                        {
                            dateUnix = dateUnix.Substring(0, 10);

                            DateTime date = UnixTimeStampToDateTime(long.Parse(dateUnix));
                            DateTime datenow = DateTime.Now;

                            if (datenow.Subtract(date).TotalMinutes < minutes)
                            {
                                CurrentSNR = SNR;
                                TimeSpentPerHours = Math.Round(datenow.Subtract(date).TotalHours, 2) + " Hours";
                                TimeSpentPerMinutes = Math.Round(datenow.Subtract(date).TotalMinutes, 2) + " Minutes";
                                TimeState = "Fail";

                                hasError = true;
                                errorMessage = LeakCheckResource.LeakPartMessage;

                                _api.LockSnrItac("TG01-SMT-LAS-L01-10", SNR, out int codeLock);
                            }
                            else
                            {
                                CurrentSNR = SNR;
                                TimeSpentPerHours = Math.Round(datenow.Subtract(date).TotalHours, 2) + " Hours";
                                TimeSpentPerMinutes = Math.Round(datenow.Subtract(date).TotalMinutes, 2) + " Minutes";
                                TimeState = "Pass";

                                long epochTicks = new DateTime(1970, 1, 1).Ticks;
                                long unixTime = (DateTime.UtcNow.Ticks - epochTicks) / 10000000;

                                _api.UnlockSnrItac("TG01-SMT-LAS-L01-10", SNR, out int codeUnclock);
                                _api.UploadState("TG01-FAS-LCK-L01-10", SNR, unixTime, out int codeUpload);
                            }
                        }
                        else if (station.Contains("FCT"))
                        {
                            hasError = true;
                            errorMessage = LeakCheckResource.AlreadyInFCTMessage;
                        }
                        else
                        {
                            hasError = true;
                            errorMessage = LeakCheckResource.TryManuelleLoadingMessage;
                        }
                    }
                    else
                    {
                        hasError = true;

                        errorTitle = DialogsResource.GlobalErrorTitle;
                        errorMessage = _api.GetErrorText(code);
                        errorType = OkDialogType.Error;
                    }
#endif
                });

                if (hasError)
                {
                    _dialogService.ShowOkDialog(errorTitle, errorMessage, errorType);
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

        private void OpenBookingDialogCommandHandler()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.No));

            _dialogService.Show(DialogNames.BookingDialog, () => { });
        }

        #endregion

        #region Private Methodes

        private DateTime UnixTimeStampToDateTime(long unixTimeStamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).LocalDateTime;
        }

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
            Title = LeakCheckResource.HeaderText;
            Message = LeakCheckResource.MessageText;

            OnFocusRequested("SNR");
        }

        #endregion
    }
}
