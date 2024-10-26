using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Resources.I18N;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Services.Dialogs;
using System;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Main.ViewModels.Dialogs
{
    public class AddBinDialogViewModel : DialogViewModelBase, IViewLoadedAndUnloadedAware, IRequestFocus
    {
        #region Privates & Protecteds

        private string _identification;
        private string _partNumberSFG;
        private string _partDescription;
        private string _binRef;
        private string _hardwareRef;
        private string _finGood;
        private string _shipping;
        private string _quantity;

        public event EventHandler<FocusRequestedEventArgs> FocusRequested;
        public override event Action<IDialogResult> RequestClose;

        #endregion

        #region Constructors

        public AddBinDialogViewModel(IDialogService dialogService)
        {
            SaveCommand = new DelegateCommand(SaveCommandHandler).ObservesCanExecute(() => CanSave);
            ExitCommand = new DelegateCommand(ExitCommandHandler);
        }

        #endregion

        #region Properties

        public string Identification
        {
            get => _identification;
            set => SetProperty(ref _identification, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string PartNumberSFG
        {
            get => _partNumberSFG;
            set => SetProperty(ref _partNumberSFG, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string PartDescription
        {
            get => _partDescription;
            set => SetProperty(ref _partDescription, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string BinRef
        {
            get => _binRef;
            set => SetProperty(ref _binRef, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string HardwareRef
        {
            get => _hardwareRef;
            set => SetProperty(ref _hardwareRef, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string FinGood
        {
            get => _finGood;
            set => SetProperty(ref _finGood, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string Shipping
        {
            get => _shipping;
            set => SetProperty(ref _shipping, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        public string Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value, () => RaisePropertyChanged(nameof(CanSave)));
        }

        #endregion

        #region Observabels

        public bool CanSave => !string.IsNullOrEmpty(Identification) ||
            !string.IsNullOrEmpty(PartNumberSFG) ||
            !string.IsNullOrEmpty(_partDescription) ||
            !string.IsNullOrEmpty(BinRef) ||
            !string.IsNullOrEmpty(HardwareRef) ||
            !string.IsNullOrEmpty(FinGood) ||
            !string.IsNullOrEmpty(Shipping);

        #endregion

        #region Commands

        public ICommand SaveCommand { get; set; }

        public ICommand ExitCommand { get; set; }

        #endregion

        #region Command Handlers

        private void SaveCommandHandler()
        {
            BinData bin = new()
            {
                Identification = Identification,
                PartNumberSFG = PartNumberSFG,
                PartDescription = PartDescription,
                BinRef = BinRef,
                HardwareRef = HardwareRef,
                FinGood = FinGood,
                Shipping = Shipping,
                Quantity = Quantity,
            };

            DialogHost.CloseDialogCommand.Invoke(bin);
        }

        private void ExitCommandHandler()
        {
            DialogHost.CloseDialogCommand.Invoke(false);
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

        public void OnLoaded() { }

        public void OnUnloaded() { }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = DoubleCheckResource.HeaderText;

            if (parameters != null && parameters.ContainsKey("data"))
            {
                BinData data = parameters.GetValue<BinData>("data");

                Identification = data.Identification;
                PartNumberSFG = data.PartNumberSFG;
                PartDescription = data.PartDescription;
                BinRef = data.BinRef;
                HardwareRef = data.HardwareRef;
                FinGood = data.FinGood;
                Shipping = data.Shipping;
                Quantity = data.Quantity;
            }

            OnFocusRequested("Identification");
        }

        #endregion
    }
}
