using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure;
using Apps.Devlosys.Infrastructure.Models;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Main.ViewModels.Dialogs
{
    public class ConfigDialogViewModel : DialogViewModelBase
    {
        #region Privates & Protecteds

        private readonly IContainerExtension _container;
        private readonly AppSession _session;

        private ObservableCollection<string> _ports;
        private ObservableCollection<PrintModeEnum> _printModes;
        private ObservableCollection<LabelTypeEnum> _labelTypes;
        private ObservableCollection<UploadMethodEnum> _uploadMethods;
        private ObservableCollection<ProjectEnum> _projects;
        private ProjectEnum _projectEnum;
        private LabelTypeEnum _labelType;
        private PrintModeEnum _printMode;
        private string _port;
        private string _station;
        private string _lablingStation;
        private string _itacServer;
        private string _shippingPrinter;
        private bool _isDoubleCheck;
        private bool _isQualityValidation;
        private bool _isFVTInterlock;
        private bool _isPrintManuelleLabel;
        private string _bin;
        private string _leakHour;
        private string _workCenter;
        private bool _isMESActive;
        private bool _isITACInterlocking;
        private UploadMethodEnum _uploadMethod;
        private string _ftpUsername;
        private string _ftpPassword;
        private string _barflowServer;
        private bool _isMESXMLActive;

        public override event Action<IDialogResult> RequestClose;

        #endregion

        #region Constructors

        public ConfigDialogViewModel(IContainerExtension container, AppSession session)
        {
            _container = container;
            _session = session;

            SaveCommand = new DelegateCommand(SaveCommandHandler).ObservesCanExecute(()=> CanSave);
            ExitCommand = new DelegateCommand(ExitCommandHandler);

            LoadExistanceSerialCom();
        }

        #endregion

        #region Properties

        public ObservableCollection<string> Ports
        {
            get => _ports;
            set => SetProperty(ref _ports, value);
        }

        public ObservableCollection<ProjectEnum> Projects
        {
            get => _projects;
            set => SetProperty(ref _projects, value);
        }

        public ObservableCollection<LabelTypeEnum> LabelTypes
        {
            get => _labelTypes;
            set => SetProperty(ref _labelTypes, value);
        }

        public ObservableCollection<PrintModeEnum> PrintModes
        {
            get => _printModes;
            set => SetProperty(ref _printModes, value);
        }

        public ObservableCollection<UploadMethodEnum> UploadMethods
        {
            get => _uploadMethods;
            set => SetProperty(ref _uploadMethods, value);
        }

        public ProjectEnum ProjectType
        {
            get => _projectEnum;
            set => SetProperty(ref _projectEnum, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public LabelTypeEnum LabelType
        {
            get => _labelType;
            set => SetProperty(ref _labelType, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public PrintModeEnum PrintMode
        {
            get => _printMode;
            set => SetProperty(ref _printMode, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string Port
        {
            get => _port;
            set => SetProperty(ref _port, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string Station
        {
            get => _station;
            set => SetProperty(ref _station, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string LablingStation
        {
            get => _lablingStation;
            set => SetProperty(ref _lablingStation, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string ItacServer
        {
            get => _itacServer;
            set => SetProperty(ref _itacServer, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string ShippingPrinter
        {
            get => _shippingPrinter;
            set => SetProperty(ref _shippingPrinter, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public bool IsDoubleCheck
        {
            get => _isDoubleCheck;
            set => SetProperty(ref _isDoubleCheck, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public bool IsQualityValidation
        {
            get => _isQualityValidation;
            set => SetProperty(ref _isQualityValidation, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public bool IsFVTInterlock
        {
            get => _isFVTInterlock;
            set => SetProperty(ref _isFVTInterlock, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public bool IsPrintManuelleLabel
        {
            get => _isPrintManuelleLabel;
            set => SetProperty(ref _isPrintManuelleLabel, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string Bin
        {
            get => _bin;
            set => SetProperty(ref _bin, value, () => RaisePropertyChanged(nameof(CanSave)));
        }
        public string LeakHours
        {
            get => _leakHour;
            set => SetProperty(ref _leakHour, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string WorkCenter
        {
            get => _workCenter;
            set => SetProperty(ref _workCenter, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public bool IsMESActive
        {
            get => _isMESActive;
            set => SetProperty(ref _isMESActive, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public bool IsITACInterlock
        {
            get => _isITACInterlocking;
            set => SetProperty(ref _isITACInterlocking, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public UploadMethodEnum UploadMethod
        {
            get => _uploadMethod;
            set => SetProperty(ref _uploadMethod, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string FtpUsername
        {
            get => _ftpUsername;
            set => SetProperty(ref _ftpUsername, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string FtpPassword
        {
            get => _ftpPassword;
            set => SetProperty(ref _ftpPassword, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string BarflowServer
        {
            get => _barflowServer;
            set => SetProperty(ref _barflowServer, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public bool IsMESXMLActive
        {
            get => _isMESXMLActive;
            set => SetProperty(ref _isMESXMLActive, value, () => RaisePropertyChanged(nameof(_isMESXMLActive)));
        }

        /* New properties for COM */

        private ObservableCollection<string> _baudRates;
        public ObservableCollection<string> BaudRates
        {
            get => _baudRates;
            set => SetProperty(ref _baudRates, value);
        }

        private ObservableCollection<string> _stopBits;
        public ObservableCollection<string> StopBits
        {
            get => _stopBits;
            set => SetProperty(ref _stopBits, value);
        }

        private ObservableCollection<string> _parities;
        public ObservableCollection<string> Parities
        {
            get => _parities;
            set => SetProperty(ref _parities, value);
        }

        private ObservableCollection<string> _dataBits;
        public ObservableCollection<string> DataBits
        {
            get => _dataBits;
            set => SetProperty(ref _dataBits, value);
        }

        private string _portInterlock;

        public string PortInterlock
        {
            get => _portInterlock;
            set => SetProperty(ref _portInterlock, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        private string _selectedBaudRate;
        public string SelectedBaudRate
        {
            get => _selectedBaudRate;
            set => SetProperty(ref _selectedBaudRate, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        private string _selectedStopBit;
        public string SelectedStopBit
        {
            get => _selectedStopBit;
            set => SetProperty(ref _selectedStopBit, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        private string _selectedParity;
        public string SelectedParity
        {
            get => _selectedParity;
            set => SetProperty(ref _selectedParity, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        private string _selectedDataBit;
        public string SelectedDataBit
        {
            get => _selectedDataBit;
            set => SetProperty(ref _selectedDataBit, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        #endregion

        #region Observables

        public bool CanSave => int.TryParse(LeakHours, out int leak) && leak >= 0;

        #endregion

        #region Commands

        public ICommand SaveCommand { get; set; }

        public ICommand ExitCommand { get; set; }

        #endregion

        #region Command Handlers

        private void SaveCommandHandler()
        {
            _session.ProjectType = ProjectType;
            _session.LabelType = LabelType;

            _session.Station = Station;
            _session.LablingStation = LablingStation;
            _session.ItacServer = ItacServer;
            _session.ShippingPrinter = ShippingPrinter;
            _session.PrintMode = PrintMode;
            _session.Port = Port;

            _session.IsDoubleCheck = IsDoubleCheck;
            _session.IsQualityValidation = IsQualityValidation;
            _session.IsFVTInterlock = IsFVTInterlock;
            _session.IsPrintManuelleLabel = IsPrintManuelleLabel;

            _session.BIN = Bin;
            _session.LeakHours = LeakHours;

            _session.WorkCenter = WorkCenter;
            _session.IsMESActive = IsMESActive;
            _session.IsMESXMLActive = IsMESXMLActive;
            _session.IsItacInterlock = IsITACInterlock;
            _session.UploadType = UploadMethod;

            _session.FtpUsername = FtpUsername;
            _session.FtpPassword = FtpPassword;
            _session.BarFlowServer = BarflowServer;

            _session.PortCOMInterlock   = PortInterlock;
            _session.BaudRatesInterLock = SelectedBaudRate;
            _session.StopBitsInterLock  = SelectedStopBit;
            _session.ParitiesInterLock  = SelectedParity;
            _session.DataBitsInterLock  = SelectedDataBit;

            _session.Save();
            _container.RegisterInstance(_session);

            //DialogHost.CloseDialogCommand.Invoke(true);
            RequestClose?.Invoke(new DialogResult(ButtonResult.Yes));
        }

        private void ExitCommandHandler()
        {
            //DialogHost.CloseDialogCommand.Invoke(false);
            RequestClose?.Invoke(new DialogResult(ButtonResult.No));
        }

        #endregion

        #region Private Methods

        private void LoadExistanceSerialCom()
        {
            Projects = new ObservableCollection<ProjectEnum>() { ProjectEnum.VISUAL, ProjectEnum.INVISIBLE, ProjectEnum.FVT, ProjectEnum.MLS };

            LabelTypes = new ObservableCollection<LabelTypeEnum>() { LabelTypeEnum.TG01, LabelTypeEnum.TS };

            PrintModes = new ObservableCollection<PrintModeEnum>() { PrintModeEnum.NET, PrintModeEnum.SR };

            UploadMethods = new ObservableCollection<UploadMethodEnum>() { UploadMethodEnum.API, UploadMethodEnum.FTP };

            string[] ports = SerialPort.GetPortNames();
            Ports = new ObservableCollection<string>(ports);

            BaudRates = new ObservableCollection<string> { "9600", "19200", "38400", "57600", "115200", "230400", "460800", "921600" };
            StopBits  = new ObservableCollection<string>  { "1", "2" };
            Parities  = new ObservableCollection<string>  { "None", "Odd", "Even" };
            DataBits  = new ObservableCollection<string>  { "7", "8" };
        }

        #endregion

        #region Public Methods 

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = Resources.I18N.ConfigResource.TitelText;

            ProjectType = _session.ProjectType;
            LabelType = _session.LabelType;

            Station = _session.Station;
            LablingStation = _session.LablingStation;
            ItacServer = _session.ItacServer;
            ShippingPrinter = _session.ShippingPrinter;
            PrintMode = _session.PrintMode;
            Port = _session.Port;

            IsDoubleCheck = _session.IsDoubleCheck;
            IsQualityValidation = _session.IsQualityValidation;
            IsFVTInterlock = _session.IsFVTInterlock;
            IsPrintManuelleLabel = _session.IsPrintManuelleLabel;

            Bin = _session.BIN;
            LeakHours = _session.LeakHours;

            WorkCenter = _session.WorkCenter;
            IsMESActive = _session.IsMESActive;
            IsMESXMLActive = _session.IsMESXMLActive;
            IsITACInterlock = _session.IsItacInterlock;
            UploadMethod = _session.UploadType;

            FtpUsername = _session.FtpUsername;
            FtpPassword = _session.FtpPassword;
            BarflowServer = _session.BarFlowServer;

            PortInterlock    = _session.PortCOMInterlock;
            SelectedBaudRate = _session.BaudRatesInterLock;
            SelectedStopBit  = _session.StopBitsInterLock;
            SelectedParity   = _session.ParitiesInterLock;
            SelectedDataBit  = _session.DataBitsInterLock;
        }

        #endregion

    }
}
