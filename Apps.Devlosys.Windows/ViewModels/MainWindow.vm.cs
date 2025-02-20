using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Prism.Ioc;
using Prism.Regions;

namespace Apps.Devlosys.Windows.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IViewLoadedAndUnloadedAware, INotificable
    {
        #region Privates & Protecteds

        private readonly IRegionManager _regionManager;
        private readonly IIMSApi _api;

        private string _title = "ViBPAD v1.1.0";

        #endregion

        #region Constructors

        public MainWindowViewModel(IContainerExtension container) : base(container)
        {
            _regionManager = Container.Resolve<IRegionManager>();
            _api = Container.Resolve<IIMSApi>();
        }

        #endregion

        #region Properties

        public ISnackbarMessageQueue GlobalMessageQueue { get; set; }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        #endregion

        #region Public Methods

        public void OnLoaded()
        {
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.LoginView);
        }

        public void OnUnloaded() { }

        #endregion
    }
}
