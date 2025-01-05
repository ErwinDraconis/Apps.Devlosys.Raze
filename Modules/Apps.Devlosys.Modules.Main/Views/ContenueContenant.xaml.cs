using Apps.Devlosys.Core.Mvvm;
using MaterialDesignColors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Apps.Devlosys.Modules.Main.Views
{
    /// <summary>
    /// Interaction logic for ContenueContenant.xaml
    /// </summary>
    public partial class ContenueContenant : UserControl
    {
        public ContenueContenant()
        {
            InitializeComponent();

            Loaded += ContenueContenant_Loaded;
        }

        private void ContenueContenant_Loaded(object sender, RoutedEventArgs e)
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
                case "SNRGalia":
                    SNRGalia.Focus();
                    break;

                case "SNR":
                    SNR.Focus();
                    break;
            }
        }

    }
}
