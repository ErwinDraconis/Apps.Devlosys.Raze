using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using System;
using System.Windows.Threading;

namespace Apps.Devlosys.Core.Mvvm
{
    public abstract class ViewModelBase : BindableBase, INavigationAware, IDestructible
    {
        protected ViewModelBase(IContainerExtension container)
        {
            Container = container;
            EventAggregator = Container.Resolve<IEventAggregator>();
        }

        public Dispatcher Dispatcher { get; set; }

        protected IContainerExtension Container { get; }

        protected IEventAggregator EventAggregator { get; }

        //public User AppUser => Container.Resolve<User>(nameof(AppUser));

        //public Parametre AppParam => Container.Resolve<Parametre>();

        //public Caisse AppCaisse => Container.Resolve<Caisse>();

        //public CaisseStatus AppCaisseStatus => Container.Resolve<CaisseStatus>();

        //public AppConfiguration AppConfig => Container.Resolve<AppConfiguration>("app_configuration");

        public virtual void Destroy()
        {

        }

        protected virtual void InvokeOnUIThread(Action action) => Dispatcher.Invoke(action);

        public virtual bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public virtual void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }

        public virtual void OnNavigatedTo(NavigationContext navigationContext)
        {

        }
    }
}
