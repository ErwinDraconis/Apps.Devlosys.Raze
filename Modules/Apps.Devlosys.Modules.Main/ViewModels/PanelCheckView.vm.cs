using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Resources.I18N;
using Apps.Devlosys.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;


namespace Apps.Devlosys.Modules.Main.ViewModels
{
    public class PanelCheckViewModel : ViewModelBase, IViewLoadedAndUnloadedAware, IRequestFocus
    {
        #region Private variables

        public event EventHandler<FocusRequestedEventArgs> FocusRequested;
        private readonly IDialogService _dialogService;
        private readonly IIMSApi _api;
        private AppSession       _session;

        #endregion

        #region Constructor

        public PanelCheckViewModel(IContainerExtension container) : base(container)
        {
            _dialogService = container.Resolve<IDialogService>();
            _api = Container.Resolve<IIMSApi>();
            _session = Container.Resolve<AppSession>();

            OnScanCommand = new DelegateCommand(OnScanCommandHandler);
            Positions = new ObservableCollection<PanelPositions>();
        }

        #endregion

        #region Properties

        private string _snr;
        public string SNR
        {
            get => _snr;
            set => SetProperty(ref _snr, value);
        }

        private ObservableCollection<PanelPositions> _positions;
        public ObservableCollection<PanelPositions> Positions
        {
            get => _positions;
            set => SetProperty(ref _positions, value);
        }

        #endregion

        #region Commands

        public ICommand OnScanCommand { get; set; }

        #endregion

        #region Command handler

        private async void OnScanCommandHandler()
        {
            Positions.Clear();

            await LoadPositions();

            OnFocusRequested("SNR");
            SNR = string.Empty;
        }

        #endregion

        #region Protected Methodes

        protected virtual void OnFocusRequested(string propertyName)
        {
            FocusRequested?.Invoke(this, new FocusRequestedEventArgs(propertyName));
        }

        #endregion

        #region Public & private methodes

        private async Task LoadPositions()
        {
            var panelsResult = await _api.GetPanelSNStateAsync(_session.Station, SNR);
 
            if (panelsResult == null || panelsResult.Count == 0)
            {
                string error = $"There is no panel record found for this SN [{SNR}]. An empty list is returned: count {panelsResult?.Count}";
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                return;
            }
 
            if (panelsResult.Count == 1)
            {
                string error = "PCB does not belong to Panel anymore, this configuration is not allowed! Part will not be booked.";
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                return;
            }

            // populate the view first (so to let the user see which PCB is Ok and which is not)
            Positions.AddRange(panelsResult);

            // Loop through all PCBs and perform iTAC booking on OK part first
            foreach (var position in panelsResult)
            {
                if (position.Status == (int)iTAC_Check_SN_RSLT_ENUM.PART_OK)
                {
#if RELEASE
                    await StartBookingAsync(position.SerialNumber);
#endif
                }
            }

            // Loop through all PCBs and show Interlock window for scrapped ones
            foreach (var position in panelsResult)
            {

                if (position.Status == (int)iTAC_Check_SN_RSLT_ENUM.PART_SCRAP)
                {
                    string scrapMessage = $"Scrapped part at position {position.PositionNumber} was found.";
                    string dialogTitle = "Panel Check - Scrapped part Detected";
                    _dialogService.ShowDialog(
                        DialogNames.UnterlockFailDialog,
                        new DialogParameters($"title={dialogTitle} &SNR={position.SerialNumber}&Description={scrapMessage} &CallerWindow=PanelCheckView")
                    );
                }
            }
        }

        private async Task StartBookingAsync(string serialNumber)
        {
            bool success   = false;
            string message = string.Empty;

            var (pcbState, errCode, errDesc) = await CheckPcbAsync(SNR);
            if (!pcbState)
            {
                ShowInterlockFailDialog(errCode, errDesc);
                return;
            }

            (success, message) = await MesBookingAsync(SNR);
            if (success)
            {
                if (await StartiTACBookingAsync(SNR))
                {
                    await _api.LockSerialAsync(_session.Station, SNR);
                }
            }
        }

        // Call the asynchronous version of UploadState
        private async Task<bool> StartiTACBookingAsync(string snr)
        {
            
            (bool result, string[] outArgs, int code) = await _api.UploadStateAsync(_session.Station, snr, ["SERIAL_NUMBER_STATE"], null);

            if (result)
            {
                (result, string[] outResults, int codeInfo) = await _api.GetSerialNumberInfoAsync(_session.Station, snr, ["PART_DESC", "SERIAL_NUMBER", "PART_NUMBER"]);

                if (!result)
                { 
                    string error = $"SN Info for {snr} could not be retrieved ! error : {await _api.GetErrorTextAsync(codeInfo)}" ;
                    _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                }

                return true;
            }
            else
            {
                string error = await _api.GetErrorTextAsync(code);
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                return false;
            }
        }

