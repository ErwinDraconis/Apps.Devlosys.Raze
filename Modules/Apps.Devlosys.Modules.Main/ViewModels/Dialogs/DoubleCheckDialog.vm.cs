using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Resources.I18N;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Main.ViewModels.Dialogs
{
    public class DoubleCheckDialogViewModel : DialogViewModelBase, IViewLoadedAndUnloadedAware, IRequestFocus
    {
        #region Privates & Protecteds

        private readonly IDialogService _dialogService;

        private string _currentSNR;
        private string _snr;
        private string _content;
        private string _message;
        private bool _taskStart;

        private string _partNumber;
        private int _numberOfTest;

        public event EventHandler<FocusRequestedEventArgs> FocusRequested;
        public override event Action<IDialogResult> RequestClose;

        #endregion

        #region Constructors

        public DoubleCheckDialogViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            CheckLaserCommand = new DelegateCommand(CheckLaserCommandHandler);
            CheckContentCommand = new DelegateCommand(CheckContentCommandHandler);
        }

        #endregion

        #region Properties

        public string CurrentSNR
        {
            get => _currentSNR;
            set => SetProperty(ref _currentSNR, value);
        }

        public string SNR
        {
            get => _snr;
            set => SetProperty(ref _snr, value);
        }

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public bool TaskStart
        {
            get => _taskStart;
            set => SetProperty(ref _taskStart, value);
        }

        public bool CanClose { get; private set; }

        #endregion

        #region Commands

        public ICommand CheckLaserCommand { get; set; }

        public ICommand CheckContentCommand { get; set; }

        #endregion

        #region Command Handlers

        private void CheckLaserCommandHandler()
        {
            if (SNR != CurrentSNR)
            {
                _dialogService.ShowOkDialog("Error", string.Format(DoubleCheckResource.LaserNotMatchMessage, SNR), OkDialogType.Error);

                SNR = string.Empty;
                OnFocusRequested("SNR");
            }
            else
            {
                OnFocusRequested("Content");
            }
        }

        private void CheckContentCommandHandler()
        {
            if (!string.IsNullOrWhiteSpace(SNR))
            {
                if (SNR == CurrentSNR)
                {
                    string srnlast = SNR.Substring(19);
                    _partNumber = Regex.Replace(_partNumber, "^0*", "");

                    if (!Content.Contains("TG01_") && Content.Contains(_partNumber) && Content.Contains(srnlast))
                    {
                        CanClose = true;
                        RequestClose?.Invoke(new DialogResult(ButtonResult.Yes));
                    }
                    else
                    {
                        _numberOfTest--;

                        if (_numberOfTest == 0)
                        {
                            CanClose = true;
                            RequestClose?.Invoke(new DialogResult(ButtonResult.No));
                        }
                        else
                        {
                            Message = string.Format(DoubleCheckResource.TestNumberText, _numberOfTest);

                            _dialogService.ShowOkDialog("Error", string.Format(DoubleCheckResource.ContentNotMatchMessage, _numberOfTest), OkDialogType.Error);
                        }

                        Content = string.Empty;
                        OnFocusRequested("Content");
                    }
                }
                else
                {
                    _dialogService.ShowOkDialog("Error", string.Format(DoubleCheckResource.LaserNotMatchMessage, SNR), OkDialogType.Error);

                    SNR = string.Empty;
                    OnFocusRequested("SNR");
                }
            }
            else
            {
                _dialogService.ShowOkDialog("Information", DoubleCheckResource.LaserEmptyMessage, OkDialogType.Information);

                SNR = string.Empty;
                OnFocusRequested("SNR");
            }
        }

        #endregion

        #region Private Methods 


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

        public void OnUnloaded() { }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = DoubleCheckResource.HeaderText;

            CurrentSNR = parameters.GetValue<string>("snr");
            _partNumber = parameters.GetValue<string>("part");

            _numberOfTest = 3;

            Message = string.Format(DoubleCheckResource.TestNumberText, _numberOfTest);
            OnFocusRequested("SNR");
        }

        public override bool CanCloseDialog()
        {
            return CanClose;
        }

        #endregion
    }
}
