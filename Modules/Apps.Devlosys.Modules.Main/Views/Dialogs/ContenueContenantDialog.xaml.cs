using Apps.Devlosys.Core.Mvvm;
using System.Windows.Controls;

namespace Apps.Devlosys.Modules.Main.Views.Dialogs
{
    /// <summary>
    /// Logique d'interaction pour ContenueContenantDialog.xaml
    /// </summary>
    public partial class ContenueContenantDialog : UserControl
    {
        public ContenueContenantDialog()
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
                case "Galia":
                    Galia.Focus();
                    Galia.SelectAll();

                    break;
                case "SNR":
                    SNR.Focus();
                    SNR.SelectAll();

                    break;
            }
        }
    }
}
