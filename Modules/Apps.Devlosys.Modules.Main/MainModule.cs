using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Modularity;
using Apps.Devlosys.Modules.Main.ViewModels.Dialogs;
using Apps.Devlosys.Modules.Main.Views;
using Apps.Devlosys.Modules.Main.Views.Dialogs;
using Prism.Ioc;
using Prism.Regions;

namespace Apps.Devlosys.Modules.Main
{
    public class MainModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;

        public MainModule(IContainerProvider container) : base(container)
        {
            _regionManager = container.Resolve<IRegionManager>();
        }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {

            containerRegistry.RegisterForNavigation<MainView>();
            containerRegistry.RegisterForNavigation<TraitmentView>();
            containerRegistry.RegisterForNavigation<BinView>();
            containerRegistry.RegisterForNavigation<PanelCheckView>(); 
            containerRegistry.RegisterForNavigation<ContenueContenant>();

            //_regionManager.RegisterViewWithRegion<TraitmentView>(RegionNames.MainViewRegion);

            containerRegistry.RegisterDialog<DoubleCheckDialog, DoubleCheckDialogViewModel>(DialogNames.DoubleCheckDialog);
            containerRegistry.RegisterDialog<QualityValidationDialog, QualityValidationDialogViewModel>(DialogNames.QualityValidationDialog);
            containerRegistry.RegisterDialog<LeakCheckDialog, LeakCheckDialogViewModel>(DialogNames.LeakCheckDialog);
            containerRegistry.RegisterDialog<ContenueContenantDialog, ContenueContenantDialogViewModel>(DialogNames.ContenueContenantDialog);
            containerRegistry.RegisterDialog<BookingDialog, BookingDialogViewModel>(DialogNames.BookingDialog);
            containerRegistry.RegisterDialog<AddBinDialog, AddBinDialogViewModel>(DialogNames.AddBinDataDialog);
            containerRegistry.RegisterDialog<ConfigDialog, ConfigDialogViewModel>(DialogNames.ConfigDialog);
            containerRegistry.RegisterDialog<UnterlockFailDialog, UnterlockFailDialogViewModel>(DialogNames.UnterlockFailDialog);
        }
    }
}