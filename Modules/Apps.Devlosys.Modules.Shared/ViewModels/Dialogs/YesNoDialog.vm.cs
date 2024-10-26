using Apps.Devlosys.Core.Mvvm;
using Prism.Services.Dialogs;

namespace Apps.Devlosys.Modules.Shared.ViewModels.Dialogs
{
    public class YesNoDialogViewModel : DialogViewModelBase
    {
        #region Privates & Protecteds

        private string _message;

        #endregion

        #region Properties

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        #endregion

        #region Public Methods 

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>("title");
            Message = parameters.GetValue<string>("message");
        }

        #endregion
    }
}
