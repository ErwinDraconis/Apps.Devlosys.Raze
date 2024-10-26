using Apps.Devlosys.Core;
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
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.MesMain.ViewModels
{
    public class MesBookingViewModel : ViewModelBase, IViewLoadedAndUnloadedAware, INotificable, IRequestFocus
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

        public MesBookingViewModel(IContainerExtension container) : base(container)
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

        private void OnScanCommandHandler()
        {
            try
            {
                Label1 = string.Empty;
                Label2 = string.Empty;
                Label3 = string.Empty;
                Label4 = string.Empty;
                Valide = false;
                ScanState = "I";

                if (_session.ProjectType is not ProjectEnum.FVT and not ProjectEnum.MLS)
                {
                    switch (TraitementOption)
                    {
                        case TraitementEnum.BOOKING:
                            break;
                        case TraitementEnum.LABLING:
                        case TraitementEnum.BOTH:
                            bool result = StartLabling(SNR);
                            if (!result)
                            {
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
                                    ButtonResult resultMES = _dialogService.ShowDialog(DialogNames.DoubleCheckDialog, new DialogParameters($"snr={SNR}&part={data.FinGood}"));

                                    if (resultMES == ButtonResult.Yes)
                                    {
                                        MesSavingProduct(SNR);
                                    }
                                    else if (resultMES == ButtonResult.No)
                                    {
                                        return;
                                    };
                                }
                            }

                            break;
                        case TraitementEnum.BIN:
                            break;
                        default:
                            break;
                    }

                    if (TraitementOption != TraitementEnum.BIN)
                    {
                        if (_session.IsMESActive && TraitementOption != TraitementEnum.MES)
                        {
                            MesSavingProduct(SNR);
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

        #endregion

        #region Private Methods

        private void SetOptionItems()
        {
            TraitementOption = _session.Traitement;

            TraitementOptions = new ObservableCollection<TraitementModel>()
            {
                new (TraitementEnum.BOOKING, _session.Traitement == TraitementEnum.BOOKING),
                new (TraitementEnum.LABLING, _session.Traitement == TraitementEnum.LABLING),
                new (TraitementEnum.BOTH, _session.Traitement == TraitementEnum.BOTH),
                new (TraitementEnum.MES, _session.Traitement == TraitementEnum.MES),
                new (TraitementEnum.BIN, _session.Traitement == TraitementEnum.BIN)
            };
        }

        private void CheckPreviousStepMLS(string snr)
        {
            if (_session.IsFVTInterlock)
            {
                if (_session.IsPrintManuelleLabel)
                {
                    PrinteMLS(snr);
                }
            }
            else
            {
                PrinteMLS(snr);
            }
        }

        private void CheckPreviousStepFVT(string snr)
        {
            HandelFVT(snr);
        }

        private bool StartLabling(string snr)
        {
            string partNumber = snr.Between("_", "_");

            BinData data = GetDataForLabel(partNumber);
            if (data == null)
            {
                _dialogService.ShowOkDialog(DialogsResource.GlobalWarningTitle, TraitmentResource.NoBinMFoundMessage, OkDialogType.Warning);
                return false;
            }

            PrintResult(true, $"LAST SN : {snr}", $"STATION NO : {_session.Station}", $"REFERENCE : {partNumber}", $"PRODUIT : NULL");

            string snrLast = (_session.LabelType == LabelTypeEnum.TG01) ? snr.Substring(snr.LastIndexOf('_') + 1) : snr.Substring(22, 5).ToString();
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

            return true;
        }

        private async void MesSavingProduct(string snr)
        {
            string lastSnr = (_session.LabelType == LabelTypeEnum.TG01) ? snr.Between("_", "_") : snr.Substring(4, 10).ToString();
            int gg = 0;

            var data = GetDataForLabel(lastSnr);
            if (data == null)
            {
                return;
            }

            string refrence = Regex.Replace(data.FinGood, "^0*", "");
            if (data.Shipping.ToUpper() != "Y")
            {
                return;
            }

            if (_session.IsMESActive)
            {
                _session.IsItacInterlock = false;

                if (!_session.IsItacInterlock)
                {
                    string date = DateTime.Now.AddMinutes(5.0).ToString(_session.DateFormat, CultureInfo.InvariantCulture);
                    string workcenter = _session.WorkCenter;

                    await _api.StartMESAsync(workcenter, refrence, date, snr, "1", "10", _session);
                }
                else if (gg == 0)
                {
                    string date = DateTime.Now.ToString(_session.DateFormat, CultureInfo.InvariantCulture);
                    string workcenter = _session.WorkCenter;

                    await _api.StartMESAsync(workcenter, refrence, date, snr, "1", "10", _session);
                }
                else
                {
                    _dialogService.ShowOkDialog(DialogsResource.GlobalWarningTitle, string.Format(TraitmentResource.BlockedProductMessage, snr), OkDialogType.Warning);
                }
            }
            else
            {
                _dialogService.ShowOkDialog(DialogsResource.GlobalWarningTitle, TraitmentResource.MESDisableMessage, OkDialogType.Warning);
            }
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
            string ZPLString = "^XA^CF0,60^LH0,0^FO2,20^BXN,2,200^FD" + content + "^FS^CF0,10^FO50,20^FD" + part + " " + hadrwarRef + " " + DateTime.Now.Year.ToString("yy") + "^FS^FO50,40^FD" + day + " " + snr + "^FS^FO50,60^FD" + binRef + " ^FS^XZ";
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
