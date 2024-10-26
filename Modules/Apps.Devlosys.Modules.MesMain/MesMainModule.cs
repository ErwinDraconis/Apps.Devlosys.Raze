using Apps.Devlosys.Core.Modularity;
using Apps.Devlosys.Modules.MesMain.Views;
using Prism.Ioc;
using Prism.Regions;

namespace Apps.Devlosys.Modules.MesMain
{
    public class MesMainModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;

        public MesMainModule(IContainerProvider container) : base(container)
        {
            _regionManager = container.Resolve<IRegionManager>();
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {

            containerRegistry.RegisterForNavigation<MesMainView>();
            containerRegistry.RegisterForNavigation<MesBookingView>();
        }
    }
}
