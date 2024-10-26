using Apps.Devlosys.Core.Mvvm;
using Prism.Services.Dialogs;

namespace Apps.Devlosys.Modules.Shared.ViewModels.Dialogs
{
    public class OkDialogViewModel : DialogViewModelBase
    {
        #region Privates & Protecteds

        private string _message;
        private OkDialogType _type;

        #endregion

        #region Properties

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public OkDialogType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        #endregion

        #region Public Methods 

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            Title = parameters.GetValue<string>("title");
            Message = parameters.GetValue<string>("message");
            Type = parameters.GetValue<OkDialogType>("type");
        }

        #endregion
    }
}
