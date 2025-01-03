﻿using Apps.Devlosys.Core;
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;


namespace Apps.Devlosys.Modules.Main.ViewModels
{
    public class PanelCheckViewModel : ViewModelBase, IViewLoadedAndUnloadedAware, IRequestFocus, INotificable
    {
        #region Private variables

        public event EventHandler<FocusRequestedEventArgs> FocusRequested;
        private readonly IDialogService _dialogService;
        private readonly IIMSApi _api;
        private AppSession _session;

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

        private bool _isTxtEnabled = true;
        public bool isTxtEnabled
        {
            get => _isTxtEnabled;
            set => SetProperty(ref _isTxtEnabled, value);
        }

        private Visibility _isLoadingGifVisible = Visibility.Collapsed;

        public Visibility isLoadingGifVisible
        {
            get => _isLoadingGifVisible; 
            set => SetProperty(ref _isLoadingGifVisible, value);
        }

        public ISnackbarMessageQueue GlobalMessageQueue { get; set; }

        #endregion

        #region Commands

        public ICommand OnScanCommand { get; set; }

        #endregion

        #region Command handler

        private async void OnScanCommandHandler()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            Log.Information($"***************** Process Booking start *********************");
            isTxtEnabled = false; isLoadingGifVisible = Visibility.Visible;
            Positions.Clear();

            await LoadPositions();

            SNR = string.Empty;
            isTxtEnabled = true; isLoadingGifVisible = Visibility.Collapsed;
            OnFocusRequested("SNR");

