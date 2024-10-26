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
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Main.ViewModels.Dialogs
{
    public class UnterlockFailDialogViewModel: DialogViewModelBase, IViewLoadedAndUnloadedAware<UnterlockFailDialog>
    {
        #region Privates & Protecteds variables

        private readonly IDialogService _dialogService;
        private readonly IIMSApi _api;
        private readonly AppSession _session;
        public override event Action<IDialogResult> RequestClose;
        private SerialPort _serialPort;

        #endregion

        #region properties 

        public bool Result { get; private set; } = false;

        private string _snr;
        public string SNR
        {
            get => _snr;
            set => SetProperty(ref _snr, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        private string _scanedSN;
        public string ScanedSN
        {
            get => _scanedSN;
            set => SetProperty(ref _scanedSN, value, () => RaisePropertyChanged(nameof(CanUnblockCommand)));
        }

        private bool _isSerialPortOpen = false;
        public bool IsSerialPortOpen
        {
            get => _isSerialPortOpen;
            set => SetProperty(ref _isSerialPortOpen, value);
        }

        public string CallerWindow { get; set; }

        #endregion

        #region Constructors

        public UnterlockFailDialogViewModel(IContainerExtension Container)
        {
            _dialogService  = Container.Resolve<IDialogService>();
            _api            = Container.Resolve<IIMSApi>();
            _session        = Container.Resolve<AppSession>();

            _session.AppState = Infrastructure.AppStates.BLOCKED;

            UnblockCommand = new DelegateCommand(UnblockCommandHandler).ObservesCanExecute(() => CanUnblockCommand);

            ConfigAndOpenCOMPort();
        }

        #endregion

        #region Methodes

        private void ConfigAndOpenCOMPort()
        {
            try
            {
                var portName = _session.PortCOMInterlock;
                var baudRate = int.Parse(_session.BaudRatesInterLock);
                var parity   = (Parity)Enum.Parse(typeof(Parity), _session.ParitiesInterLock);
                var dataBits = int.Parse(_session.DataBitsInterLock);
                var stopBits = (StopBits)Enum.Parse(typeof(StopBits), _session.StopBitsInterLock);

                _serialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);

                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();

                if(_serialPort.IsOpen)
                    IsSerialPortOpen = true;
                

            }
            catch (Exception ex)
            {
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, string.Format(DialogsResource.GlobalErrorMessage, (ex.InnerException ?? ex).Message), OkDialogType.Error);
            }
        }

        public void OnLoaded(UnterlockFailDialog view)
        {
            
        }

        public void OnUnloaded(UnterlockFailDialog view)
        {
            ScanedSN = string.Empty;
            SNR = string.Empty;

            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();

                _serialPort.DataReceived -= SerialPort_DataReceived;
            }
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>("title");
            SNR   = parameters.GetValue<string>("SNR");
            Description = parameters.GetValue<string>("Description");
            CallerWindow = parameters.GetValue<string>("CallerWindow");
        }

        public override bool CanCloseDialog()
        {
            return Result;
        }

        private string NormalizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty; 
            }
  
            return Regex.Replace(input.Trim(), "[^a-zA-Z0-9_-]", string.Empty);
        }
        #endregion

        #region events

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = _serialPort.ReadLine();
            
            if (!string.IsNullOrEmpty(data) && data.Length > 0)
                ScanedSN = Regex.Replace(data, "[^a-zA-Z0-9_-]", string.Empty);
            
        }

        #endregion

        #region Commands

        public ICommand UnblockCommand { get; set; }

        #endregion

        #region Command Handlers

        private async void UnblockCommandHandler()
        {
            await Task.Run(() =>
            {
                Result = true;
            });

            _session.AppState = Infrastructure.AppStates.NORMAL;
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        #endregion

        #region Observables

        public bool CanUnblockCommand => (NormalizeString(ScanedSN) == NormalizeString(SNR));

        #endregion
    }
}
