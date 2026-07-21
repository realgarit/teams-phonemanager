using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using PhoneDesk.Helpers;
using PhoneDesk.ViewModels;

namespace PhoneDesk.Views
{
    public partial class HolidaysView : UserControl
    {
        public HolidaysView()
        {
            InitializeComponent();
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
