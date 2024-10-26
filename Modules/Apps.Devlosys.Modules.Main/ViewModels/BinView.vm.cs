using Apps.Devlosys.Core;
using Apps.Devlosys.Core.Events;
using Apps.Devlosys.Core.Mvvm;
using Apps.Devlosys.Infrastructure.Models;
using Apps.Devlosys.Resources.I18N;
using Apps.Devlosys.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Apps.Devlosys.Modules.Main.ViewModels
{
    public class BinViewModel : ViewModelBase, IViewLoadedAndUnloadedAware, INotificable, IRequestFocus
    {
        #region Privates & Protecteds

        private readonly IDialogService _dialogService;
        private readonly IContentDialogService _contentDialogService;

        private ObservableCollection<BinData> _dataList;
        private BinData _data;
        private string _search;
        private bool _taskStart;

        private IList<BinData> _allData;

        public event EventHandler<FocusRequestedEventArgs> FocusRequested;

        #endregion

        #region Constructors

        public BinViewModel(IContainerExtension container) : base(container)
        {
            _dialogService = container.Resolve<IDialogService>();
            _contentDialogService = container.Resolve<IContentDialogService>();

            AddNewBinCommand = new DelegateCommand(AddNewBinCommandHandler);
            RemoveBinCommand = new DelegateCommand<BinData>(RemoveBinCommandHandler);
            EditBinCommand = new DelegateCommand<BinData>(EditBinCommandHandler);
            SyncDataCommand = new DelegateCommand(SyncDataCommandHandler);
        }

        #endregion

        #region Properties

        public ISnackbarMessageQueue GlobalMessageQueue { get; set; }

        public ObservableCollection<BinData> DataList
        {
            get => _dataList;
            set => SetProperty(ref _dataList, value);
        }

        public BinData Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        public string Search
        {
            get => _search;
            set => SetProperty(ref _search, value, FilterData);
        }

        public bool TaskStart
        {
            get => _taskStart;
            set => SetProperty(ref _taskStart, value);
        }

        public bool CanClose { get; private set; }

        #endregion

        #region Commands

        public ICommand AddNewBinCommand { get; set; }

        public ICommand RemoveBinCommand { get; set; }

        public ICommand EditBinCommand { get; set; }

        public ICommand SyncDataCommand { get; set; }

        #endregion

        #region Command Handlers

        private void AddNewBinCommandHandler()
        {
            _contentDialogService.ShowDialog<Views.Dialogs.AddBinDialog, BinData>(null, param =>
            {
                _allData.Add(param);

                DataList.Clear();
                DataList.AddRange(_allData);
            });
        }

        private void RemoveBinCommandHandler(BinData data)
        {
            _allData.Remove(data);
            DataList.Remove(data);
        }

        private void EditBinCommandHandler(BinData data)
        {
            DialogParameters param = new() { { "data", data } };

            _contentDialogService.ShowDialog<Views.Dialogs.AddBinDialog, BinData>(param, result =>
            {
                _allData[_allData.IndexOf(data)] = result;

                DataList.Clear();
                DataList.AddRange(_allData);
            });
        }

        private void SyncDataCommandHandler()
        {
            try
            {
                string dataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DATA");
                string filePath = Path.Combine(dataPath, "bin.txt");

                if (!Directory.Exists(dataPath))
                {
                    _ = Directory.CreateDirectory(dataPath);
                }

                StreamWriter writer = new(filePath, false);
                foreach (BinData item in _allData)
                {
                    string v1 = item.Identification;
                    string v2 = item.PartNumberSFG;
                    string v3 = item.PartDescription;
                    string v4 = item.BinRef;
                    string v5 = item.HardwareRef;
                    string v6 = item.FinGood;
                    string v7 = item.Shipping;
                    string v8 = "30";

                    writer.WriteLine($"{v1}|{v2}|{v3}|{v4}|{v5}|{v6}|{v7}|{v8}");
                }
                writer.Close();

                GlobalMessageQueue.Enqueue(BinResource.DataSavedMessage);
            }
            catch (Exception ex)
            {
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorTitle, (ex.InnerException ?? ex).Message, OkDialogType.Error);
            }
        }

        #endregion

        #region Private Methods 

        private async Task GetDataFromFileAsync()
        {
            try
            {
                TaskStart = true;

                await Task.Run(() => GetDataFromFile());
            }
            catch (Exception ex)
            {
                _dialogService.ShowOkDialog(DialogsResource.GlobalErrorMessage, (ex.InnerException ?? ex).Message, OkDialogType.Error);
            }
            finally
            {
                TaskStart = false;
            }
        }

        private void GetDataFromFile()
        {
            _allData = new List<BinData>();
            Data = new BinData();

            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + @"\DATA\bin.txt");
            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);

                foreach (string line in lines)
                {
                    string[] col = line.Split('|');
                    if (col.Length > 7)
                    {
                        BinData data = new()
                        {
                            Identification = col[0],
                            PartNumberSFG = col[1],
                            PartDescription = col[2],
                            BinRef = col[3],
                            HardwareRef = col[4],
                            FinGood = col[5],
                            Shipping = col[6],
                            Quantity = col[7],
                        };

                        _allData.Add(data);
                    }
                }
            }

            DataList = new ObservableCollection<BinData>(_allData);
        }

        private void FilterData()
        {
            TaskStart = true;

            if (!string.IsNullOrWhiteSpace(Search))
            {
                List<BinData> list = _allData
                        .Where(a =>
                            (a.Identification != null && a.Identification.ToUpper().Contains(Search.ToUpper())) ||
                            (a.PartNumberSFG != null && a.PartNumberSFG.ToUpper().Contains(Search.ToUpper())) ||
                            (a.PartDescription != null && a.PartDescription.ToUpper().Contains(Search.ToUpper())) ||
                            (a.BinRef != null && a.BinRef.ToUpper().Contains(Search.ToUpper())) ||
                            (a.HardwareRef != null && a.HardwareRef.ToUpper().Contains(Search.ToUpper())) ||
                            (a.FinGood != null && a.FinGood.ToUpper().Contains(Search.ToUpper())) ||
                            (a.Shipping != null && a.Shipping.ToUpper().Contains(Search.ToUpper())) ||
                            (a.Quantity != null && a.Quantity.ToUpper().Contains(Search.ToUpper())))
                        .ToList();

                DataList.Clear();
                DataList.AddRange(list);
            }
            else
            {
                DataList.Clear();
                DataList.AddRange(_allData);
            }

            TaskStart = false;
        }

        private bool HasChange()
        {
            try
            {
                TaskStart = true;

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"DATA\bin.txt");
                if (!File.Exists(filePath))
                {
                    return false;
                }

                string oldFile = string.Empty;
                string newFile = string.Empty;

                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    oldFile += line;
                }

                foreach (BinData item in _allData)
                {
                    string v1 = item.Identification;
                    string v2 = item.PartNumberSFG;
                    string v3 = item.PartDescription;
                    string v4 = item.BinRef;
                    string v5 = item.HardwareRef;
                    string v6 = item.FinGood;
                    string v7 = item.Shipping;
                    string v8 = "30";

                    newFile += $"{v1}|{v2}|{v3}|{v4}|{v5}|{v6}|{v7}|{v8}";
                }

                return oldFile != newFile;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                TaskStart = false;
            }
        }

        #endregion

        #region Protected Methodes

        protected virtual void OnFocusRequested(string propertyName)
        {
            FocusRequested?.Invoke(this, new FocusRequestedEventArgs(propertyName));
        }

        #endregion

        #region Public Methods 

        public async void OnLoaded()
        {
            OnFocusRequested("Search");
            await GetDataFromFileAsync();
        }

        public void OnUnloaded() { }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            if (HasChange())
            {
                _dialogService.ShowConfirmation(DialogsResource.GlobalQuestionTitle, BinResource.DataChangerMessage, () =>
                {
                    SyncDataCommandHandler();
                });
            }
        }

        public override void OnNavigatedTo(NavigationContext navigationContext) { }

        public override bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        #endregion
    }
}
