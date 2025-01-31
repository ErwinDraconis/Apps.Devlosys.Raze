using Apps.Devlosys.Controls.Helpers;
using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Services.Interfaces;
using Prism.Commands;
using Prism.Ioc;
using Serilog;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Main.ViewModels
{
    public class ContenueContenantViewModel : ViewModelBase, IRequestFocus, IViewLoadedAndUnloadedAware
    {
        #region Private variables

        public event EventHandler<FocusRequestedEventArgs> FocusRequested;
        private readonly IIMSApi _api;
        private AppSession _session;

        #endregion

        #region class constructor

        public ContenueContenantViewModel(IContainerExtension container) : base(container)
        {
            _api = Container.Resolve<IIMSApi>();
            _session = Container.Resolve<AppSession>();

            OnGaliaTxtKeyDown = new DelegateCommand(OnGaliaTxtKeyDownHandler);
            OnSNTxtKeyDown    = new DelegateCommand(OnSNTxtKeyDownHandler);
            ResultList        = new ObservableCollection<GaliaResult>();
        }

        #endregion

        #region Properties

        private string _snr;
        public string SNR
        {
            get => _snr;
            set => SetProperty(ref _snr, value);
        }

        private string _snrGalia;
        public string SNRGalia
        {
            get => _snrGalia;
            set => SetProperty(ref _snrGalia, value);
        }

        private bool _isGaliaTxtEnabled = true;
        public bool isGaliaTxtEnabled
        {
            get => _isGaliaTxtEnabled;
            set => SetProperty(ref _isGaliaTxtEnabled, value);
        }

        private bool _isSNTxtEnabled = false;
        public bool isSNTxtEnabled
        {
            get => _isSNTxtEnabled;
            set => SetProperty(ref _isSNTxtEnabled, value);
        }

        private bool _isPrintManuelleLabel = true;
        public bool IsPrintManuelleLabel
        {
            get => _isPrintManuelleLabel;
            set
            {
                SetProperty(ref _isPrintManuelleLabel, value);
                ClearTextControls();
            } 
        }

        public string GaliaToggleCaption => IsPrintManuelleLabel ? "Enable Galia" : "Disable Galia";

        private string _galiaNumber;
        public string GaliaNumber
        {
            get => _galiaNumber;
            set => SetProperty(ref _galiaNumber, value);
        }

        private string _galiaPN;
        public string GaliaPN
        {
            get => _galiaPN;
            set => SetProperty(ref _galiaPN, value);
        }

        private bool _isGaliaDataValid = true;
        public bool IsGaliaDataValid
        {
            get => _isGaliaDataValid;
            set => SetProperty(ref _isGaliaDataValid, value);
        }

        private Visibility _rslt_1_Visibility = Visibility.Collapsed;

        public Visibility Rslt_1_Visibility
        {
            get => _rslt_1_Visibility;
            set => SetProperty(ref _rslt_1_Visibility, value);
        }

        private ObservableCollection<GaliaResult> _resultList;
        public ObservableCollection<GaliaResult> ResultList
        {
            get => _resultList;
            set => SetProperty(ref _resultList, value);
        }

        private string _pnFromScannedSN;
        public string PnFromScannedSN
        {
            get => _pnFromScannedSN;
            set => SetProperty(ref _pnFromScannedSN, value);
        }

        private bool _isScannedSNValid = true;
        public bool IsScannedSNValid
        {
            get => _isScannedSNValid;
            set => SetProperty(ref _isScannedSNValid, value);
        }

        #endregion

        #region Commands & handlers

        public ICommand OnSNTxtKeyDown { get; set; }

        private void OnSNTxtKeyDownHandler()
        {
            IsScannedSNValid = true;
            if (string.IsNullOrEmpty(SNR) || SNR.Length < 10)
            {
                IsScannedSNValid = false;
                return;
            }

            try
            {
                if (SNR.Substring(0, 4) == "TG01")
                {
                    PnFromScannedSN = SNR.Substring(5, 10).TrimStart('0');
                    IsScannedSNValid = true;
                    PerformComparison();
                }
                else if (SNR.Substring(0, 4) == "L000")
                {
                    PnFromScannedSN = SNR.Substring(4, 10).TrimStart('0');
                    IsScannedSNValid = true;
                    PerformComparison();
                }
                else
                {
                    PnFromScannedSN = string.Empty;
                    IsScannedSNValid = false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Exception occurred in class ContenueContenantViewModel function {nameof(OnSNTxtKeyDownHandler)}: {ex.Message}");
            }
        }

        public ICommand OnGaliaTxtKeyDown { get; set; }

        private void OnGaliaTxtKeyDownHandler()
        {
            IsPrintManuelleLabel = false;
        }
        #endregion

        #region Protected Methodes

        protected virtual void OnFocusRequested(string propertyName)
        {
            FocusRequested?.Invoke(this, new FocusRequestedEventArgs(propertyName));
        }

        #endregion

        #region private methodes

        private void ClearTextControls()
        {
            if(GaliaToggleCaption == "Enable Galia")
            {
                SNRGalia = SNR = string.Empty;
                GaliaPN = GaliaNumber = string.Empty;
                ResultList.Clear();
                Rslt_1_Visibility = Visibility.Collapsed;
                isGaliaTxtEnabled = true;
                isSNTxtEnabled = false;
                OnFocusRequested("SNRGalia");
            }

            if(GaliaToggleCaption == "Disable Galia")
            {
                if (CheckIfGaliaDataAreCorrect())
                {
                    IsGaliaDataValid = true;
                    isGaliaTxtEnabled = false;
                    isSNTxtEnabled = true;
                    OnFocusRequested("SNR");
                    Rslt_1_Visibility = Visibility.Visible;
                }
                else
                {
                    IsGaliaDataValid = false;
                    IsPrintManuelleLabel = true;
                }
            }
        }

        private bool CheckIfGaliaDataAreCorrect()
        {
            GaliaNumber = GaliaPN = string.Empty;
            IsGaliaDataValid = true;
            if (string.IsNullOrWhiteSpace(SNRGalia))
                return false;

            string[] snrParts = SNRGalia.Split(';');

            if (snrParts.Length < 2)
                return false; // Not enough data in the string

            if (snrParts[0].Length > 1)
            {
                GaliaNumber = snrParts[0].Substring(1);
            }

            if (snrParts[1].Length > 1)
            {
                GaliaPN = snrParts[1].Substring(1);
            }

            // Ensure both values are populated
            return !string.IsNullOrEmpty(GaliaNumber) && !string.IsNullOrEmpty(GaliaPN);
        }

        private void PerformComparison()
        {
            int comparisonResult = (GaliaPN == PnFromScannedSN) ? 0 : 1; 

            var result = new GaliaResult
            {
                GaliaNb = GaliaNumber,
                GaliaPN = GaliaPN,
                PCBSN = SNR,
                PCBPN = PnFromScannedSN,
                Status = comparisonResult
            };

            ResultList.Add(result);

            // Write the result to a CSV file
            CsvHelper.WriteToCsv(result);

            SNR = string.Empty;
            OnFocusRequested("SNR");
        }


        #endregion

        #region Public methodes

        public void OnLoaded()
        {
            OnFocusRequested("SNRGalia");
        }

        public void OnUnloaded()
        {

        }
        
        #endregion

    }

}
