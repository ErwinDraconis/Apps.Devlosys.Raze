using Apps.Devlosys.Core.Mvvm;
using System.Windows;
using System.Windows.Controls;

namespace Apps.Devlosys.Modules.Authentification.Views
{
    /// <summary>
    /// Logique d'interaction pour LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();

            Loaded += LoginView_Loaded;
        }

        private void LoginView_Loaded(object sender, RoutedEventArgs e)
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
                case "UserName":
                    UserName.Focus();
                    UserName.SelectAll();

                    break;
            }
        }
    }
}
