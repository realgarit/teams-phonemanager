using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Views
{
    public partial class CallQueuesView : UserControl
    {
        public CallQueuesView()
        {
            InitializeComponent();
            DataContext = Program.Services?.GetService<CallQueuesViewModel>();
        }

        private void CreateResourceAccountBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.CallQueuesViewModel viewModel)
            {
                viewModel.CloseCreateResourceAccountDialogCommand.Execute(null);
            }
        }

        private void CreateResourceAccountCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void UpdateUsageLocationBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.CallQueuesViewModel viewModel)
            {
                viewModel.CloseUpdateUsageLocationDialogCommand.Execute(null);
            }
        }

        private void UpdateUsageLocationCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void CreateCallQueueBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.CallQueuesViewModel viewModel)
            {
                viewModel.CloseCreateCallQueueDialogCommand.Execute(null);
            }
        }

        private void CreateCallQueueCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void AssociateResourceAccountBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.CallQueuesViewModel viewModel)
            {
                viewModel.CloseAssociateDialogCommand.Execute(null);
            }
        }

        private void AssociateResourceAccountCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }
    }
} 
