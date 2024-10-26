﻿using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Infrastructure.Params;
using Apps.Devlosys.Resources.I18N;
using Apps.Devlosys.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Main.ViewModels
{
    public class TraitmentViewModel : ViewModelBase, IViewLoadedAndUnloadedAware, INotificable, IRequestFocus
    {
        #region Privates & Protecteds

        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogService _dialogService;
        private readonly IContentDialogService _contentDialogService;
        private readonly IIMSApi _api;

        private AppSession _session;

        private readonly SubscriptionToken @token;

        private ObservableCollection<MenuButton> _menu_buttons;
        private ObservableCollection<TraitementModel> _traitementOption;
        private MenuButton _selectedMenuButton;
        private string _snr;
        private TraitementEnum _traitement_option;
        private string _label1;
        private string _label2;
        private string _label3;
        private string _label4;
        private bool _valide;
        private string _scanState;
        private string _date_display;
        private string _time_display;
        private bool _taskStart;

        private SerialPort serialPort1 = new();
        public event EventHandler<FocusRequestedEventArgs> FocusRequested;

        #endregion

        #region Constructors

        public TraitmentViewModel(IContainerExtension container) : base(container)
        {
            _eventAggregator = Container.Resolve<IEventAggregator>();
            _dialogService = Container.Resolve<IDialogService>();
            _contentDialogService = Container.Resolve<IContentDialogService>();
            _api = Container.Resolve<IIMSApi>();
            _session = Container.Resolve<AppSession>();

            OnOptionChangedCommand = new DelegateCommand<TraitementEnum?>(OnOptionChangedCommandHandler);
            OnScanCommand = new DelegateCommand(OnScanCommandHandler);

            @token = _eventAggregator.GetEvent<ConfigChangedEvent>().Subscribe(OnConfigChangedEvent);

            SetOptionItems();
        }

        #endregion

        #region Properties

        public ISnackbarMessageQueue GlobalMessageQueue { get; set; }

        public ObservableCollection<MenuButton> MenuButtons
        {
            get => _menu_buttons;
            set => SetProperty(ref _menu_buttons, value);
        }

        public ObservableCollection<TraitementModel> TraitementOptions
        {
            get => _traitementOption;
            set => SetProperty(ref _traitementOption, value);
        }

        public MenuButton SelectedMenuButton
        {
            get => _selectedMenuButton;
            set
            {
                SetProperty(ref _selectedMenuButton, value);

                _selectedMenuButton?.Action?.Invoke();
            }
        }

        public TraitementEnum TraitementOption
        {
            get => _traitement_option;
            set => SetProperty(ref _traitement_option, value);
        }

        public string SNR
        {
            get => _snr;
            set => SetProperty(ref _snr, value);
        }

        public string Label1
        {
            get => _label1;
            set => SetProperty(ref _label1, value);
        }

        public string Label2
        {
            get => _label2;
            set => SetProperty(ref _label2, value);
        }

        public string Label3
        {
            get => _label3;
            set => SetProperty(ref _label3, value);
        }

        public string Label4
        {
            get => _label4;
            set => SetProperty(ref _label4, value);
        }

        public bool Valide
        {
            get => _valide;
            set => SetProperty(ref _valide, value);
        }

        public string ScanState
        {
            get => _scanState;
            set => SetProperty(ref _scanState, value);
        }

        public string DateDisplay
        {
            get => _date_display;
            set => SetProperty(ref _date_display, value);
        }

        public string TimeDisplay
        {
            get => _time_display;
            set => SetProperty(ref _time_display, value);
        }

        public bool TaskStart
        {
            get => _taskStart;
            set => SetProperty(ref _taskStart, value);
        }

        #endregion

        #region Observables

        public bool CanScan => !string.IsNullOrWhiteSpace(SNR);

        #endregion

        #region Commands

        public ICommand OnOptionChangedCommand { get; set; }

        public ICommand OnScanCommand { get; set; }

        #endregion

        #region Command Handlers

        private void OnOptionChangedCommandHandler(TraitementEnum? traitement)
        {
            TraitementOption = traitement.Value;

            _session.Traitement = traitement.Value;
            _session.Save();

            OnFocusRequested("SNR");
        }

        private async void OnScanCommandHandler()
        {
            string errCode, errDesc;
            try
            {
                bool pcbState = false;

                Label1 = string.Empty;
                Label2 = string.Empty;
                Label3 = string.Empty;
                Label4 = string.Empty;
                Valide = false;
                ScanState = "I";

#if DEBUG
                bool resultTest = new Random().Next(0, 2) == 0;

                PrintVisualLabel("content", "imidiatPart", "day", "snr", "binRef", "A");

                if (resultTest)
                    PrintResult(true, $"LAST SN : {SNR}", $"STATION NO : {_session.Station}", $"REFERENCE : TG_00101212", $"PRODUIT : 5421548");
                else
                    PrintResult(false, $"{SNR} : Faild");

                return;
#endif

                if (_session.ProjectType is not ProjectEnum.FVT and not ProjectEnum.MLS)
                {
                    switch (TraitementOption)
                    {
                        case TraitementEnum.BOOKING:

                            ProcessBookingAsync();
                            break;

                        case TraitementEnum.LABLING:
                        case TraitementEnum.BOTH:

                            pcbState = CheckPcb(SNR, out errCode, out errDesc);

                            if (pcbState)
                            {
                                bool result = StartLabling(SNR);
                                if (!result)
                                {
                                    PrintResult(false, $"{SNR} : Faild");
                                    return;
                                }
                            }
                            else
                            {
                                _dialogService.ShowDialog(DialogNames.UnterlockFailDialog, new DialogParameters($"title=Interlock - ERROR DETECTED &SNR={SNR} &Description=Interlock in iTAC failed with error code [{errCode}] : {errDesc}"));
                                PrintResult(false, $"{SNR} : Failde");
                                return;
                            }

                            break;
                        case TraitementEnum.MES:
                            if (_session.IsMESActive)
                            {
                                BinData data = GetDataForLabel(SNR.Between("_", "_"));

                                if (data != null)
                                {
                                    ButtonResult result = _dialogService.ShowDialog(DialogNames.DoubleCheckDialog, new DialogParameters($"snr={SNR}&part={data.FinGood}"));

                                    if (result == ButtonResult.Yes)
                                    {
                                        await MesSavingProduct(SNR);
                                    }
                                    else if (result == ButtonResult.No)
                                    {
                                        _dialogService.ShowDialog(DialogNames.QualityValidationDialog, new DialogParameters($"title={DoubleCheckResource.BlockTitle}&description={DoubleCheckResource.BlockMessage}"));

                                        return;
                                    };
                                }
                            }

                            break;
                        case TraitementEnum.BIN:
                            try
                            {
                                string[] vs = SNR.Split(new string[1] { "[GS]" }, StringSplitOptions.None);
                                bool result = _api.GetBinData(vs[10].Substring(2), out string[] bindata, out int code);
                                if (result)
                                {
                                    PrintResult(true, $"UID : {bindata[3]}", $"Initial QTY : {bindata[2]}", $"Machine QTY : {bindata[1]}", $"Part number : {bindata[0]}");
                                }
                                else
                                {
                                    string error = _api.GetErrorText(code);
                                    _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);

                                    PrintResult(false, error);
                                }
                            }
                            catch (Exception ex)
                            {
                                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, string.Format(DialogsResource.GlobalErrorMessage, (ex.InnerException ?? ex).Message), OkDialogType.Error);
                            }

                            break;
                        default:
                            break;
                    }

                    if (TraitementOption != TraitementEnum.BIN || TraitementOption != TraitementEnum.BOOKING)
                    {
                        _api.SetUserWhoMan(_session.Station, SNR, _session.UserName);

                        if (_session.IsMESActive && TraitementOption != TraitementEnum.MES)
                        {
                            await MesSavingProduct(SNR);
                        }
                    }
                }
                else if (_session.ProjectType == ProjectEnum.MLS)
                {
                    CheckPreviousStepMLS(SNR);
                }
                else if (_session.ProjectType == ProjectEnum.FVT)
                {
                    if (_session.IsFVTInterlock)
                    {
                        CheckPreviousStepFVT(SNR);
                    }
                    else
                    {
                        HandelFVT(SNR);
                    }
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, string.Format(DialogsResource.GlobalErrorMessage, (ex.InnerException ?? ex).Message), OkDialogType.Error);
            }
            finally
            {
                SNR = string.Empty;
                OnFocusRequested("SNR");
            }
        }

        private async void ProcessBookingAsync()
        {
            // 1: Check if booking is for Board only (board in panel not allowed) 
            var ppResult = await _api.GetPanelSNStateAsync(_session.Station, SNR);

            if (ppResult != null || ppResult.Count > 1)
            {
                string error = "Panel Booking is not allowed in this menu.";
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                PrintResult(false, "Panel Booking is not allowed in this menu.", "Please scan SN of a PCB only");
                return;
            }

            // 2: Check SN State  
            string errCode = string.Empty;
            string errDesc = string.Empty;
            var pcbState   = await Task.Run(()=> CheckPcb(SNR, out errCode, out errDesc));
            
            if (!pcbState)
            {
                // iTAC check failed : lock the app and show Error Dialog, then exit.
                _dialogService.ShowDialog(
                    DialogNames.UnterlockFailDialog,
                    new DialogParameters($"title=iTAC Check - ERROR DETECTED &SNR={SNR}&Description=Interlock in iTAC failed with error code [{errCode}] : {errDesc},&CallerWindow=TraitmentView")
                );
                PrintResult(false, $"iTAC check for {SNR} : Failed", $"Error code [{errCode}]", $"Error Descirption: {errDesc}");
                return;
            }

            // 3: Verify if iTAC attributes exist  
            var isAttrAppended = await Task.Run(()=> _api.VerifyMESAttr(_session.Station, SNR));
            
            // 3.1 - Attribute exists, proceed with iTAC booking only, then exit.  
            if (isAttrAppended == 0)
            {
                await Task.Run(()=> StartBooking(SNR));
                return;
            }

            // 3.2 - Attribute does not exist, append attributes and perform MES booking
            var appendMesAttrRslt = await Task.Run(() => _api.SetUserWhoMan(_session.Station, SNR, _session.UserName));
            appendMesAttrRslt    |= await Task.Run(() => _api.AppendMESAttr(_session.Station, SNR));
            if (appendMesAttrRslt != 0)
            {
                //PrintResult(false, $"MES Attributes not added correctly for SN {SNR} : Please re-try again");
                //return;
            }

            // Set up for retry logic for MES booking  
            bool retry = true;
            while (retry)
            {
                // Attempt MES Booking  
                if (await MesSavingProduct(SNR)) // If MES booking is successful, proceed with iTAC booking  
                {
                    await Task.Run(() => StartBooking(SNR));
                    return;
                    
                }
                else // MES booking failed, prompt the user to retry or cancel  
                {  
                    ButtonResult check = _dialogService.ShowDialog(
                        "Re-try MES booking",
                        new DialogParameters($"title=MES Booking Failed &message=MES booking for {SNR} failed. Do you want to retry?")
                    );

                    if (check == ButtonResult.No)
                    {
                        PrintResult(false, $"MES booking for {SNR} : Failed");
                        return;
                    }
                    else
                    {
                        retry = false; // Allow only one retry more  
                    }
                }
            }

            // If all retries fail, log the final result  
            PrintResult(false, $"MES booking for {SNR} : Failed after retry");
        }

        #endregion

        #region Private Methods

        private void SetOptionItems()
        {
            if (_session.Level != 1 && _session.Traitement == TraitementEnum.LABLING)
            {
                _session.Traitement = TraitementEnum.BOOKING;
            }

            TraitementOption = _session.Traitement;

            TraitementOptions = new ObservableCollection<TraitementModel>()
            {
                new (TraitementEnum.BOOKING, _session.Traitement == TraitementEnum.BOOKING),
                new (TraitementEnum.LABLING, _session.Traitement == TraitementEnum.LABLING, _session.Level == 1),
                new (TraitementEnum.BOTH, _session.Traitement == TraitementEnum.BOTH),
                new (TraitementEnum.MES, _session.Traitement == TraitementEnum.MES),
                new (TraitementEnum.BIN, _session.Traitement == TraitementEnum.BIN)
            };
        }

        public bool CheckPcb(string snr, out string errCode, out string errorDescription)
        {
            bool result = _api.CheckSerialNumberState(_session.Station, snr, out _, out string errorCode);

            if (result)
            {
                errorDescription = String.Empty;
                errCode = String.Empty;
                return true;
            }
            else
            {
                errorDescription = _api.GetErrorText(int.Parse(errorCode));
                errCode = errorCode;
                return false;
            }
        }

        private void CheckPreviousStepMLS(string snr)
        {
            if (_session.IsFVTInterlock)
            {
                bool result = _api.CheckSerialNumberState(_session.Station, snr, out _, out string errorCode);

                if (result)
                {
                    if (_session.IsPrintManuelleLabel)
                    {
                        PrinteMLS(snr);
                    }
                }
                else
                {
                    string error = _api.GetErrorText(int.Parse(errorCode));
                    _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                }
            }
            else
            {
                PrinteMLS(snr);
            }
        }

        private void CheckPreviousStepFVT(string snr)
        {
            bool result = _api.CheckSerialNumberState(_session.Station, snr, out _, out string errorCode);
            if (result)
            {
                HandelFVT(snr);
            }
            else
            {
                string error = _api.GetErrorText(int.Parse(errorCode));
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
            }
        }

        private bool StartBooking(string snr)
        {
            bool result = _api.UploadState(_session.Station, snr, new string[1] { "SERIAL_NUMBER_STATE" }, null, out string[] outArgs, out int code);

            if (result)
            {
                result = _api.GetSerialNumberInfo(_session.Station, snr, new string[3] { "PART_DESC", "SERIAL_NUMBER", "PART_NUMBER" }, out string[] outResults, out int codeInfo);
                if (result)
                {
                    PrintResult(true, $"LAST SN : {snr}", $"STATION NO : {_session.Station}", $"REFERENCE : {outResults[2]}", $"PRODUIT : {outResults[0]}");
                }
                else
                {
                    string error = _api.GetErrorText(codeInfo);
                    _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                    PrintResult(false, $"Get Serial Number Info for SN : {snr} Faild", $"Error code {codeInfo}", $"Error Description {error}");
                }

                return true;
            }
            else
            {
                string error = _api.GetErrorText(code);
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);

                return false;
            }
        }


        private bool StartLabling(string snr)
        {
            if (TraitementOption == TraitementEnum.BOTH)
            {
                if (!GetState(snr))
                {
                    return false;
                }
            }

            bool result = _api.GetSerialNumberInfo(_session.LablingStation, snr, new string[3] { "PART_DESC", "SERIAL_NUMBER", "PART_NUMBER" }, out string[] results, out int code);
            if (!result)
            {
                string error = _api.GetErrorText(code);
                _dialogService.ShowOkDialog("Error", error, OkDialogType.Error);

                return false;
            }

            string partDescription = results[0];
            string serialNumber = results[1];
            string partNumber = results[2];

            BinData data = GetDataForLabel(partNumber);
            if (data == null)
            {
                _dialogService.ShowOkDialog(DialogsResource.GlobalWarningTitle, TraitmentResource.NoBinMFoundMessage, OkDialogType.Warning);
                return false;
            }

            PrintResult(true, $"LAST SN : {snr}", $"STATION NO : {_session.Station}", $"REFERENCE : {partNumber}", $"PRODUIT : {partDescription}");

            string snrLast = (_session.LabelType == LabelTypeEnum.TG01) ? serialNumber.Substring(serialNumber.LastIndexOf('_') + 1) : serialNumber.Substring(22, 5).ToString();
            string bin = _session.BIN.Trim();
            string day = DateTime.Now.DayOfYear.ToString("000");
            string year = DateTime.Now.ToString("yy");
            string imidiatPart = Regex.Replace(data.PartNumberSFG.Trim(), "^0*", "");
            string binRef = data.BinRef.Trim();
            string HardwareRef = data.HardwareRef.Trim() == "-1" ? "" : data.HardwareRef.Trim();

            string content = imidiatPart + HardwareRef + year + day + snrLast.Substring(3) + bin + binRef;

            if (_session.ProjectType == ProjectEnum.VISUAL)
            {
                PrintVisualLabel(content, imidiatPart, day, snrLast.Substring(3), binRef, HardwareRef);
            }
            else
            {
                PrintInVisualLabel(content);
            }

            if (TraitementOption == TraitementEnum.BOTH)
            {
                ButtonResult check = _dialogService.ShowDialog(DialogNames.DoubleCheckDialog, new DialogParameters($"snr={SNR}&part={imidiatPart}"));
                if (check == ButtonResult.Yes)
                {
                    return StartBooking(snr);
                }
                else if (check == ButtonResult.No)
                {
                    _dialogService.ShowDialog(DialogNames.QualityValidationDialog, new DialogParameters($"title={DoubleCheckResource.BlockTitle}&description={DoubleCheckResource.BlockMessage}"));

                    return false;
                };
            }

            return true;
        }

        private bool GetState(string snr)
        {
            bool result = _api.CheckSerialNumberState(_session.Station, snr, out _, out string errorCode);

            if (!result)
            {
                if (errorCode != "206")
                {
                    string error = _api.GetErrorText(int.Parse(errorCode));
                    _dialogService.ShowOkDialog(DialogsResource.GlobalErrorMessage, error ?? "", OkDialogType.Error);
                }
                else
                {
                    _dialogService.ShowOkDialog(DialogsResource.GlobalErrorMessage, "Please check product state", OkDialogType.Error);
                }
            }

            return result;
        }

        private async Task<bool> MesSavingProduct(string snr)
        {
            string lastSnr = (_session.LabelType == LabelTypeEnum.TG01) ? snr.Between("_", "_") : snr.Substring(4, 10).ToString();
            int gg = 0;

            var data = GetDataForLabel(lastSnr);
            if (data == null)
            {
                return false;
            }

            string refrence = Regex.Replace(data.FinGood, "^0*", "");
            if (data.Shipping.ToUpper() != "Y")
            {
                return true;
            }
            
            if (_session.IsMESActive)
            {
                if (!_session.IsItacInterlock)
                {
                    var result = await StartMESWithDelay(snr, refrence, 5.0);
                    if (result.status == "fail")
                    {
                        _dialogService.ShowOkDialog(DialogsResource.GlobalWarningTitle, result.reason, OkDialogType.Warning);
                        return false;
                    }
                }
                else if (gg == 0)
                {
                    var result = await StartMESNow(snr, refrence);
                    if (result.status == "fail")
                    {  
                        _dialogService.ShowOkDialog(DialogsResource.GlobalWarningTitle, result.reason, OkDialogType.Warning);
                        return false;
                    }
                }
                else
                {
                    _dialogService.ShowOkDialog(DialogsResource.GlobalWarningTitle, string.Format(TraitmentResource.BlockedProductMessage, snr), OkDialogType.Warning);
                    return false;
                }
            }
            else
            {
                _dialogService.ShowOkDialog(DialogsResource.GlobalWarningTitle, TraitmentResource.MESDisableMessage, OkDialogType.Warning);
                return false; // MES is inactive 
            }

            return true;
        }

        private async Task<(string status, string reason)> StartMESWithDelay(string snr, string reference, double delayMinutes)
        {
            string date = DateTime.Now.AddMinutes(delayMinutes).ToString(_session.DateFormat, CultureInfo.InvariantCulture);
            string workcenter = _session.WorkCenter;

            return await _api.StartMESAsync(workcenter, reference, date, snr, "1", "10", _session);
        }

        private async Task<(string status, string reason)> StartMESNow(string snr, string reference)
        {
            string date = DateTime.Now.ToString(_session.DateFormat, CultureInfo.InvariantCulture);
            string workcenter = _session.WorkCenter;

            return await _api.StartMESAsync(workcenter, reference, date, snr, "1", "10", _session);
        }


        private void HandelFVT(string snr)
        {
            if (snr.Contains("L90171691"))
            {
                PrinteFCL($"L90171858{DateTime.Now:yyMMdd}{snr.Substring(18)}A1A");
            }
            else if (snr.Contains("000L374327") || snr.Contains("000L374325"))
            {
                string year = DateTime.Now.ToString("yy");
                string day = DateTime.Now.DayOfYear.ToString();
                string srnLast = snr.Substring(19);
                string sf = snr.Contains("000L374327") ? "000L374327" : "000L374325";
                BinData data = GetDataForLabel(sf);
                string bin = data.BinRef;
                string hardref = data.HardwareRef;
                string finGood = data.FinGood;

                string content = finGood + hardref + year + day + srnLast + bin;

                PrintMXB(content);
            }
            else if (snr.Contains("L90193458") || snr.Contains("L90193457"))
            {
                string year = DateTime.Now.ToString("yy");
                string day = DateTime.Now.DayOfYear.ToString();
                string srnLast = snr.Substring(19);
                string sf = snr.Contains("L90193458") ? "0L90193458" : "0L90193457";
                string spechchar = snr.Contains("L90193458") ? "R" : "L";
                BinData data = GetDataForLabel(sf);
                string bin = data.BinRef;
                string hardref = data.HardwareRef;
                string finGood = data.FinGood;

                string content = finGood + hardref + year + day + srnLast + bin;

                PrintP2QO(content, finGood, hardref, year, day, srnLast, bin, spechchar);
            }
            else if (snr.ToLower().Contains("B592242003".ToLower()) || snr.ToLower().Contains("B592242002".ToLower()))
            {
                string line = "ML";
                string year = DateTime.Now.ToString("yy");
                string day = DateTime.Now.ToString("dd");
                string month = DateTime.Now.ToString("MM");
                string sf = snr.Contains("B592242003") ? "B592242003" : "B592242002";
                string spechchar = snr.Contains("B592242003") ? "R" : "L";
                string project = snr.Contains("B592242003") ? "516 Base" : "516 High";
                BinData data = GetDataForLabel(sf);
                string[] hardsoft = data.HardwareRef.Split(',');
                string finGood = data.FinGood;

                string content = finGood + "_" + line + "_" + hardsoft[0].ToUpper() + "_" + hardsoft[1].ToUpper() + "_" + day + month + year + "_" + snr.Substring(19);

                PrintJEEP(content, project, hardsoft[0].ToUpper(), DateTime.Now.ToString("dd/MM/yy"), finGood, hardsoft[1].ToUpper());
            }
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

            if (!founded)
            {
                _dialogService.ShowOkDialog("Information", "No record found with this part number, try to add data in bin table ", OkDialogType.Warning);
            }

            return data;
        }

        private void OpenComAndSendData(string zpl)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    serialPort1.Open();
                }
                else
                {
                    serialPort1.PortName = _session.Port;
                    serialPort1.BaudRate = 9600;

                    serialPort1.Open();
                }
                serialPort1.Write(zpl);
            }
            catch (Exception ex)
            {
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, (ex.InnerException ?? ex).Message, OkDialogType.Error);
            }
            finally
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                }
            }
        }

        private void PrintResult(bool succes, string label1, string label2 = null, string label3 = null, string label4 = null)
        {
            Valide = succes;
            ScanState = Valide ? "Pass" : "Fail";

            Label1 = label1;
            Label2 = label2;
            Label3 = label3;
            Label4 = label4;
        }

        #region Print Methods

        public void PrintVisualLabel(string content, string part, string day, string snr, string binRef, string hadrwarRef)
        {
            string ZPLString = "^XA^CF0,60^LH0,0^FO2,20^BXN,2,200^FD" + content + "^FS^CF0,10^FO50,20^FD" + part + " " + hadrwarRef + " " + DateTime.Now.ToString("yy") + "^FS^FO50,40^FD" + day + " " + snr + "^FS^FO50,60^FD" + binRef + " ^FS^XZ";
            Print(ZPLString);
        }

        public void PrintInVisualLabel(string content)
        {
            string ZPLString = "^XA^FO4,3^BQ,2,2^FDQA," + content + "^FS^XZ";
            Print(ZPLString);
        }

        private void PrinteMLS(string content)
        {
            string ZPLString = "^XA^FO2,3^BQ,2,2^FDQA," + content + "^FS^XZ";
            Print(ZPLString);
        }

        private void PrinteFCL(string content)
        {
            string ZPLString = "^XA^CF0,60^LH0,0^FO60,17^BXN,2,200^FD" + content + "^FS^XZ";
            Print(ZPLString);
        }

        private void PrintMXB(string content)
        {
            string ZPLString = "^XA^CF0,60^LH0,0^FO60,10^BXN,2,200^FD" + content + "^FS^CF0,10^FO106,10^FDVW270:MD-E9-1148^FS^FO106,25^FD13.5V-HB:22W^FS^FO106,40^FDVW276:MD-E9-1298^FS^FO106,55^FD13.5V-HB:29W^FS^XZ";
            Print(ZPLString);
        }

        private void PrintP2QO(string content, string valeoRef, string hardRef, string year, string day, string snr, string bin, string spc)
        {
            string ZPLString = "^XA^CF0,15^FO160,15^FDMD  E9     13.5V^FS^CF0,15^FO160,40^FDDRL : 16.2W/PL:3W/^FS^FO160,70^FDTI:17.9W " + spc + "^FS^FO160,100^FD" + valeoRef + " " + hardRef + " " + year + "^FS^FO160,130^FD" + day + " " + snr + " " + bin + "^FS^LL200,^LH0,0^FO80,40^BXN,4,200^FD" + content + "^FS^XZ";
            Print(ZPLString);
        }

        private void PrintJEEP(string content, string project, string softRef, string productionDate, string valeoRef, string hardRef)
        {
            string ZPLString = "^XA^CF0,15^FO80,15^FD" + project + "^FS^FO180,15^FDHW:" + hardRef + "^FS^FO180,40^FDSW:" + softRef + "^FS^CF0,30^FO180,70^FDVALEO^FS^CF0,15^FO180,102^FD" + productionDate + "   ML^FS^FO80,130^FDMADE IN MOROCCO        " + valeoRef + "^FS^LL200,^LH0,0^FO80,40^BXN,3,200^FD" + content + "^FS^XZ";
            Print(ZPLString);
        }

        private void Print(string data)
        {
            if (_session.PrintMode == PrintModeEnum.SR)
            {
                OpenComAndSendData(data);
            }
            else
            {
                PrintUsingWlan(data);
            }
        }

        private void PrintUsingWlan(string ZPLString)
        {
            try
            {
                TcpClient client = new();
                client.Connect(_session.ShippingPrinter, 6101);

                StreamWriter writer = new(client.GetStream());
                writer.Write(ZPLString);
                writer.Flush();
                writer.Close();
                client.Close();
            }
            catch (Exception ex)
            {
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, (ex.InnerException ?? ex).Message, OkDialogType.Error);
            }
        }

        #endregion

        #endregion

        #region Event Handlers

        private void OnConfigChangedEvent()
        {
            _session = Container.Resolve<AppSession>();
            OnFocusRequested("SNR");
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

        public void OnUnloaded()
        {
            _eventAggregator.GetEvent<ConfigChangedEvent>().Unsubscribe(@token);
        }

        #endregion
    }
}