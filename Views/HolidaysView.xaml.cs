using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.Helpers;
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

        private HolidaysViewModel? VM => DataContext as HolidaysViewModel;

        private void CreateHolidayBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCreateHolidayDialogCommand);

        private void CheckAutoAttendantBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseCheckAutoAttendantDialogCommand);

        private void AttachHolidayBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseAttachHolidayDialogCommand);

        private void CreateHolidayCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void CheckAutoAttendantCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void AttachHolidayCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
    }
}
