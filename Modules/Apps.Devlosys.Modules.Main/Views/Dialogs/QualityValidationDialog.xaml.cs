using Apps.Devlosys.Core.Mvvm;
using System.Windows.Controls;

namespace Apps.Devlosys.Modules.Main.Views.Dialogs
{
    /// <summary>
    /// Logique d'interaction pour QualityValidationDialog.xaml
    /// </summary>
    public partial class QualityValidationDialog : UserControl
    {
        public QualityValidationDialog()
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
                case "UserName":
                    UserName.Focus();
                    UserName.SelectAll();

                    break;
            }
        }
    }
}
