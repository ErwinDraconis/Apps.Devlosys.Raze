﻿using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
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

        private void OnScanCommandHandler()
        {
            LoadPositions();
        }

        #endregion

        #region Protected Methodes

        protected virtual void OnFocusRequested(string propertyName)
        {
            FocusRequested?.Invoke(this, new FocusRequestedEventArgs(propertyName));
        }

        #endregion

        #region Public & private methodes

        private async void LoadPositions()
        {
            Positions.Clear();

            // Get Data from iTAC
            var ppResult = await _api.GetPanelSNStateAsync(_session.Station, SNR);

            // Check if the result is null or empty  
            if (ppResult == null || ppResult.Count == 0)
            {
                string error = $"There is no panel record found for this SN [{SNR}]. An empty list is returned: count {ppResult?.Count}";
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                return;
            }

            // Check if the result indicates a single PCB  
            if (ppResult.Count == 1)
            {
                string error = "PCB does not belong to Panel anymore, this configuration is not allowed! Part will not be booked.";
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, error, OkDialogType.Error);
                return;
            }

            // populate the view first (so to let the user see which PCB is Ok and which is not)
            Positions.AddRange(ppResult);

            // Loop through all PCBs and perform iTAC booking on OK part first
            foreach (var position in ppResult)
            {
                if (position.Status == (int)iTAC_Check_SN_RSLT_ENUM.PART_OK)
                {
#if RELEASE
                    StartBooking(position.SerialNumber);
#endif
                }
            }

            // Loop through all PCBs and show Interlock window for scrapped ones
            foreach (var position in ppResult)
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

            OnFocusRequested("SNR");
            SNR = string.Empty;
        }

        private bool StartBooking(string snr)
        {
            bool result = _api.UploadState(_session.Station, snr, new string[1] { "SERIAL_NUMBER_STATE" }, null, out string[] outArgs, out int code);
            Log.Information($"Upload SN {snr} for station {_session.Station} done, with Result {result}");
            if (result)
            {
                result = _api.GetSerialNumberInfo(_session.Station, snr, new string[3] { "PART_DESC", "SERIAL_NUMBER", "PART_NUMBER" }, out string[] outResults, out int codeInfo);
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

        public void OnLoaded()
        {
            OnFocusRequested("SNR");
        }

        public void OnUnloaded()
        {
           
        }

        #endregion

    }

    public enum iTAC_Check_SN_RSLT_ENUM
    {
        PART_OK    = 0,
        PART_FAIL  = 1,
        PART_SCRAP = 2,
        Unknown    = -1,
    }
}