using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.Helpers;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Views
{
    public partial class AutoAttendantsView : UserControl
    {
        public AutoAttendantsView()
        {
            InitializeComponent();
            DataContext = Program.Services?.GetService<AutoAttendantsViewModel>();
        }

        private AutoAttendantsViewModel? VM => DataContext as AutoAttendantsViewModel;

        // Backdrop handlers - close the dialog
        private void CreateResourceAccountBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCreateResourceAccountDialogCommand);

        private void UpdateUsageLocationBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseUpdateUsageLocationDialogCommand);

        private void CreateAutoAttendantBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCreateAutoAttendantDialogCommand);

        private void AssociateResourceAccountBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseAssociateDialogCommand);

        private void ValidateCallQueueBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseValidateCallQueueDialogCommand);

        private void CreateCallTargetBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCreateCallTargetDialogCommand);

        private void CreateDefaultCallFlowBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCreateDefaultCallFlowDialogCommand);

        private void CreateAfterHoursCallFlowBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCreateAfterHoursCallFlowDialogCommand);

        private void CreateAfterHoursScheduleBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCreateAfterHoursScheduleDialogCommand);

        private void CreateCallHandlingAssociationBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCreateCallHandlingAssociationDialogCommand);

        // Card handlers - stop click propagation (all identical)
        private void CreateResourceAccountCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void UpdateUsageLocationCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void CreateAutoAttendantCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void AssociateResourceAccountCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void ValidateCallQueueCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void CreateCallTargetCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void CreateDefaultCallFlowCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void CreateAfterHoursCallFlowCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void CreateAfterHoursScheduleCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void CreateCallHandlingAssociationCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
    }
}
