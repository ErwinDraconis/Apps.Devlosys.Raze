using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Apps.Devlosys.Services.Interfaces
{
    public interface IContentDialogService
    {
        void ShowDialog<T, U>(Prism.Services.Dialogs.IDialogParameters parameters, Action<U> callback = null) where T : UserControl;

        Task<U> ShowDialogAsync<T, U>(Prism.Services.Dialogs.IDialogParameters parameters) where T : UserControl;
    }
}
