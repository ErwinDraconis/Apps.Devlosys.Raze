using System.ComponentModel;

namespace Apps.Devlosys.Infrastructure.Models
{
    public class PanelPositions : INotifyPropertyChanged
    {
        private int    _positionNumber;
        private string _serialNumber;
        private int    _status;
        private int    _displayStatus;

        public int PositionNumber
        {
            get => _positionNumber;
            set
            {
                if (_positionNumber != value)
                {
                    _positionNumber = value;
                    OnPropertyChanged(nameof(PositionNumber));
                }
            }
        }

        public string SerialNumber
        {
            get => _serialNumber;
            set
            {
                if (_serialNumber != value)
                {
                    _serialNumber = value;
                    OnPropertyChanged(nameof(SerialNumber));
                }
            }
        }

        public int Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public int DisplayStatus
        {
            get => _displayStatus;
            set
            {
                if (_displayStatus != value)
                {
                    _displayStatus = value;
                    OnPropertyChanged(nameof(DisplayStatus));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
