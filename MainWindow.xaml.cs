using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.ComponentModel;
using teams_phonemanager.Services;

namespace teams_phonemanager;

public partial class MainWindow : Window
{
    private bool _isSyncingNavSelection;

    public MainWindow()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is ViewModels.MainWindowViewModel vm)
        {
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModels.MainWindowViewModel.CurrentPage))
        {
            SyncNavSelectionToCurrentPage();
        }
    }

    /// <summary>
    /// Syncs the sidebar ListBox selection to match the current page.
    /// This handles the case where navigation happens programmatically
    /// (e.g. from Wizard "Edit Variables" or Welcome "Get Started").
    /// </summary>
    private void SyncNavSelectionToCurrentPage()
    {
        if (_isSyncingNavSelection) return;
        if (DataContext is not ViewModels.MainWindowViewModel vm) return;

        var listBox = this.FindControl<ListBox>("NavigationListBox");
        if (listBox == null) return;

        // Normalize page name for matching (strip spaces to match Tag values)
        var targetTag = vm.CurrentPage.Replace(" ", "");

        foreach (var item in listBox.Items)
        {
            if (item is ListBoxItem listBoxItem && listBoxItem.Tag is string tag && tag == targetTag)
            {
                _isSyncingNavSelection = true;
                listBox.SelectedItem = listBoxItem;
                _isSyncingNavSelection = false;
                return;
            }
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && DataContext is ViewModels.MainWindowViewModel vm)
        {
            if (vm.IsLogDialogOpen)
            {
                vm.CloseLogDialogCommand.Execute(null);
                e.Handled = true;
            }
            else if (vm.IsSettingsOpen)
            {
                vm.CloseSettingsCommand.Execute(null);
                e.Handled = true;
            }
        }
    }

    private void LogDialogBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is ViewModels.MainWindowViewModel viewModel)
        {
            viewModel.CloseLogDialogCommand.Execute(null);
        }
    }

    private void LogDialogCard_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        e.Handled = true;
    }

    private void ScrollLogToEnd()
    {
        try
        {
            var textBox = this.FindControl<TextBox>("LogView");
            if (textBox == null)
            {
                textBox = this.FindDescendantOfType<TextBox>();
            }

            if (textBox != null)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    return;
                }

                var scrollViewer = textBox.FindAncestorOfType<ScrollViewer>();
                textBox.CaretIndex = textBox.Text?.Length ?? 0;
                textBox.SelectionStart = textBox.CaretIndex;
                textBox.SelectionEnd = textBox.CaretIndex;

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
            Dispatcher.UIThread.Post(() =>
            {
                textBox.CaretIndex = textBox.Text?.Length ?? 0;
                textBox.SelectionStart = textBox.CaretIndex;
                textBox.SelectionEnd = textBox.CaretIndex;

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
        if (DataContext is ViewModels.MainWindowViewModel viewModel)
        {
            viewModel.CloseSettingsCommand.Execute(null);
        }
    }

    private void NavigationListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_isSyncingNavSelection) return;

        if (sender is ListBox listBox && listBox.SelectedItem is ListBoxItem item && item.Tag is string page)
        {
            if (DataContext is ViewModels.MainWindowViewModel viewModel)
            {
                viewModel.NavigateToCommand.Execute(page);
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        var psContext = Program.Services?.GetService(typeof(Services.Interfaces.IPowerShellContextService)) as IDisposable;
        psContext?.Dispose();
    }
}
