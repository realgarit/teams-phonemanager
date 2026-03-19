using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
using teams_phonemanager.Helpers;
using teams_phonemanager.ViewModels;

namespace teams_phonemanager.Views
{
    public partial class VariablesView : UserControl
    {
        public VariablesView()
        {
            InitializeComponent();
            DataContext = Program.Services?.GetService<VariablesViewModel>();
        }

        private VariablesViewModel? VM => DataContext as VariablesViewModel;

        private void HolidaySeriesManagerBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CancelHolidaySeriesManagerCommand);

        private void HolidayTimePickerBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseHolidayTimePickerCommand);

        private void EditHolidayBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CancelEditHolidayCommand);

        private void PredefinedHolidaysWizardBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CancelPredefinedHolidaysWizardCommand);

        private void AargauInfoBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CloseAargauInfoCommand);

        private void CallQueueConfigurationBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CancelCallQueueConfigurationCommand);

        private void AutoAttendantConfigurationBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
            => DialogEventHelper.CloseOnBackdropClick(VM, VM?.CancelAutoAttendantConfigurationCommand);

        private void HolidaySeriesManagerCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void HolidayTimePickerCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void EditHolidayCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void PredefinedHolidaysWizardCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void AargauInfoCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void CallQueueConfigurationCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
        private void AutoAttendantConfigurationCard_PointerPressed(object? sender, PointerPressedEventArgs e) => DialogEventHelper.StopPropagation(sender, e);
    }
}
