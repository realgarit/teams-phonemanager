using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Views
{
    public partial class HolidaysView : UserControl
    {
        public HolidaysView()
        {
            InitializeComponent();
            DataContext = Program.Services?.GetService<HolidaysViewModel>();
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
