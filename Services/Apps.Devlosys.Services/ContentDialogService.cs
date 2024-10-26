using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Prism.Ioc;
using Prism.Services.Dialogs;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Apps.Devlosys.Services
{
    public class ContentDialogService : IContentDialogService
    {
        private readonly IContainerProvider _container;

        public ContentDialogService(IContainerProvider container)
        {
            _container = container;
        }

        public async void ShowDialog<T, U>(Prism.Services.Dialogs.IDialogParameters parameters, Action<U> onCloseCallback = null) where T : UserControl
        {
            T view = _container.Resolve<T>();

            if (view is not FrameworkElement dialogContent)
            {
                throw new NullReferenceException("A dialog's content must be a FrameworkElement");
            }

            if (view.DataContext is DialogViewModelBase viewModel)
            {
                viewModel.OnDialogOpened(parameters);
            }

            object obj = await DialogHost.Show(dialogContent, "RootDialog", null, null);

            if (obj is U u)
            {
                onCloseCallback.Invoke(u);
            }
            else
            {
                if (obj is bool result && !result)
                {
                    return;
                }
                else
                {
                    throw new InvalidTypeParameterException();
                }
            }
        }

        public async Task<U> ShowDialogAsync<T, U>(IDialogParameters parameters) where T : UserControl
        {
            T view = _container.Resolve<T>();

            if (view is not FrameworkElement dialogContent)
            {
                throw new NullReferenceException("A dialog's content must be a FrameworkElement");
            }

            if (view.DataContext is DialogViewModelBase viewModel)
            {
                viewModel.OnDialogOpened(parameters);
            }

            object obj = await DialogHost.Show(dialogContent, "RootDialog", null, null);

            return obj is U u ? u : throw new InvalidTypeParameterException();
        }
    }
}
