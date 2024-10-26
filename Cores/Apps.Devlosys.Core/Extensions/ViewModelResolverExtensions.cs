using Apps.Devlosys.Infrastructure;
using MaterialDesignThemes.Wpf;
using Prism.Ioc;
using System;
using System.Linq;
using System.Windows;

namespace Apps.Devlosys.Core.Mvvm
{
    public static class ViewModelResolverExtensions
    {
        public static IViewModelResolver UseDefaultConfigure(this IViewModelResolver @this)
        {
            return @this
                .IfInheritsFrom<ViewModelBase>((view, viewModel) =>
                {
                    viewModel.Dispatcher = view.Dispatcher;
                })
                .IfInheritsFrom<IViewLoadedAndUnloadedAware>((view, viewModel) =>
                {
                    view.Loaded += (sender, e) => viewModel.OnLoaded();
                    view.Unloaded += (sender, e) => viewModel.OnUnloaded();
                })
                .IfInheritsFrom(typeof(IViewLoadedAndUnloadedAware<>), (view, viewModel, interfaceInstance) =>
                {
                    Type viewType = view.GetType();

                    if (interfaceInstance.GenericArguments.Single() != viewType)
                    {
                        throw new InvalidOperationException();
                    }

                    Action<object> onLoadedMethod = interfaceInstance.GetMethod<Action<object>>("OnLoaded", viewType);
                    Action<object> onUnloadedMethod = interfaceInstance.GetMethod<Action<object>>("OnUnloaded", viewType);

                    view.Loaded += (sender, args) => onLoadedMethod(sender);
                    view.Unloaded += (sender, args) => onUnloadedMethod(sender);
                })
                .IfInheritsFrom<INotificable>((view, viewModel, container) =>
                {
                    viewModel.GlobalMessageQueue = container.Resolve<ISnackbarMessageQueue>();
                })
                .IfInheritsFrom<IRequestFocus>((view, viewModel, container) =>
                {
                    
                });
        }

        public static IViewModelResolver IfInheritsFrom<TViewModel>(this IViewModelResolver @this, Action<FrameworkElement, TViewModel> configuration)
        {
            Guards.ThrowIfNull(@this, configuration);

            return @this.IfInheritsFrom<FrameworkElement, TViewModel>((view, viewModel, container) => configuration(view, viewModel));
        }

        public static IViewModelResolver IfInheritsFrom<TViewModel>(this IViewModelResolver @this, Action<FrameworkElement, TViewModel, IContainerProvider> configuration)
        {
            Guards.ThrowIfNull(@this, configuration);

            return @this.IfInheritsFrom(configuration);
        }

        public static IViewModelResolver IfInheritsFrom(this IViewModelResolver @this, Type genericInterfaceType, Action<FrameworkElement, object, IGenericInterface> configuration)
        {
            Guards.ThrowIfNull(@this, configuration);

            return @this.IfInheritsFrom<FrameworkElement>(genericInterfaceType, (view, viewModel, interfaceInstance, container) => configuration(view, viewModel, interfaceInstance));
        }
    }
}
