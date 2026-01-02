using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.DependencyInjection;
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

        private void HolidaySeriesManagerBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Close dialog when clicking on backdrop
            if (DataContext is ViewModels.VariablesViewModel viewModel)
            {
                viewModel.CancelHolidaySeriesManagerCommand.Execute(null);
            }
        }

        private void HolidaySeriesManagerCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            // Stop event propagation so clicking on card doesn't close the dialog
            e.Handled = true;
        }

        private void HolidayTimePickerBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.VariablesViewModel viewModel)
            {
                viewModel.CloseHolidayTimePickerCommand.Execute(null);
            }
        }

        private void HolidayTimePickerCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void EditHolidayBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.VariablesViewModel viewModel)
            {
                viewModel.CancelEditHolidayCommand.Execute(null);
            }
        }

        private void EditHolidayCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void PredefinedHolidaysWizardBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.VariablesViewModel viewModel)
            {
                viewModel.CancelPredefinedHolidaysWizardCommand.Execute(null);
            }
        }

        private void PredefinedHolidaysWizardCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void AargauInfoBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.VariablesViewModel viewModel)
            {
                viewModel.CloseAargauInfoCommand.Execute(null);
            }
        }

        private void AargauInfoCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void CallQueueConfigurationBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.VariablesViewModel viewModel)
            {
                viewModel.CancelCallQueueConfigurationCommand.Execute(null);
            }
        }

        private void CallQueueConfigurationCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }

        private void AutoAttendantConfigurationBackdrop_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (DataContext is ViewModels.VariablesViewModel viewModel)
            {
                viewModel.CancelAutoAttendantConfigurationCommand.Execute(null);
            }
        }

        private void AutoAttendantConfigurationCard_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            e.Handled = true;
        }
    }
} 
