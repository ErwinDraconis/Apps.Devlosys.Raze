using Prism.Services.Dialogs;
using System.Windows;

namespace Apps.Devlosys.Raze.Dialogs
{
    /// <summary>
    /// Logique d'interaction pour DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : Window, IDialogWindow
    {
        public DialogWindow()
        {
            InitializeComponent();
        }

        public IDialogResult Result { get; set; }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            InvalidateVisual();
        }
    }
}
