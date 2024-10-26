using Apps.Devlosys.Core.Handlers;
using Apps.Devlosys.Core.Helpers;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Modules.Authentification;
using Apps.Devlosys.Modules.Main;
using Apps.Devlosys.Modules.MesMain;
using Apps.Devlosys.Modules.Shared;
using Apps.Devlosys.Raze.Dialogs;
using Apps.Devlosys.Raze.Views;
using Apps.Devlosys.Resources.Properties;
using Apps.Devlosys.Services;
using Apps.Devlosys.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Serilog;
using Serilog.Events;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;

namespace Apps.Devlosys.Raze
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private const string ProcessName = @"Apps.Devlosys.Raze";

        private Modules.Shared.Views.SplashScreen splash;

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            splash = new Modules.Shared.Views.SplashScreen();
            splash.Show();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .WriteTo.File("file.log", rollingInterval: RollingInterval.Month)
                .CreateLogger();

            //LogBasicInfo();

            ProcessController.CheckSingleton(ProcessName, (IntPtr)Settings.Default.WindowHandle);

            ConfigureApplicationEventHandlers();

            base.OnStartup(e);
        }

        protected override void Initialize()
        {
            base.Initialize();

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            Settings.Default.PropertyChanged += (sender, eventArgs) => Settings.Default.Save();
        }

        protected override void OnInitialized()
        {
            splash.Close();

            base.OnInitialized();
        }

        protected override void ConfigureViewModelLocator()
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory(new ViewModelResolver(() => Container)
                .UseDefaultConfigure()
                .ResolveViewModelForView
            );
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var snackbarMessageQueue = new SnackbarMessageQueue(TimeSpan.FromSeconds(2));

            containerRegistry.RegisterInstance<ISnackbarMessageQueue>(snackbarMessageQueue);
            containerRegistry.RegisterSingleton<IIMSApi, IMSApi>();
            containerRegistry.RegisterSingleton<IContentDialogService, ContentDialogService>();

            containerRegistry.RegisterDialogWindow<DialogWindow>("IDialog");
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<SharedModule>();
            moduleCatalog.AddModule<AuthentificationModule>();
            moduleCatalog.AddModule<MainModule>();
            moduleCatalog.AddModule<MesMainModule>();
        }

        private void ConfigureApplicationEventHandlers()
        {
            ExceptionHandler handler = new();

            AppDomain.CurrentDomain.UnhandledException += handler.UnhandledExceptionHandler;
            Current.DispatcherUnhandledException += handler.DispatcherUnhandledExceptionHandler;
        }
    }
}
