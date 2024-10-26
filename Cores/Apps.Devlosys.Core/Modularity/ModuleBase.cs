using Prism.Ioc;
using Prism.Modularity;

namespace Apps.Devlosys.Core.Modularity
{
    public abstract class ModuleBase : IModule
    {
        protected IContainerProvider Container { get; }

        protected ModuleBase(IContainerProvider container) => Container = container;

        public virtual void RegisterTypes(IContainerRegistry containerRegistry) { }

        public virtual void OnInitialized(IContainerProvider containerProvider) { }
    }
}
