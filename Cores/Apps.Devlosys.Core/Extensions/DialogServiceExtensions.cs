using Apps.Devlosys.Core;
using System;

namespace Prism.Services.Dialogs
{
    public static class DialogServiceExtensions
    {
        public static ButtonResult ShowDialog(this IDialogService dialog, string name, IDialogParameters parameters)
        {
            ButtonResult _result = ButtonResult.None;

            dialog.ShowDialog(name, parameters, result =>
            {
                _result = result.Result;
            }, "IDialog");

            return _result;
        }

        public static void Show(this IDialogService dialog, string name, Action OnConfirm = null)
        {
            dialog.Show(name, null, result =>
            {
                if (result.Result is ButtonResult.Yes or ButtonResult.OK)
                {
                    OnConfirm?.Invoke();
                }
            }, "IDialog");
        }

        public static void ShowDialog(this IDialogService dialog, string name, IDialogParameters parameters, Action OnConfirm = null, Action OnCancel = null)
        {
            dialog.ShowDialog(name, parameters, result =>
            {
                if (result.Result == ButtonResult.Yes)
                {
                    OnConfirm?.Invoke();
                }
                else if (result.Result == ButtonResult.No)
                {
                    OnCancel?.Invoke();
                }
            }, "IDialog");
        }

        public static void ShowDialog(this IDialogService dialog, string name, IDialogParameters parameters, Action<IDialogParameters> OnConfirm, Action<IDialogParameters> OnCancel = null)
        {
            dialog.ShowDialog(name, parameters, result =>
            {
                if (result.Result == ButtonResult.Yes)
                {
                    OnConfirm?.Invoke(result.Parameters);
                }
                else if (result.Result == ButtonResult.No)
                {
                    OnCancel?.Invoke(result.Parameters);
                }
            }, "IDialog");
        }

        public static void ShowOkDialog(this IDialogService dialog, string title, string message, OkDialogType type = OkDialogType.Information)
        {
            DialogParameters param = new()
            {
                { "title", title },
                { "message", message },
                { "type", type }
            };

            dialog.ShowDialog(DialogNames.OkDialog, param, null, "IDialog");
        }

        public static void ShowConfirmation(this IDialogService dialog, string title, string message, Action OnConfirm, Action OnCancel = null)
        {
            dialog.ShowDialog(DialogNames.YesNoDialog, new DialogParameters($"title={title}&message={message}"), result =>
            {
                if (result.Result == ButtonResult.Yes)
                {
                    OnConfirm.Invoke();
                }
                else if (result.Result == ButtonResult.No)
                {
                    OnCancel?.Invoke();
                }
            }, "IDialog");
        }

        public static ButtonResult ShowConfirmation(this IDialogService dialog, string title, string message)
        {
            ButtonResult _result = ButtonResult.None;
            dialog.ShowDialog(DialogNames.YesNoDialog, new DialogParameters($"title={title}&message={message}"), result =>
            {
                _result = result.Result;
            }, "IDialog");

            return _result;
        }
    }

    public enum OkDialogType { Error, Warning, Information, }
}
