using Apps.Devlosys.Core.Mvvm;
using System.Windows;
using System.Windows.Controls;

namespace Apps.Devlosys.Modules.Main.Views
{
    /// <summary>
    /// Interaction logic for PanelCheckView.xaml
    /// </summary>
    public partial class PanelCheckView : UserControl
    {
        public PanelCheckView()
        {
            InitializeComponent();

            Loaded += PanelCheckView_Loaded;
        }

        private void PanelCheckView_Loaded(object sender, System.Windows.RoutedEventArgs e)
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
