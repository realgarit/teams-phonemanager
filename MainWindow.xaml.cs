using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using teams_phonemanager.Services;

namespace teams_phonemanager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void LogDialogHost_DialogOpened(object sender, MaterialDesignThemes.Wpf.DialogOpenedEventArgs eventArgs)
    {
        var dialogHost = sender as MaterialDesignThemes.Wpf.DialogHost;
        DependencyObject searchRoot = dialogHost != null ? (DependencyObject)dialogHost : this;

        // Scroll to end when dialog opens - use dispatcher to ensure it happens after rendering
        Dispatcher.BeginInvoke(new Action(() =>
        {
            ScrollLogToEnd(searchRoot);
        }), DispatcherPriority.Loaded);
    }

    private void ScrollLogToEnd(DependencyObject? searchRoot = null)
    {
        try
        {
            searchRoot ??= this;

            // Try to find TextBox by name first (if accessible)
            TextBox? textBox = LogView;

            // If not accessible by name, try to find it in the visual tree
            if (textBox == null)
            {
                textBox = FindVisualChild<TextBox>(searchRoot);
            }

            if (textBox != null)
            {
                // Wait for text to be populated if it's empty
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    return; // Try again later
                }

                // Find the parent ScrollViewer
                ScrollViewer? scrollViewer = FindVisualParent<ScrollViewer>(textBox);

                // Move caret to end
                textBox.CaretIndex = textBox.Text.Length;
                textBox.ScrollToEnd();
                
                // Also scroll the ScrollViewer parent if available
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }
        catch
        {
            // Ignore errors - element might not be ready yet
        }
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t)
            {
                return t;
            }
            
            var childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
            {
                return childOfChild;
            }
        }
        return null;
    }

    private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent != null)
        {
            if (parent is T t)
            {
                return t;
            }
            parent = VisualTreeHelper.GetParent(parent);
        }
        return null;
    }

    private void LogView_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // Scroll to end when text changes (new log entries)
            Dispatcher.BeginInvoke(new Action(() =>
            {
                textBox.CaretIndex = textBox.Text.Length;
                textBox.ScrollToEnd();
                
                // Find the parent ScrollViewer and scroll it too
                var scrollViewer = FindVisualParent<ScrollViewer>(textBox);
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToEnd();
                }
            }), DispatcherPriority.Background);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        PowerShellContextService.Instance.Dispose();
    }
}
