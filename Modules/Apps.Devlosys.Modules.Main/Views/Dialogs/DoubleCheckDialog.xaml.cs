using Apps.Devlosys.Core.Mvvm;
using System.Windows.Controls;

namespace Apps.Devlosys.Modules.Main.Views.Dialogs
{
    /// <summary>
    /// Logique d'interaction pour DoubleCheckDialog.xaml
    /// </summary>
    public partial class DoubleCheckDialog : UserControl
    {
        public DoubleCheckDialog()
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
                case "Content":
                    Content.Focus();
                    Content.SelectAll();

                    break;
            }
        }
    }
}
