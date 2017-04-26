using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace SimulaDesign.WPFCustomUI.Controls
{
    public class BindableWebBrowser : FrameworkElement
    {
        CommandBinding cb;
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Uri),
            typeof(BindableWebBrowser), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(SourceChangedHandler)));


        private readonly WebBrowser webBrowser;

        static BindableWebBrowser()
        {
            FocusableProperty.OverrideMetadata(typeof(BindableWebBrowser), new FrameworkPropertyMetadata(true));
        }
        public BindableWebBrowser()
        {
            webBrowser = new WebBrowser();
            cb = new CommandBinding(NavigationCommands.BrowseBack, BrowseBack, CanBrowseBack);
            CommandBindings.Add(cb);
            var cb2 = new CommandBinding(NavigationCommands.BrowseForward, BrowseForward, CanBrowseForward);
            CommandBindings.Add(cb2);
            webBrowser.Navigated += new NavigatedEventHandler(WebNavigated);

            base.AddVisualChild(webBrowser);
            base.AddLogicalChild(webBrowser);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (webBrowser != null)
            {
                webBrowser.Measure(availableSize);
                return webBrowser.DesiredSize;
            }
            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (webBrowser != null)
            {
                webBrowser.Arrange(new Rect(finalSize));
            }

            return base.ArrangeOverride(finalSize);
        }


        public WebBrowser WebBrowser { get { return webBrowser; } }

        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        protected override Visual GetVisualChild(int index)
        {
            return webBrowser;
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return webBrowser != null ? 1 : 0;
            }
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                yield return webBrowser;
            }
        }


        private static void SourceChangedHandler(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var bindableWebBrowser = (BindableWebBrowser)source;
            bindableWebBrowser.WebBrowser.Source = (Uri)e.NewValue;

        }

        private void CanBrowseBack(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = webBrowser.CanGoBack;
        }

        private void BrowseBack(object sender, ExecutedRoutedEventArgs e)
        {
            webBrowser.GoBack();
        }

        private void CanBrowseForward(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = webBrowser.CanGoForward;
        }

        private void BrowseForward(object sender, ExecutedRoutedEventArgs e)
        {
            webBrowser.GoForward();
        }

        private void WebNavigated(object sender, EventArgs e)
        {
            this.Focus();
        }
    } 
}
