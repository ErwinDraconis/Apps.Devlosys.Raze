using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Apps.Devlosys.Infrastructure.Models
{
    public class TraitementModel : INotifyPropertyChanged
    {
        private TraitementEnum _traitement;
        private bool _isSelected;
        private bool _isEnable;

        public event PropertyChangedEventHandler PropertyChanged;

        public TraitementModel(TraitementEnum Traitement, bool IsSelected, bool IsEnable = true)
        {
            this.Traitement = Traitement;
            this.IsSelected = IsSelected;
            this.IsEnable = IsEnable;
        }

        public TraitementEnum Traitement
        {
            get => _traitement;
            set => SetProperty(ref _traitement, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsEnable
        {
            get { return _isEnable; }
            set { SetProperty(ref _isEnable, value); }
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            this.PropertyChanged?.Invoke(this, args);
        }
    }
}
