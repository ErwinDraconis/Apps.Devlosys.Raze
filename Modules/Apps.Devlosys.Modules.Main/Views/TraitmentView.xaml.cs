using Apps.Devlosys.Core.Mvvm;
using System.Windows;
using System.Windows.Controls;

namespace Apps.Devlosys.Modules.Main.Views
{
    /// <summary>
    /// Logique d'interaction pour TraitmentView.xaml
    /// </summary>
    public partial class TraitmentView : UserControl
    {
        public TraitmentView()
        {
            InitializeComponent();

            Loaded += TraitmentView_Loaded;
        }

        private void TraitmentView_Loaded(object sender, RoutedEventArgs e)
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
