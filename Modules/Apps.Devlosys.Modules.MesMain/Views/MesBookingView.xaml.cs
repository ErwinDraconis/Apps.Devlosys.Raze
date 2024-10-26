using Apps.Devlosys.Core.Mvvm;
using System.Windows;
using System.Windows.Controls;

namespace Apps.Devlosys.Modules.MesMain.Views
{
    /// <summary>
    /// Logique d'interaction pour MesBookingView.xaml
    /// </summary>
    public partial class MesBookingView : UserControl
    {
        public MesBookingView()
        {
            InitializeComponent();

            Loaded += MesBookingView_Loaded;
        }

        private void MesBookingView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is IRequestFocus vm)
            {
                vm.FocusRequested += OnFocusRequested;
            }
        }

        private void OnFocusRequested(object sender, Core.Events.FocusRequestedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SNR":
                    SNR.Focus();
                    SNR.SelectAll();

                    break;
            }
        }
    }
}
