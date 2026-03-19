using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.Helpers;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Views
{
    public partial class M365GroupsView : UserControl
    {
        public M365GroupsView()
        {
            InitializeComponent();
            DataContext = Program.Services?.GetService<M365GroupsViewModel>();
        }

        private M365GroupsViewModel? VM => DataContext as M365GroupsViewModel;

        // Confirm dialog intentionally does not close on backdrop click - it has action buttons only
        private void ConfirmVariablesBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e) { }

        private void CreateGroupBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCreateGroupDialogCommand);

        private void ConfirmVariablesCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void CreateGroupCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
    }
}
