using Avalonia.Controls;
using Avalonia.Input;

namespace teams_phonemanager.Views
{
    public partial class AutoAttendantsView : UserControl
    {
        public AutoAttendantsView()
        {
            InitializeComponent();
        }

        private void CreateResourceAccountBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.AutoAttendantsViewModel viewModel)
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
            if (DataContext is ViewModels.AutoAttendantsViewModel viewModel)
            {
                viewModel.CloseUpdateUsageLocationDialogCommand.Execute(null);
            }
        }

        private void UpdateUsageLocationCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void CreateAutoAttendantBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.AutoAttendantsViewModel viewModel)
            {
                viewModel.CloseCreateAutoAttendantDialogCommand.Execute(null);
            }
        }

        private void CreateAutoAttendantCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void AssociateResourceAccountBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.AutoAttendantsViewModel viewModel)
            {
                viewModel.CloseAssociateDialogCommand.Execute(null);
            }
        }

        private void AssociateResourceAccountCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void ValidateCallQueueBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.AutoAttendantsViewModel viewModel)
            {
                viewModel.CloseValidateCallQueueDialogCommand.Execute(null);
            }
        }

        private void ValidateCallQueueCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void CreateCallTargetBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.AutoAttendantsViewModel viewModel)
            {
                viewModel.CloseCreateCallTargetDialogCommand.Execute(null);
            }
        }

        private void CreateCallTargetCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void CreateDefaultCallFlowBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.AutoAttendantsViewModel viewModel)
            {
                viewModel.CloseCreateDefaultCallFlowDialogCommand.Execute(null);
            }
        }

        private void CreateDefaultCallFlowCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void CreateAfterHoursCallFlowBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.AutoAttendantsViewModel viewModel)
            {
                viewModel.CloseCreateAfterHoursCallFlowDialogCommand.Execute(null);
            }
        }

        private void CreateAfterHoursCallFlowCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void CreateAfterHoursScheduleBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.AutoAttendantsViewModel viewModel)
            {
                viewModel.CloseCreateAfterHoursScheduleDialogCommand.Execute(null);
            }
        }

        private void CreateAfterHoursScheduleCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void CreateCallHandlingAssociationBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.AutoAttendantsViewModel viewModel)
            {
                viewModel.CloseCreateCallHandlingAssociationDialogCommand.Execute(null);
            }
        }

        private void CreateCallHandlingAssociationCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }
    }
} 
