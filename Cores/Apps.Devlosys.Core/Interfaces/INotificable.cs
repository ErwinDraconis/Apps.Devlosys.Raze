using MaterialDesignThemes.Wpf;

namespace Apps.Devlosys.Core.Mvvm
{
    public interface INotificable
    {
        ISnackbarMessageQueue GlobalMessageQueue { get; set; }
    }
}
