using Apps.Devlosys.Core.Mvvm;
using System.Windows;
using System.Windows.Controls;

namespace Apps.Devlosys.Modules.Main.Views
{
    /// <summary>
    /// Logique d'interaction pour BinView.xaml
    /// </summary>
    public partial class BinView : UserControl
    {
        public BinView()
        {
            InitializeComponent();

            Loaded += BinView_Loaded;
        }

        private void BinView_Loaded(object sender, RoutedEventArgs e)
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
                case "Search":
                    Search.Focus();
                    Search.SelectAll();

                    break;
            }
        }
    }
}
