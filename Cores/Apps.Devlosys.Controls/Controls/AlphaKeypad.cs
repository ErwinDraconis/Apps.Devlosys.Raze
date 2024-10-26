using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Apps.Devlosys.Controls
{
    [TemplatePart(Name = CapsToUpperName, Type = typeof(Button))]
    public class AlphaKeypad : ContentControl
    {
        public const string CapsToUpperName = "PART_CapsToUpper";
        public const string CapsToLowerName = "PART_CapsToLower";

        private Button _to_upper_button;
        private Button _to_lower_button;

        static AlphaKeypad()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AlphaKeypad), new FrameworkPropertyMetadata(typeof(AlphaKeypad)));
            
        }

        public override void OnApplyTemplate()
        {
            _to_upper_button = GetTemplateChild(CapsToUpperName) as Button;
            _to_lower_button = GetTemplateChild(CapsToLowerName) as Button;

            _to_upper_button.Click += _to_upper_button_Click;
            _to_lower_button.Click += _to_lower_button_Click;

            base.OnApplyTemplate();
        }

        private void _to_upper_button_Click(object sender, RoutedEventArgs e)
        {
            IsLower = false;
        }

        private void _to_lower_button_Click(object sender, RoutedEventArgs e)
        {
            IsLower = true;
        }

        #region DependencyProperty : IsLowerProperty

        public static readonly DependencyProperty IsLowerProperty =
            DependencyProperty.Register("IsLower", typeof(bool), typeof(AlphaKeypad), new PropertyMetadata(true));

        public bool IsLower
        {
            get { return (bool)GetValue(IsLowerProperty); }
            set { SetValue(IsLowerProperty, value); }
        }

        #endregion

        #region DependencyProperty : IsDecimalProperty

        public static readonly DependencyProperty IsDecimalProperty =
            DependencyProperty.Register(nameof(IsDecimal), typeof(bool), typeof(AlphaKeypad), new PropertyMetadata(false));

        public bool IsDecimal
        {
            get => (bool)GetValue(IsDecimalProperty);
            set => SetValue(IsDecimalProperty, value);
        }

        #endregion

        #region  DependencyProperty : CommandProperty

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(AlphaKeypad), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #endregion
    }
}