            stopwatch.Stop();
            Log.Information($"Process Booking and MES for a panel of {Positions.Count} position , took {stopwatch.Elapsed.TotalSeconds:F2} s");
            GlobalMessageQueue.Enqueue($"Extracted PCBs [{Positions.Count}], " +
                $"Treatment Time: {(stopwatch.Elapsed.TotalMinutes >= 1 ? $"{stopwatch.Elapsed.TotalMinutes:F2} min" 
                : stopwatch.Elapsed.TotalSeconds >= 1 ? $"{stopwatch.Elapsed.TotalSeconds:F2} s" 
                : $"{stopwatch.Elapsed.TotalMilliseconds:F2} ms")}");
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
                try
                {
                    if (position.Status == (int)iTAC_Check_SN_RSLT_ENUM.PART_OK)
                    {
#if RELEASE
                        Log.Information($"ProcessBooking for SN {position.SerialNumber} which has a result of {position.Status}");
                        await ProcessBookingAsync(position.SerialNumber);
#endif
                    }

                }
                catch (Exception ex)
                {
                    Log.Error($"Exception Occured : ProcessBookingAsync - Exception occured for SN {position.SerialNumber} - error : {ex.Message} {Environment.NewLine} {ex.InnerException} ");
                    _dialogService.ShowOkDialog(
                            "Exception Occured", $"title=Exception Occured &message={ex.Message}", OkDialogType.Error);
                }
            }

            // Loop through all PCBs and show Interlock window for scrapped ones
            isLoadingGifVisible = Visibility.Collapsed;
            foreach (var position in panelsResult)
            {
                Log.Information($"{position.SerialNumber} - {position.PositionNumber} - {position.Status}");
                if (position.Status != (int)iTAC_Check_SN_RSLT_ENUM.PART_OK)
                {
                    iTAC_Check_SN_RSLT_ENUM status = Enum.IsDefined(typeof(iTAC_Check_SN_RSLT_ENUM), position.Status)
                                                    ? (iTAC_Check_SN_RSLT_ENUM)position.Status
                                                    : iTAC_Check_SN_RSLT_ENUM.PART_Unknown;

                    string scrapMessage = $"{status} at position {position.PositionNumber} was found.";
                    string dialogTitle = $"Panel Check - {status} Detected";
                    _dialogService.ShowDialog(
                        DialogNames.UnterlockFailDialog,
                        new DialogParameters($"title={dialogTitle} &SNR={position.SerialNumber}&Description={scrapMessage} &CallerWindow=PanelCheckView")
                    );
                }
            }


        }

        private async Task ProcessBookingAsync(string SerialNumber)
        {
            bool loop = false;

            // Verify if iTAC attributes exist  
            var isAttrAppended = await _api.VerifyMESAttrAsync(_session.Station, SerialNumber);
            if (isAttrAppended == 0)
            {
                while (true)
                {
                    var (success, message) = await StartiTACBookingAsync(SerialNumber);

                    if (success)
                    {
                        return;
                    }
                    else
                    {
                        _dialogService.ShowConfirmation("Re-try iTAC booking",
                            $"iTAC Booking Failed.\r\n iTAC booking for {SerialNumber} failed, reason: {message}. \r\n Do you want to retry?",
                            OnConfirm: () => { loop = true; },
                            OnCancel: () =>
                            {
                                // Change color on the display to show that this board has failed in iTAC or MES booking
                                Positions.FirstOrDefault(x => x.SerialNumber == SerialNumber).Status = 10;
                                loop = false; return;
                            }
                        );
                        if (!loop) break;
                    }
                }
                return;
            }

            // Set up for retry logic for MES booking  
            while (true)
            {
                var (success, message) = await MesBookingAsync(SerialNumber);

                if (success)
                {
                    while (true)
                    {
                        (success, message) = await StartiTACBookingAsync(SerialNumber);
                        if (success)
                        {
                            await _api.LockSerialAsync(_session.Station, SerialNumber);
                            return;
                        }
                        else
                        {
                            _dialogService.ShowConfirmation("Re-try iTAC booking",
                            $"iTAC Booking Failed.\r\n iTAC booking for {SerialNumber} failed, reason: {message}. \r\n Do you want to retry?",
                                OnConfirm: () => { loop = true; },
                                OnCancel: () =>
                                {
                                    Positions.FirstOrDefault(x => x.SerialNumber == SerialNumber).Status = 10;
                                    loop = false; return;
                                }
                            );
                            if (!loop) break;
                        }
                    }
                    break;
                }
                else
                {
                    _dialogService.ShowConfirmation("Re-try MES booking",
                            $"MES Booking Failed.\r\n MES booking for {SerialNumber} failed ,reason: {message}. Do you want to retry?",
                            OnConfirm: () => { loop = true; },
                            OnCancel: () =>
                            {
                                Positions.FirstOrDefault(x => x.SerialNumber == SerialNumber).Status = 10;
                                loop = false; return;
                            }
                    );
                    if (!loop) break;
                }
            }

        }

        private async Task<(bool success, string message)> StartiTACBookingAsync(string snr)
        {
            (bool result, string[] outArgs, int code) = await _api.UploadStateAsync(_session.Station, snr, ["SERIAL_NUMBER_STATE"], null);

            if (result)
            {
                (result, string[] outResults, int codeInfo) = await _api.GetSerialNumberInfoAsync(_session.Station, snr, ["PART_DESC", "SERIAL_NUMBER", "PART_NUMBER"]);

                if (!result)
                {
                    string error = $"SN Info for {snr} could not be retrieved! Error: {await _api.GetErrorTextAsync(codeInfo)}";
                    return (false, error);
                }

                bool splitSuccess = await _api.SplitSnFromPanelAsync(_session.Station, snr);

                return (true, $"iTAC booking for {snr} succeeded.");
            }
            else
            {
                string error = $"iTAC booking for {snr} failed! Error: {await _api.GetErrorTextAsync(code)}";
                return (false, error);
            }
        }


        private async Task<(bool success, string message)> MesBookingAsync(string snr)
        {
            bool MES_BOOKING_RSLT = false;
            bool success = false;
            string message = string.Empty;

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < 5; i++)
            {
                (success, message) = await MesSavingProductAsync(snr);
                if (success)
                {
                    await _api.SetUserWhoManAsync(_session.Station, snr, _session.UserName);

                    await _api.AppendMESAttrAsync(_session.Station, snr);

                    MES_BOOKING_RSLT = true;

                    break;
                }
                else
                {
                    await Task.Delay(100);
                }
            }

            stopwatch.Stop();
            Log.Information($"MesBookingAsync executed in [{stopwatch.ElapsedMilliseconds}] ms");

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
            string lastSnr = (snr.Contains("_")) ? snr.Between("_", "_") : snr.Substring(4, 10).ToString();

            var data = GetDataForLabel(lastSnr);
            if (data == null)
            {
                return (false, $"Label data not available for SN {lastSnr}");
            }

            string reference = Regex.Replace(data.FinGood, "^0*", "");

            string date = DateTime.Now.ToString(_session.DateFormat, CultureInfo.InvariantCulture);
            string workcenter = _session.WorkCenter;

            return await _api.StartMESAsync(workcenter, reference, date, snr, "1", "10", _session);

        }

        private BinData GetDataForLabel(string snr)
        {
            BinData data = null;

            string line;

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

                        break;
                    }
                }
            }

            return data;
        }

        public async Task<(bool success, int errCode, string errorDescription)> CheckPcbAsync(string snr)
        {
            (bool result, string errorDescription, int errorCode) = await _api.CheckSerialNumberStateAsync(_session.Station, snr);

            if (result)
            {
                return (true, 0, string.Empty);
            }
            else
            {
                // PCB already booked in iTAC but without MES, this should return true
                if (errorCode == 0)
                {
                    return (true, 0, string.Empty);
                }

                string errorDesc = await _api.GetErrorTextAsync(errorCode);
                return (false, errorCode, errorDesc);
            }
        }

        private void ShowInterlockFailDialog(string errCode, string errDesc, string SerialNumber)
        {
            _dialogService.ShowDialog(
                DialogNames.UnterlockFailDialog,
                new DialogParameters($"title=iTAC Check - ERROR DETECTED &SNR={SerialNumber}&Description=Interlock in iTAC failed with error code [{errCode}] : {errDesc},&CallerWindow=PanelCheckView")
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
