using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Resources.I18N;
using Apps.Devlosys.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using MaterialDesignThemes.Wpf.Converters;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections.Generic;
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

        private int SN_FONT_SIZE = 12;
        private string PRODUCT_NAME = string.Empty;
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

        private bool _isSVGFileFound = false;

        public bool IsSVGFileFound
        {
            get => _isSVGFileFound;
            set => SetProperty(ref _isSVGFileFound, value);
        }

        private string _svgFilePath;
        public string SVGFilePath
        {
            get => _svgFilePath;
            set => SetProperty(ref _svgFilePath, value);
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
            
            string SvgFilePath, FinishGood = string.Empty;
            int TotalPCBs = 0;
            if (!isSVGDataAvailable(panelsResult.First().SerialNumber, out SvgFilePath,out TotalPCBs,out FinishGood))
                await ProcessPanelAsync(panelsResult);
            else
                await ProcessPanelAndDisplaySVGAsync(panelsResult, SvgFilePath, TotalPCBs, FinishGood);

        }

        private async Task ProcessPanelAsync(List<PanelPositions> panelsResult)
        {
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
                        var Booking_Rslt = await ProcessBookingAsync(position.SerialNumber);
                        Positions.FirstOrDefault(x => x.SerialNumber == position.SerialNumber).DisplayStatus = (int)Booking_Rslt;
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
                    // Display Real Panel
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

        private async Task ProcessPanelAndDisplaySVGAsync(List<PanelPositions> panelsResult, string SvgPath, int TotalPCBs, string FinishGood)
        {
            // Load default SVG first
            SVGFilePath = SvgPath;
            Log.Information($"Loaded {panelsResult.Count} positions for SN [{SNR}]. Processing now...");

            await Task.Run(async () =>
            {
                string svgContent = File.ReadAllText(SvgPath);
                int iCounter = 1;
                string modifiedSvgPath = SvgPath.Replace(".svg", "_TestResult.svg");

                foreach (var position in panelsResult)
                {
                    try
                    {
                        if (position == null)
                        {
                            Log.Warning($"A null position was found in panelsResult for SN [{SNR}]. Skipping...");
                            iCounter++;
                            continue;
                        }
#if DEBUG
                        svgContent = await UpdatePCBColorOnSVGFile(svgContent, iCounter, position);

                        File.WriteAllText(modifiedSvgPath, svgContent);

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            SVGFilePath = null;
                            SVGFilePath = modifiedSvgPath;
                        });

                        await Task.Delay(200); // Allow UI to refresh

                        iCounter++;
#else
                    if (position.Status == (int)iTAC_Check_SN_RSLT_ENUM.PART_OK)
                    {
                        Log.Information($"[START] ProcessBookingAsync: Processing SN [{position.SerialNumber}] at station [{_session.Station}].");
                        var Booking_Rslt = await ProcessBookingAsync(position.SerialNumber);
                        Positions.FirstOrDefault(x => x.SerialNumber == position.SerialNumber).DisplayStatus = (int)Booking_Rslt;
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


                    // update color on SVG file
                    svgContent = await UpdatePCBColorOnSVGFile(svgContent, iCounter, position);

                    File.WriteAllText(modifiedSvgPath, svgContent);

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        SVGFilePath = null;
                        SVGFilePath = modifiedSvgPath;
                    });

                    iCounter++;
                    await Task.Delay(50); // Allow UI to refresh

#endif
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[EXCEPTION] SN [{position?.SerialNumber ?? "Unknown"}] at station [{_session.Station}] - Error: {ex.Message}\n{ex}");
                        _dialogService.ShowOkDialog("Exception Occured", $"{ex.Message}", OkDialogType.Error);
                    }
                }
            });
        }

        private async Task<iTAC_Check_SN_RSLT_ENUM> ProcessBookingAsync(string SerialNumber)
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
                        //UpdatePCBColorOnThePanel(SerialNumber, iTAC_Check_SN_RSLT_ENUM.PART_OK);
                        return iTAC_Check_SN_RSLT_ENUM.PART_OK;
                    }
                    else
                    {
                        Log.Error($"[ERROR] iTAC booking for SN [{SerialNumber}] failed (Attempt #{retryCount}). Reason: {message}");

                        _dialogService.ShowConfirmation("Re-try iTAC booking",
                            $"iTAC Booking Failed.\r\n iTAC booking for {SerialNumber} failed, reason: {message}. \r\n Do you want to retry?",
                            OnConfirm: () => { loop = true; },
                            OnCancel: () => { loop = false; }
                        );
                        if (!loop) break;
                    }
                }
                Log.Warning($"[EXIT] iTAC booking for SN [{SerialNumber}] was not completed after {retryCount} attempts.");
                return iTAC_Check_SN_RSLT_ENUM.BOOKING_OP_FAILED;
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
                            return iTAC_Check_SN_RSLT_ENUM.PART_OK;
                        }
                        else
                        {
                            Log.Error($"[ERROR] iTAC booking for SN [{SerialNumber}] failed (Attempt #{retryCount}). Reason: {message}");

                            _dialogService.ShowConfirmation("Re-try iTAC booking",
                                $"iTAC Booking Failed.\r\n iTAC booking for {SerialNumber} failed, reason: {message}. \r\n Do you want to retry?",
                                OnConfirm: () => { loop = true; },
                                 OnCancel: () => { loop = false; }
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
                        OnCancel: () =>  { loop = false; }
                    );
                    if (!loop) break;
                }
            }

            return iTAC_Check_SN_RSLT_ENUM.BOOKING_OP_FAILED;
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

        #endregion

        #region Helper methodes

        private static Dictionary<string, string> ReadIniFile(string filePath)
        {
            var configData = new Dictionary<string, string>();

            foreach (var line in File.ReadAllLines(filePath))
            {
                // Ignore empty lines and comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";")) continue;

                // Extract key-value pairs
                var parts = line.Split(new char[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    configData[parts[0].Trim()] = parts[1].Trim();
                }
            }

            return configData;
        }

        private static Dictionary<string, Dictionary<string, string>> ReadIniFile(string path, string section)
        {
            var result = new Dictionary<string, Dictionary<string, string>>();
            string currentSection = null;
            Dictionary<string, string> sectionData = null;

            try
            {
                using (var reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();

                        // Skip comments or empty lines
                        if (string.IsNullOrEmpty(line) || line.StartsWith(";") || line.StartsWith("#"))
                            continue;

                        // Detect section headers
                        if (line.StartsWith("[") && line.EndsWith("]"))
                        {
                            currentSection = line.Trim('[', ']');
                            sectionData = new Dictionary<string, string>();

                            // Add section to result if it matches the target section
                            if (currentSection.Equals(section, StringComparison.OrdinalIgnoreCase))
                            {
                                result[currentSection] = sectionData;
                            }
                        }
                        // Parse key-value pairs
                        else if (currentSection != null && sectionData != null)
                        {
                            var splitIndex = line.IndexOf('=');
                            if (splitIndex > 0)
                            {
                                var key = line.Substring(0, splitIndex).Trim();
                                var value = line.Substring(splitIndex + 1).Trim();
                                sectionData[key] = value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading INI file: " + ex.Message);
            }

            return result;
        }

        private bool isSVGDataAvailable(string SerialNumber, out string SvgFilePath, out int TotalPCBs, out string FinishGood)
        {
            // Initialize the output parameters
            SvgFilePath = string.Empty;
            TotalPCBs = 0;
            FinishGood = string.Empty;

            IsSVGFileFound = false;
            if (string.IsNullOrEmpty(SerialNumber))
                return false;

            string _exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string _panelLayoutPath = Path.Combine(_exeDirectory, "PanelLayout");
            string _svgFolderPath = Path.Combine(_panelLayoutPath, "SVG");
            string _configFilePath = Path.Combine(_panelLayoutPath, "config.ini");

            if (!Directory.Exists(_panelLayoutPath))
                return false;

            if (!Directory.Exists(_svgFolderPath))
                return false;

            if (!File.Exists(_configFilePath))
            {
                Log.Error($"Config file not found at {_configFilePath}");
                return false;
            }

            var data = GetDataForLabel(SerialNumber);
            if (data == null)
            {
                Log.Error("Missing required data in bin file");
                return false;
            }

            // Read and parse config file
            var configData = ReadIniFile(_configFilePath, data.FinGood);
            if (!configData.ContainsKey(data.FinGood))
            {
                Log.Error("Missing required keys (SVG_File or PCB_Numbers) in config file");
                return false;
            }

            var sectionData = configData[data.FinGood];

            if (!sectionData.ContainsKey("SVG_File") || !sectionData.ContainsKey("PCB_Numbers"))
            {
                Log.Error("Missing required keys (SVG_File or PCB_Numbers) in config file");
                return false;
            }

            // Set the SVG file path
            SvgFilePath = Path.Combine(_svgFolderPath, sectionData["SVG_File"] + ".svg");

            if(!File.Exists(SvgFilePath))
            {
                Log.Error("Defined SVG file does not exist in the SVG folder, SVG : " + SvgFilePath);
                return false;
            }

            int pcbCount = 0;

            // Try to parse PCB_Numbers
            if (!int.TryParse(sectionData["PCB_Numbers"], out pcbCount) || pcbCount <= 0)
            {
                Log.Error("PCB_Numbers is not a valid positive number.");
                return false;
            }

            int.TryParse(sectionData["SN_FONT_SIZE"], out SN_FONT_SIZE);
            PRODUCT_NAME = sectionData["PRODUCT_NAME"];
           

            // Set the total PCB count
            TotalPCBs = pcbCount;
            FinishGood = data.FinGood;
            IsSVGFileFound = true;
            return true;
        }

        private string statusToColor(int status)
        {
            return status switch
            {
                0 => "Green",     // OK
                1 => "OrangeRed", // NOK
                2 => "DarkRed",   // Scrap
                3 => "Gray",      // Initial state, pending test
                10 => "Azure",     // Booking failed
                _ => "Yellow"     // Unknown
            };
        }

        private string statusToMsgStr(int status)
        {
            return status switch
            {
                0 => "Pass",     
                1 => "Fail", 
                2 => "Scrap",   
                3 => "Pending",     
                10 => "MES or ITAC Failed",     
                _ => "Unknown"     
            };
        }

        private (double, double) CalculateCentroid(string pathData)
        {
            var matches = Regex.Matches(pathData, @"(\d+\.?\d*),(\d+\.?\d*)");

            double sumX = 0, sumY = 0;
            int count = matches.Count;

            if (count == 0) return (0, 0); // Avoid division by zero

            foreach (Match match in matches)
            {
                sumX += double.Parse(match.Groups[1].Value);
                sumY += double.Parse(match.Groups[2].Value);
            }

            double centerX = sumX / count;
            double centerY = sumY / count;

            return (Math.Round(centerX, 0), Math.Round(centerY, 0));
        }

        private BinData GetDataForLabel(string snr)
        {
            BinData data = null;
            string line = string.Empty;
            bool founded = false;
            string interSnr = string.Empty;

            if (snr.Length < 15)
                return null;

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

        private async Task<string> UpdatePCBColorOnSVGFile(string svgContent, int iCounter, PanelPositions position)
        {
            // Change the fill color dynamically
            string fillPattern = $"fill=\"[^\"]*\"(?=.*id=\"pcb_{iCounter}\")";
            string fillReplacement = $"fill=\"{statusToColor(position.DisplayStatus)}\"";
            svgContent = Regex.Replace(svgContent, fillPattern, fillReplacement);

            // Extract PCB path data (using flexible pattern)
            string pathPattern = $"<path[^>]*d=\"([^\"]+)\"[^>]*id=\"pcb_{iCounter}\"";
            Match match = Regex.Match(svgContent, pathPattern);

            if (match.Success)
            {
                string pathData = match.Groups[1].Value;
                var (centerX, centerY) = CalculateCentroid(pathData);

                // Insert the serial number text at the centroid
                string textElement = $"<text x=\"{centerX}\" y=\"{centerY}\" font-size=\"{SN_FONT_SIZE}\" fill=\"black\" text-anchor=\"middle\"> [{position.PositionNumber}]  - [{statusToMsgStr(position.Status)}] - {position.SerialNumber}</text>";
                svgContent = Regex.Replace(svgContent, $"(<path[^>]*id=\"pcb_{iCounter}\"[^>]*>)", $"$1\n{textElement}");
            }

            return svgContent;
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
