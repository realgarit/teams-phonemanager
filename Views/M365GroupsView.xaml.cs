using Avalonia.Controls;
using Avalonia.Input;

namespace teams_phonemanager.Views
{
    public partial class M365GroupsView : UserControl
    {
        public M365GroupsView()
        {
            InitializeComponent();
        }

        private void ConfirmVariablesBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // This dialog doesn't have a close command, it only has action buttons
            // So we don't close it on backdrop click
        }

        private void ConfirmVariablesCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void CreateGroupBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.M365GroupsViewModel viewModel)
            {
                viewModel.CloseCreateGroupDialogCommand.Execute(null);
            }
        }

        private void CreateGroupCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }
    }
} 
