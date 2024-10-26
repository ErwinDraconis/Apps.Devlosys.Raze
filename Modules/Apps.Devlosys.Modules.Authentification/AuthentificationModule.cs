using Apps.Devlosys.Core.Modularity;
using Apps.Devlosys.Modules.Authentification.Views;
using Prism.Ioc;

namespace Apps.Devlosys.Modules.Authentification
{
    public class AuthentificationModule : ModuleBase
    {
        public AuthentificationModule(IContainerProvider container) : base(container) { }

        public override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<LoginView>();
        }
    }
}