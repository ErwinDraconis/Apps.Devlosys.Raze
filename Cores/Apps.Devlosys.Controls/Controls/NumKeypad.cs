using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Apps.Devlosys.Controls
{
    public class NumKeypad : ContentControl
    {
        static NumKeypad()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumKeypad), new FrameworkPropertyMetadata(typeof(NumKeypad)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        #region DependencyProperty : IsDecimalProperty

        public static readonly DependencyProperty IsDecimalProperty =
            DependencyProperty.Register(nameof(IsDecimal), typeof(bool), typeof(NumKeypad), new PropertyMetadata(false));

        public bool IsDecimal
        {
            get => (bool)GetValue(IsDecimalProperty);
            set => SetValue(IsDecimalProperty, value);
        }

        #endregion

        #region DependencyProperty : HasCheckProperty

        public static readonly DependencyProperty HasCheckProperty =
            DependencyProperty.Register(nameof(HasCheck), typeof(bool), typeof(NumKeypad), new PropertyMetadata(false));

        public bool HasCheck
        {
            get => (bool)GetValue(HasCheckProperty);
            set => SetValue(HasCheckProperty, value);
        }

        #endregion

        #region  DependencyProperty : CommandProperty

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(NumKeypad), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        #endregion
    }
}
