using Avalonia.Controls;
using Avalonia.Input;

namespace teams_phonemanager.Views
{
    public partial class HolidaysView : UserControl
    {
        public HolidaysView()
        {
            InitializeComponent();
        }

        private void CreateHolidayBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.HolidaysViewModel viewModel)
            {
                viewModel.CloseCreateHolidayDialogCommand.Execute(null);
            }
        }

        private void CreateHolidayCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void CheckAutoAttendantBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.HolidaysViewModel viewModel)
            {
                viewModel.CloseCheckAutoAttendantDialogCommand.Execute(null);
            }
        }

        private void CheckAutoAttendantCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void AttachHolidayBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.HolidaysViewModel viewModel)
            {
                viewModel.CloseAttachHolidayDialogCommand.Execute(null);
            }
        }

        private void AttachHolidayCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }
    }
} 
