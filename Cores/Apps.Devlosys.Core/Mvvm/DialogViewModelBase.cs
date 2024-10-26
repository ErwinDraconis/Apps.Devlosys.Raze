using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Windows.Input;

namespace Apps.Devlosys.Core.Mvvm
{
    public abstract class DialogViewModelBase : BindableBase, IDialogAware
    {
        private ICommand _closeDialogCommand;
        private string _title;

        public virtual event Action<IDialogResult> RequestClose;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public ICommand CloseDialogCommand => _closeDialogCommand ??= new DelegateCommand<string>(CloseDialog);

        public virtual bool CanCloseDialog() => true;

        public virtual void CloseDialog(string param = "No")
        {
            var result = ButtonResult.None;

            if (param.ToLower() == "ok")
            {
                result = ButtonResult.OK;
            }
            else if (param.ToLower() == "yes")
            {
                result = ButtonResult.Yes;
            }
            else if (param.ToLower() == "no")
            {
                result = ButtonResult.No;
            }

            RequestClose?.Invoke(new DialogResult(result));
        }

        public virtual void OnDialogClosed() { }

        public virtual void OnDialogOpened(IDialogParameters parameters) { }
    }
}
