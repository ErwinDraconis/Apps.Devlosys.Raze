﻿using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Infrastructure.Params;
using Apps.Devlosys.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Threading;

namespace Apps.Devlosys.Modules.Main.ViewModels
{
    public class MainViewModel : ViewModelBase, IViewLoadedAndUnloadedAware, INotificable
    {
        #region Privates & Protecteds

        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogService _dialogService;
        private readonly IContentDialogService _contentDialogService;
        private readonly IIMSApi _api;

        private AppSession _session;

        private ObservableCollection<MenuButton> _menu_buttons;
        private MenuButton _selectedMenuButton;
        private string _date_display;
        private string _time_display;
        private bool _taskStart;

        private DispatcherTimer _timerDisplay;

        #endregion

        #region Constructors

        public MainViewModel(IContainerExtension container) : base(container)
        {
            _regionManager   = Container.Resolve<IRegionManager>();
            _eventAggregator = Container.Resolve<IEventAggregator>();
            _dialogService   = Container.Resolve<IDialogService>();
            _contentDialogService = Container.Resolve<IContentDialogService>();
            _api = Container.Resolve<IIMSApi>();

            LogoutCommand = new DelegateCommand(LogoutCommandHandler);

            InitTimer();
        }

        #endregion

        #region Properties

        public ISnackbarMessageQueue GlobalMessageQueue { get; set; }

        public ObservableCollection<MenuButton> MenuButtons
        {
            get => _menu_buttons;
            set => SetProperty(ref _menu_buttons, value);
        }

        public MenuButton SelectedMenuButton
        {
            get => _selectedMenuButton;
            set => SetProperty(ref _selectedMenuButton, value, () => _selectedMenuButton?.Action?.Invoke());
        }

        public string DateDisplay
        {
            get => _date_display;
            set => SetProperty(ref _date_display, value);
        }

        public string TimeDisplay
        {
            get => _time_display;
            set => SetProperty(ref _time_display, value);
        }

        public bool TaskStart
        {
            get => _taskStart;
            set => SetProperty(ref _taskStart, value);
        }

        #endregion

        #region Commands

        public ICommand LogoutCommand { get; set; }

        #endregion

        #region Command Handlers

        private void LogoutCommandHandler()
        {
            _api.ItacShutDown();
            _regionManager.RequestNavigate(RegionNames.ContentRegion, ViewNames.LoginView);
        }

        #endregion

        #region Private Methods

        private void InitTimer()
        {
            _timerDisplay = new() { Interval = TimeSpan.FromMilliseconds(1000) };
            _timerDisplay.Tick += new EventHandler((state, e) =>
            {
                DateDisplay = DateTime.Now.ToString("dd/MM/yyyy");
                TimeDisplay = DateTime.Now.ToString("HH:mm");
            });

            _timerDisplay.Start();
        }

        private void SetMenuItems()
        {
            _session = Container.Resolve<AppSession>();
            
            MenuButtons = new ObservableCollection<MenuButton>
            {
                new MenuButton()
                {
                    Title = "Panel",
                    Kind = "Panel",
                    IsEnable = true,
                    Action = new DelegateCommand(() => {
                        _regionManager.RequestNavigate(RegionNames.MainViewRegion,ViewNames.PanelCheckView);
                    }),
                },

                new MenuButton()
                {
                    Title = "PCB",
                    Kind = "BarcodeScan",
                    IsEnable = true,
                    Action = new DelegateCommand(() => {
                        _regionManager.RequestNavigate(RegionNames.MainViewRegion,ViewNames.TraitmentView);
                    }),
                },
                
                new MenuButton()
                {
                    Title = "Orders",
                    Kind = "FileCsvOutline",
                    IsEnable = _session.Level == 1,
                    Action = new DelegateCommand(() => {
                        //_dialogService.ShowDialog(DialogNames.BinDataDialog,new DialogParameters());
                        _regionManager.RequestNavigate(RegionNames.MainViewRegion,ViewNames.BinView);
                    }),
                },
                 
                new MenuButton()
                {
                     Title = "Leack",
                     Kind = "ClockCheckOutline",
                     IsEnable = true,
                     Action = new DelegateCommand(() => {
                         _dialogService.Show(DialogNames.LeakCheckDialog, () =>{ });

                         _eventAggregator.GetEvent<ConfigChangedEvent>().Publish();
                     }),
                },

                new MenuButton()
                {
                     Title = "CC",
                     Kind = "PackageVariant",
                     IsEnable = true,
                      Action = new DelegateCommand(() => {
                        _regionManager.RequestNavigate(RegionNames.MainViewRegion,ViewNames.ContenueContenant);
                    }),
                },
                new MenuButton()
                {
                     Title = "Settings",
                     Kind = "Cog",
                     IsEnable = _session.Level == 1,
                     Action = new DelegateCommand(() => {
                         _dialogService.ShowDialog(DialogNames.ConfigDialog, null, () =>
                         {
                             _session= Container.Resolve<AppSession>();
                            GlobalMessageQueue.Enqueue("Config saved.");
                         });

                         _eventAggregator.GetEvent<ConfigChangedEvent>().Publish();

                        //bool  result = await _contentDialogService.ShowDialogAsync<Views.Dialogs.ConfigDialog, bool>(null);
                        //if (result)
                        //{
                        //    _session= Container.Resolve<AppSession>();
                        //    GlobalMessageQueue.Enqueue("Config saved.");

                        //}
                        //_eventAggregator.GetEvent<ConfigChangedEvent>().Publish();
                     }),
                }
            };

            if (_session.DisplayOption == Infrastructure.DisplayOptionEnum.PCB)
                MenuButtons.RemoveAt(0);
            if(_session.DisplayOption == Infrastructure.DisplayOptionEnum.PANEL)
                MenuButtons.RemoveAt(1);

        }

        #endregion

        #region Public Methods 

        public void OnLoaded()
        {
            SetMenuItems();
            if (_session.DisplayOption == Infrastructure.DisplayOptionEnum.BOTH)
                _regionManager.RequestNavigate(RegionNames.MainViewRegion, ViewNames.PanelCheckView);
            else if (_session.DisplayOption == Infrastructure.DisplayOptionEnum.PCB)
                _regionManager.RequestNavigate(RegionNames.MainViewRegion, ViewNames.TraitmentView);
            else
                _regionManager.RequestNavigate(RegionNames.MainViewRegion, ViewNames.PanelCheckView);
        }

        public void OnUnloaded()
        {
            _regionManager.Regions[RegionNames.MainViewRegion].RemoveAll();
            _timerDisplay?.Stop();
        }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        #endregion
    }
}
