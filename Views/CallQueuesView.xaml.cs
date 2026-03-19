using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.Helpers;
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

        private CallQueuesViewModel? VM => DataContext as CallQueuesViewModel;

        private void CreateResourceAccountBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCreateResourceAccountDialogCommand);

        private void UpdateUsageLocationBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseUpdateUsageLocationDialogCommand);

        private void CreateCallQueueBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCreateCallQueueDialogCommand);

        private void AssociateResourceAccountBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseAssociateDialogCommand);

        private void CreateResourceAccountCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void UpdateUsageLocationCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void CreateCallQueueCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void AssociateResourceAccountCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
    }
}
