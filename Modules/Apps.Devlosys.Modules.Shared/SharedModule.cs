using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Modularity;
using Apps.Devlosys.Modules.Shared.ViewModels.Dialogs;
using Apps.Devlosys.Modules.Shared.Views.Dialogs;
using Prism.Ioc;

namespace Apps.Devlosys.Modules.Shared
{
    public class SharedModule : ModuleBase
    {
        public SharedModule(IContainerProvider container) : base(container) { }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<OkDialog, OkDialogViewModel>(DialogNames.OkDialog);
            containerRegistry.RegisterDialog<YesNoDialog, YesNoDialogViewModel>(DialogNames.YesNoDialog);
        }
    }
}