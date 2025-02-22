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
            isTxtEnabled = false; isLoadingGifVisible = Visibility.Visible;
            Positions.Clear();

            await LoadPositions();

            SNR = string.Empty;
            isTxtEnabled = true; isLoadingGifVisible = Visibility.Collapsed;
            OnFocusRequested("SNR");

            stopwatch.Stop();
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
                Log.Warning(error);
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                return;
            }

            if (panelsResult.Count == 1)
            {
                string error = "PCB does not belong to Panel anymore, this configuration is not allowed! Part will not be booked.";
                Log.Warning(error);
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                return;
            }

            // Display panel layout in Gray
            Positions.AddRange(panelsResult);
            Log.Information($"Loaded {panelsResult.Count} positions for SN [{SNR}]. Processing now...");

            // Loop through all PCBs and perform iTAC and MES booking on OK parts,Scrap or Failed parts will be blocked (Interlock)
            foreach (var position in panelsResult)
            {
                try
                {
                    if (position == null)
                    {
                        Log.Warning($"A null position was found in panelsResult for SN [{SNR}]. Skipping...");
                        continue;
                    }
#if RELEASE
                    if (position.Status == (int)iTAC_Check_SN_RSLT_ENUM.PART_OK)
                    {
                        Log.Information($"[START] ProcessBookingAsync: Processing SN [{position.SerialNumber}] at station [{_session.Station}].");
                        await ProcessBookingAsync(position.SerialNumber);
                    }
                    else
                    {
                        Log.Warning($"SN [{position.SerialNumber}] has a failed iTAC status. Interlock window will be shown.");
                        Positions.FirstOrDefault(x => x.SerialNumber == position.SerialNumber).DisplayStatus = (int)position.Status;
                        iTAC_Check_SN_RSLT_ENUM status = Enum.IsDefined(typeof(iTAC_Check_SN_RSLT_ENUM), position.Status)
                                                        ? (iTAC_Check_SN_RSLT_ENUM)position.Status
                                                        : iTAC_Check_SN_RSLT_ENUM.PART_Unknown;

                        string scrapMessage = $"{status} at position {position.PositionNumber} was found.";
                        string dialogTitle = $"Panel Check - {status} Detected";
                        _dialogService.ShowDialog(DialogNames.UnterlockFailDialog,
                            new DialogParameters($"title={dialogTitle} &SNR={position.SerialNumber}&Description={scrapMessage} &CallerWindow=PanelCheckView"));
                    }
#else
                    Positions.FirstOrDefault(x => x.SerialNumber == position.SerialNumber).DisplayStatus = (int)position.Status;
                    await Task.Delay(300);
#endif
                }
                catch (Exception ex)
                {
                    Log.Error($"[EXCEPTION] SN [{position?.SerialNumber ?? "Unknown"}] at station [{_session.Station}] - Error: {ex.Message}\n{ex}");
                    _dialogService.ShowOkDialog("Exception Occured", $"{ex.Message}", OkDialogType.Error);
                }
            }

        }


        private async Task ProcessBookingAsync(string SerialNumber)
        {
            bool loop = false;
            int retryCount = 0;

            // Verify if iTAC attributes exist  
            var isAttrAppended = await _api.VerifyMESAttrAsync(_session.Station, SerialNumber);
            if (isAttrAppended == 0)
            {
                Log.Information($"[CHECK] MES attributes already exist for SN [{SerialNumber}]. Only iTAC booking required.");

                while (true)
                {
                    retryCount++;
                    Log.Information($"[RETRY #{retryCount}] Attempting iTAC booking for SN [{SerialNumber}] at station [{_session.Station}].");

                    var (success, message) = await StartiTACBookingAsync(SerialNumber);

                    if (success)
                    {
                        Log.Information($"[SUCCESS] iTAC booking for SN [{SerialNumber}] completed successfully.");
                        Positions.FirstOrDefault(x => x.SerialNumber == SerialNumber).DisplayStatus = (int)iTAC_Check_SN_RSLT_ENUM.PART_OK;
                        return;
                    }
                    else
                    {
                        Log.Error($"[ERROR] iTAC booking for SN [{SerialNumber}] failed (Attempt #{retryCount}). Reason: {message}");

                        _dialogService.ShowConfirmation("Re-try iTAC booking",
                            $"iTAC Booking Failed.\r\n iTAC booking for {SerialNumber} failed, reason: {message}. \r\n Do you want to retry?",
                            OnConfirm: () => { loop = true; },
                            OnCancel: () =>
                            {
                                Positions.FirstOrDefault(x => x.SerialNumber == SerialNumber).DisplayStatus = (int)iTAC_Check_SN_RSLT_ENUM.BOOKING_OP_FAILED;
                                loop = false;
                            }
                        );
                        if (!loop) break;
                    }
                }
                Log.Warning($"[EXIT] iTAC booking for SN [{SerialNumber}] was not completed after {retryCount} attempts.");
                return;
            }

            // Set up retry logic for MES booking  
            retryCount = 0;
            while (true)
            {
                retryCount++;
                Log.Information($"[RETRY #{retryCount}] Attempting MES booking for SN [{SerialNumber}] at station [{_session.Station}].");

                var (success, message) = await MesBookingAsync(SerialNumber);

                if (success)
                {
                    Log.Information($"[SUCCESS] MES booking for SN [{SerialNumber}] succeeded. Proceeding with iTAC booking.");

                    retryCount = 0;
                    while (true)
                    {
                        retryCount++;
                        Log.Information($"[RETRY #{retryCount}] Attempting iTAC booking for SN [{SerialNumber}] after MES success.");

                        (success, message) = await StartiTACBookingAsync(SerialNumber);
                        if (success)
                        {
                            Log.Information($"[SUCCESS] iTAC booking for SN [{SerialNumber}] succeeded after MES.");
                            await _api.LockSerialAsync(_session.Station, SerialNumber);
                            Positions.FirstOrDefault(x => x.SerialNumber == SerialNumber).DisplayStatus = (int)iTAC_Check_SN_RSLT_ENUM.PART_OK;

                            return;
                        }
                        else
                        {
                            Log.Error($"[ERROR] iTAC booking for SN [{SerialNumber}] failed (Attempt #{retryCount}). Reason: {message}");

                            _dialogService.ShowConfirmation("Re-try iTAC booking",
                                $"iTAC Booking Failed.\r\n iTAC booking for {SerialNumber} failed, reason: {message}. \r\n Do you want to retry?",
                                OnConfirm: () => { loop = true; },
                                OnCancel: () =>
                                {
                                    Positions.FirstOrDefault(x => x.SerialNumber == SerialNumber).DisplayStatus = (int)iTAC_Check_SN_RSLT_ENUM.BOOKING_OP_FAILED;
                                    loop = false;
                                }
                            );
                            if (!loop) break;
                        }
                    }
                    break;
                }
                else
                {
                    Log.Error($"[ERROR] MES booking for SN [{SerialNumber}] failed (Attempt #{retryCount}). Reason: {message}");

                    _dialogService.ShowConfirmation("Re-try MES booking",
                        $"MES Booking Failed.\r\n MES booking for {SerialNumber} failed ,reason: {message}. Do you want to retry?",
                        OnConfirm: () => { loop = true; },
                        OnCancel: () =>
                        {
                            Positions.FirstOrDefault(x => x.SerialNumber == SerialNumber).DisplayStatus = (int)iTAC_Check_SN_RSLT_ENUM.BOOKING_OP_FAILED;
                            loop = false;
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
            bool success          = false;
            string message        = string.Empty;

            for (int i = 0; i < 5; i++)
            {
                (success, message) = await MesSavingProductAsync(snr);
                if (success)
                {
                    // Check explicitly if shipping flag is "Y" before adding attributes
                    var data = GetDataForLabel(snr);
                    if (data != null && data.Shipping.ToUpper() == "Y")
                    {
                        Log.Information($"Add attributes for SN {snr}");
                        await _api.SetUserWhoManAsync(_session.Station, snr, _session.UserName);
                        await _api.AppendMESAttrAsync(_session.Station, snr);
                        MES_BOOKING_RSLT = true;
                    }
                    else
                    {
                        Log.Warning($"MES ATTR will not be added, data in BIN file is set to [{data.Shipping.ToUpper()}]");
                        MES_BOOKING_RSLT = true;
                    }
                    
                    break;
                }
                else  // only retry if MES booking has failed
                {
                    if (message.Contains("MES is inactive")) // MES is not active, do not retry.
                        break;

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
            var data = GetDataForLabel(snr);
            if (data == null)
            {
                Log.Error($"Label data not available for SN {snr}");
                return (false, $"Label data not available for SN {snr}");
            }

            
            if (data.Shipping.ToUpper() != "Y")
            {
                Log.Warning($"No MES for SN {snr} is needed, data in BIN File is [{data.Shipping.ToUpper()}]");
                return (true, $"No MES for SN {snr} is needed, data in BIN File is [{data.Shipping.ToUpper()}]");
            }

            if (!_session.IsMESActive)
            {
                Log.Warning("MES booking is inactive");
                _dialogService.ShowOkDialog(DialogsResource.GlobalWarningTitle, TraitmentResource.MESDisableMessage, OkDialogType.Warning);
                return (false, "MES booking is inactive"); 
            }
            
            string date = DateTime.Now.ToString(_session.DateFormat, CultureInfo.InvariantCulture);
            string workcenter = _session.WorkCenter;
            string reference = Regex.Replace(data.FinGood, "^0*", "");

            return await _api.StartMESAsync(workcenter, reference, date, snr, "1", "10", _session);

        }

        private BinData GetDataForLabel(string snr)
        {
            BinData data = null;
            string line  = string.Empty;
            bool founded = false;
            string interSnr = string.Empty; 

            if (!string.IsNullOrEmpty(snr))
            {
                if (snr.Contains("_"))
                    interSnr = snr.Between("_", "_");
                else
                    interSnr = snr.Substring(4, 10);
            }


            StreamReader file = new(AppDomain.CurrentDomain.BaseDirectory + "\\data\\bin.txt");
            
            while ((line = file.ReadLine()) != null)
            {
                string[] col = line.Split('|');
                if (col.Length > 7)
                {
                    if (col[0] == interSnr)
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

            if (!founded)
            {
                _dialogService.ShowOkDialog("Information", "No record found with this part number, try to add data in bin table ", OkDialogType.Warning);
                Log.Error($"No record found for this SN [{snr}], try to add data in bin table ");
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