        private bool StartBooking(string snr)
        {
            bool result = _api.UploadState(_session.Station, snr, ["SERIAL_NUMBER_STATE"], null, out string[] outArgs, out int code);

            if (result)
            {
                result = _api.GetSerialNumberInfo(_session.Station, snr, ["PART_DESC", "SERIAL_NUMBER", "PART_NUMBER"], out string[] outResults, out int codeInfo);
                if (result)
                {
                    Log.Information($"SN {snr} booked successfully : {outResults[0]} - {outResults[1]} - {outResults[2]}");
                }
                else
                {

                    string error = _api.GetErrorText(codeInfo);
                    _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                }

                return true;
            }
            else
            {
                string error = _api.GetErrorText(code);
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                Log.Error($"Upload SN {snr} for station {_session.Station} Failed, error : {error}");
                return false;
            }
        }

        private async Task<(bool success, string message)> MesBookingAsync(string snr)
        {
            bool MES_BOOKING_RSLT = false;
            bool success   = false; 
            string message = string.Empty; 

            for (int i = 0; i < 5; i++)
            {
                (success, message) = await MesSavingProductAsync(snr);
                if (success)
                {
                    MES_BOOKING_RSLT = true;
                    break;
                }
                else
                {
                    await Task.Delay(100);
                }    
            }

            if (MES_BOOKING_RSLT)
            {
                return (true, $"MES booking for {snr}: Succeeded");
            }
            else
            {
                return (false, $"MES booking for {snr}: Failed, error : {message}");
            }
        }


        private async Task<(bool success, string message)> MesSavingProductAsync(string snr)
        {
            string lastSnr = (_session.LabelType == LabelTypeEnum.TG01) ? snr.Between("_", "_") : snr.Substring(4, 10).ToString();

            var data = GetDataForLabel(lastSnr);
            if (data == null)
            {
                return (false,$"Label data not available for SN {lastSnr}");
            }

            string refrence = Regex.Replace(data.FinGood, "^0*", "");
            

            var result = await StartMESNowAsync(snr, refrence);

            if (result.status == "fail")
            {
                _dialogService.ShowOkDialog(DialogsResource.GlobalWarningTitle, result.reason, OkDialogType.Warning);
                return (false, result.reason);
            }

            return (true,null);
        }

        private BinData GetDataForLabel(string snr)
        {
            BinData data = null;

            string line;
            bool founded = false;

            StreamReader file = new(AppDomain.CurrentDomain.BaseDirectory + "\\data\\bin.txt");

            while ((line = file.ReadLine()) != null)
            {
                string[] col = line.Split('|');
                if (col.Length > 7)
                {
                    if (col[0] == snr)
                    {
                        data = new BinData()
                        {
                            Identification = col[0],
                            PartNumberSFG = col[1],
                            PartDescription = col[2],
                            BinRef = col[3],
                            HardwareRef = col[4],
                            FinGood = col[5],
                            Shipping = col[6],
                            Quantity = col[7],
                        };

                        founded = true;

                        break;
                    }
                }
            }

            return data;
        }

        private async Task<(string status, string reason)> StartMESNowAsync(string snr, string reference)
        {
            string date = DateTime.Now.ToString(_session.DateFormat, CultureInfo.InvariantCulture);
            string workcenter = _session.WorkCenter;

            return await _api.StartMESAsync(workcenter, reference, date, snr, "1", "10", _session);
        }

        public async Task<(bool success, string errCode, string errorDescription)> CheckPcbAsync(string snr)
        {
            (bool result, string errorCode, string errorDescription) = await _api.CheckSerialNumberStateAsync(_session.Station, snr);

            if (result)
            {
                return (true, string.Empty, string.Empty);
            }
            else
            {
                // PCB already booked in iTAC but without MES, this should return true
                if (errorCode == "0")
                {
                    return (true, "Ok", string.Empty);
                }

                string errorDesc = await _api.GetErrorTextAsync(int.Parse(errorCode));
                return (false, errorCode, errorDesc);
            }
        }

        private void ShowInterlockFailDialog(string errCode, string errDesc)
        {
            _dialogService.ShowDialog(
                DialogNames.UnterlockFailDialog,
                new DialogParameters($"title=iTAC Check - ERROR DETECTED &SNR={SNR}&Description=Interlock in iTAC failed with error code [{errCode}] : {errDesc},&CallerWindow=TraitmentView")
            );

        }
        public void OnLoaded()
        {
            OnFocusRequested("SNR");
        }

        public void OnUnloaded()
        {
           
        }

        #endregion

    }

}
