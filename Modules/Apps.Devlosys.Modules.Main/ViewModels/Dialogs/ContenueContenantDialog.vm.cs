using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Resources.I18N;
using Apps.Devlosys.Services.Interfaces;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Serilog;
using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Main.ViewModels.Dialogs
{
    public class ContenueContenantDialogViewModel : DialogViewModelBase, IViewLoadedAndUnloadedAware, IRequestFocus
    {
        #region Privates & Protecteds

        private readonly IDialogService _dialogService;
        private readonly IIMSApi _api;

        private const double minutes = 1440.0;

        private string _snr;
        private string _galia;
        private string _message;
        private string _state;
        private bool _taskStart;

        public event EventHandler<FocusRequestedEventArgs> FocusRequested;
        public override event Action<IDialogResult> RequestClose;

        #endregion

        #region Constructors

        public ContenueContenantDialogViewModel(IContainerExtension Container)
        {
            _dialogService = Container.Resolve<IDialogService>();
            _api = Container.Resolve<IIMSApi>();

            State = "S";

            CheckGaliaCommand = new DelegateCommand(CheckGaliaCommandHandler).ObservesCanExecute(() => CanCheckGalia);
            CheckSnrCommand = new DelegateCommand(CheckSnrCommandHandler).ObservesCanExecute(() => CanCheckSNR);
        }

        #endregion

        #region Properties

        public string Galia
        {
            get => _galia;
            set => SetProperty(ref _galia, value);
        }

        public string SNR
        {
            get => _snr;
            set => SetProperty(ref _snr, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public string State
        {
            get => _state;
            set => SetProperty(ref _state, value);
        }

        public bool TaskStart
        {
            get => _taskStart;
            set => SetProperty(ref _taskStart, value);
        }

        #endregion

        #region Observables

        public bool CanCheckGalia => !string.IsNullOrWhiteSpace(Galia);

        public bool CanCheckSNR => !string.IsNullOrWhiteSpace(SNR);

        #endregion

        #region Commands

        public ICommand CheckGaliaCommand { get; set; }

        public ICommand CheckSnrCommand { get; set; }

        #endregion

        #region Command Handlers

        private void CheckGaliaCommandHandler()
        {
            State = "S";
            Message = string.Empty;

            if (string.IsNullOrWhiteSpace(SNR))
            {
                SNR = string.Empty;
                OnFocusRequested("SNR");
            }
            else
            {
                StartCompare();
            }
        }

        private void CheckSnrCommandHandler()
        {
            State = "S";
            Message = string.Empty;

            if (string.IsNullOrWhiteSpace(Galia))
            {
                Galia = string.Empty;
                OnFocusRequested("Galia");
            }
            else
            {
                StartCompare();
            }
        }

        #endregion

        #region Private Methodes

        /*private async void StartCompare()
        {
            try
            {
                TaskStart = true;

                await Task.Run(() =>
                {
                    string galia = Regex.Replace(Galia.Substring(1), "^0*", "");
                    string part = SNR.Between("_", "_");

                    string galiyaFromFile = GetGaliyaFromConfig(part);
                    string galiaTocompare = Regex.Replace(galiyaFromFile, "^0*", "");

                    if (galia == galiaTocompare)
                    {
                        State = "T";
                        Message = ContenueContenantResource.GaliaOkMessage;
                    }
                    else
                    {
                        State = "F";
                        Message = ContenueContenantResource.GaliaNotOkMessage;
                    }

                });

                TaskStart = false;
            }
            catch (Exception ex)
            {
                TaskStart = false;

                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, string.Format(DialogsResource.ConnectionException, (ex.InnerException ?? ex).Message), OkDialogType.Error);
                Log.Error(ex, ex.Message);
            }

            Galia = string.Empty;
            SNR = string.Empty;

            OnFocusRequested("Galia");
        }*/

        private async Task StartCompare()
        {
            try
            {
                TaskStart = true;

                await Task.Run(() => PerformGaliaComparison()).ConfigureAwait(false);

                TaskStart = false;
            }
            catch (Exception ex)
            {
                TaskStart = false;
                _ = Application.Current.Dispatcher.BeginInvoke(() =>
                {
                    _dialogService.ShowOkDialog( DialogsResource.GlobalErrorTitle, ex.Message,  OkDialogType.Error);
                });
                
                Log.Error(ex, ex.Message);
            }
            finally
            {
                Galia = string.Empty;
                SNR   = string.Empty;

                OnFocusRequested("Galia");
            }
        }

        private void PerformGaliaComparison()
        {
            if (string.IsNullOrEmpty(SNR) || !SNR.Contains("_"))
            {
                State   = "F";
                Message = "Invalid SNR input. Please provide the correct format.";
                return;
            }

            string part = SNR.SafeExtractBetween("_", "_");

            if (string.IsNullOrEmpty(part))
            {
                State   = "F";
                Message = "Invalid SNR input. Cannot extract required part.";
                return;
            }

            // Proceed with the comparison logic
            string galia = Galia.TrimStart('0');
            string galiyaFromFile = GetGaliyaFromConfig(part);
            string galiaToCompare = galiyaFromFile.TrimStart('0');

            if (galia == galiaToCompare)
            {
                State   = "T";
                Message = ContenueContenantResource.GaliaOkMessage;
            }
            else
            {
                State   = "F";
                Message = ContenueContenantResource.GaliaNotOkMessage + $". Expected [{galia}] readed from .bin file [{galiyaFromFile}]";
            }
        }


        public string GetGaliyaFromConfig(string snr)
        {
            string data = "";
            StreamReader file = new(AppDomain.CurrentDomain.BaseDirectory + "\\data\\bin.txt");

            string line;
            while ((line = file.ReadLine()) != null)
            {
                string[] col = line.Split('|');
                if (col[0] == snr)
                {
                    data = col[5].ToString();
                    break;
                }
            }
            return data;
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
            OnFocusRequested("Galia");
        }

        public void OnUnloaded() { }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = ContenueContenantResource.HeaderText;

            OnFocusRequested("Galia");
        }

        #endregion
    }
}
