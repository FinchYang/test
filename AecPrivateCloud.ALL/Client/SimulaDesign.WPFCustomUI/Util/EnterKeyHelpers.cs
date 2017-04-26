using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SimulaDesign.WPFCustomUI.Util
{
    //http://www.dotnetjobsandcareers.com/how-to-invoke-a-command-on-the-viewmodel-by-pressing-the-enter-key-in-a-textbox-with-silverlight-and-mvvm/
    public static class EnterKeyHelpers
    {
        public static ICommand GetEnterKeyCommand(DependencyObject target)
        {
            return (ICommand)target.GetValue(EnterKeyCommandProperty);
        }

        public static void SetEnterKeyCommand(DependencyObject target, ICommand value)
        {
            target.SetValue(EnterKeyCommandProperty, value);
        }

        public static readonly DependencyProperty EnterKeyCommandProperty =
            DependencyProperty.RegisterAttached(
                "EnterKeyCommand",
                typeof(ICommand),
                typeof(EnterKeyHelpers),
                new PropertyMetadata(null, OnEnterKeyCommandChanged));

        static void OnEnterKeyCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var command = (ICommand)e.NewValue;
            var fe = (FrameworkElement)target;
            var control = (Control)target;
            control.KeyDown += (s, args) =>
            {
                if (args.Key == Key.Enter)
                {
                    // make sure the textbox binding updates its source first
                    BindingExpression b = control.GetBindingExpression(TextBox.TextProperty);
                    if (b != null)
                    {
                        b.UpdateSource();
                    }
                    command.Execute(null);
                }
            };
        }
    }
}
