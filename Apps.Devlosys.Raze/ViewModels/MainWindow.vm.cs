using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Mvvm;
using MaterialDesignThemes.Wpf;
using Prism.Ioc;
using Prism.Regions;

namespace Apps.Devlosys.Raze.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IViewLoadedAndUnloadedAware, INotificable
    {
        #region Privates & Protecteds

        private readonly IRegionManager _regionManager;

        private string _title = "ViBPAD";

        #endregion

        #region Constructors

        public MainWindowViewModel(IContainerExtension container) : base(container)
        {
            _regionManager = Container.Resolve<IRegionManager>();
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
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.MesMainView);
        }

        public void OnUnloaded() { }

        #endregion
    }
}
