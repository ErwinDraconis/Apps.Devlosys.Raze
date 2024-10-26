using Apps.Devlosys.Core.Mvvm;
using System.Windows.Controls;

namespace Apps.Devlosys.Modules.Main.Views.Dialogs
{
    /// <summary>
    /// Logique d'interaction pour LeakCheckDialog.xaml
    /// </summary>
    public partial class LeakCheckDialog : UserControl
    {
        public LeakCheckDialog()
        {
            InitializeComponent();

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
