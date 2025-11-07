using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using teams_phonemanager.Services;

namespace teams_phonemanager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void LogDialogBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Close dialog when clicking on backdrop
        if (DataContext is ViewModels.MainWindowViewModel viewModel)
        {
            viewModel.CloseLogDialogCommand.Execute(null);
        }
    }

    private void LogDialogCard_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Stop event propagation so clicking on card doesn't close the dialog
        e.Handled = true;
    }

    private void ScrollLogToEnd()
    {
        try
        {
            // Try to find TextBox by name first (if accessible)
            var textBox = this.FindControl<TextBox>("LogView");

            // If not accessible by name, try to find it in the visual tree
            if (textBox == null)
            {
                textBox = this.FindDescendantOfType<TextBox>();
            }

            if (textBox != null)
            {
                // Wait for text to be populated if it's empty
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    return; // Try again later
                }

                // Find the parent ScrollViewer
                var scrollViewer = textBox.FindAncestorOfType<ScrollViewer>();

                // Move caret to end
                textBox.CaretIndex = textBox.Text?.Length ?? 0;
                textBox.SelectionStart = textBox.CaretIndex;
                textBox.SelectionEnd = textBox.CaretIndex;
                
                // Also scroll the ScrollViewer parent if available
                if (scrollViewer != null)
                {
                    scrollViewer.Offset = new Avalonia.Vector(0, scrollViewer.Extent.Height);
                }
            }
        }
        catch
        {
            // Ignore errors - element might not be ready yet
        }
    }

    private void LogView_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            // Scroll to end when text changes (new log entries)
            Dispatcher.UIThread.Post(() =>
            {
                textBox.CaretIndex = textBox.Text?.Length ?? 0;
                textBox.SelectionStart = textBox.CaretIndex;
                textBox.SelectionEnd = textBox.CaretIndex;
                
                // Find the parent ScrollViewer and scroll it too
                var scrollViewer = textBox.FindAncestorOfType<ScrollViewer>();
                if (scrollViewer != null)
                {
                    scrollViewer.Offset = new Avalonia.Vector(0, scrollViewer.Extent.Height);
                }
            }, DispatcherPriority.Background);
        }
    }

    private void SettingsOverlay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        // Close settings when clicking on the overlay
        if (DataContext is ViewModels.MainWindowViewModel viewModel)
        {
            viewModel.CloseSettingsCommand.Execute(null);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        PowerShellContextService.Instance.Dispose();
    }
}
