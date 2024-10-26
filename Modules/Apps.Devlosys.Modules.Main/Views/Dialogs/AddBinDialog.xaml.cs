using Apps.Devlosys.Core.Mvvm;
using System.Windows;
using System.Windows.Controls;

namespace Apps.Devlosys.Modules.Main.Views.Dialogs
{
    /// <summary>
    /// Logique d'interaction pour AddBinDialog.xaml
    /// </summary>
    public partial class AddBinDialog : UserControl
    {
        public AddBinDialog()
        {
            InitializeComponent();

            if (DataContext is IRequestFocus vm)
            {
                vm.FocusRequested += OnFocusRequested;
            }
        }

        private void AddBinDialog_Loaded(object sender, RoutedEventArgs e)
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
                case "Identification":
                    Identification.Focus();
                    Identification.SelectAll();

                    break;
            }
        }
    }
}
