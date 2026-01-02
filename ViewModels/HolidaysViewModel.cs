using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using teams_phonemanager.Services.Interfaces;
using teams_phonemanager.Services;
using teams_phonemanager.Models;
using System;
using System.Threading.Tasks;
using Avalonia.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace teams_phonemanager.ViewModels
{
    public partial class HolidaysViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _showCreateHolidayDialog;

        [ObservableProperty]
        private bool _showCheckAutoAttendantDialog;

        [ObservableProperty]
        private bool _showAttachHolidayDialog;

        [ObservableProperty]
        private string _holidayName = string.Empty;

        [ObservableProperty]
        private DateTime _holidayDate = DateTime.Now;


        [ObservableProperty]
        private string _autoAttendantName = string.Empty;

        [ObservableProperty]
        private bool _isHolidayCreated = false;

        public HolidaysViewModel(
            IPowerShellContextService powerShellContextService,
            IPowerShellCommandService powerShellCommandService,
            ILoggingService loggingService,
            ISessionManager sessionManager,
            INavigationService navigationService,
            IErrorHandlingService errorHandlingService,
            IValidationService validationService)
            : base(powerShellContextService, powerShellCommandService, loggingService,
                  sessionManager, navigationService, errorHandlingService, validationService)
        {
            _mainWindowViewModel = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow?.DataContext as MainWindowViewModel
                : null;
            _loggingService.Log("Holidays page loaded", LogLevel.Info);
        }

        [RelayCommand]
        private void ResetHolidayState()
        {
            IsHolidayCreated = false;
            HolidayName = string.Empty;
            HolidayDate = DateTime.Now;
            StatusMessage = "Holiday state reset. You can now create a new holiday.";
        }

        [RelayCommand]
        private void OpenCreateHolidayDialog()
        {
            var variables = _mainWindowViewModel?.Variables;
            if (variables != null && !string.IsNullOrEmpty(variables.Customer) && !string.IsNullOrEmpty(variables.CustomerGroupName))
            {
                HolidayName = variables.HolidayName;
                HolidayDate = variables.HolidayDate;
            }
            else
            {
                HolidayName = "hd-";
                HolidayDate = DateTime.Now;
            }
            ShowCreateHolidayDialog = true;
        }

        [RelayCommand]
        private void CloseCreateHolidayDialog()
        {
            ShowCreateHolidayDialog = false;
        }

        [RelayCommand]
        private async Task CreateHolidayAsync()
        {
            try
            {
                IsBusy = true;
                ShowCreateHolidayDialog = false;

                var variables = _mainWindowViewModel?.Variables;
                if (variables == null)
                {
                    StatusMessage = "Error: Variables not found";
                    return;
                }

                if (variables.HolidaySeries.Count == 0)
                {
                    StatusMessage = "Error: No holidays configured. Please add holidays in the Variables page first.";
                    return;
                }

                // Create a single holiday schedule with multiple date ranges
                var holidayDates = variables.HolidaySeries.Select(h => h.DateTime).ToList();
                var holidayName = variables.HolidayName;
                
                _loggingService.Log($"Creating holiday series: {holidayName} with {holidayDates.Count} dates", LogLevel.Info);

                var command = _powerShellCommandService.GetCreateHolidaySeriesCommand(holidayName, holidayDates);
                var result = await ExecutePowerShellCommandAsync(command, "CreateHolidaySeries");
                
                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Holiday series '{holidayName}' created successfully with {holidayDates.Count} dates!";
                    _loggingService.Log($"Holiday series {holidayName} created successfully", LogLevel.Info);
                    IsHolidayCreated = true;
                }
                else
                {
                    StatusMessage = $"Error creating holiday series: {result}";
                    _loggingService.Log($"Error creating holiday series {holidayName}: {result}", LogLevel.Error);
                    IsHolidayCreated = false;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in CreateHolidayAsync: {ex}", LogLevel.Error);
                IsHolidayCreated = false;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OpenCheckAutoAttendantDialog()
        {
            var variables = _mainWindowViewModel?.Variables;
            if (variables != null && !string.IsNullOrEmpty(variables.Customer) && !string.IsNullOrEmpty(variables.CustomerGroupName))
            {
                AutoAttendantName = variables.AaDisplayName;
            }
            else
            {
                AutoAttendantName = "aa-";
            }
            ShowCheckAutoAttendantDialog = true;
        }

        [RelayCommand]
        private void CloseCheckAutoAttendantDialog()
        {
            ShowCheckAutoAttendantDialog = false;
        }

        [RelayCommand]
        private async Task VerifyAutoAttendantAsync()
        {
            if (string.IsNullOrWhiteSpace(AutoAttendantName))
            {
                StatusMessage = "Error: Auto attendant name cannot be empty";
                return;
            }

            try
            {
                IsBusy = true;
                ShowCheckAutoAttendantDialog = false;

                _loggingService.Log($"Verifying auto attendant: {AutoAttendantName}", LogLevel.Info);

                var command = _powerShellCommandService.GetVerifyAutoAttendantCommand(AutoAttendantName);
                var result = await ExecutePowerShellCommandAsync(command, "VerifyAutoAttendant");
                
                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Auto attendant '{AutoAttendantName}' verified successfully and is ready for holiday configuration";
                    _loggingService.Log($"Auto attendant {AutoAttendantName} verified successfully", LogLevel.Info);
                }
                else
                {
                    StatusMessage = $"Error: Auto attendant '{AutoAttendantName}' not found or not accessible: {result}";
                    _loggingService.Log($"Error verifying auto attendant {AutoAttendantName}: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in VerifyAutoAttendantAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OpenAttachHolidayDialog()
        {
            var variables = _mainWindowViewModel?.Variables;
            if (variables != null)
            {
                AutoAttendantName = variables.AaDisplayName;
                HolidayName = variables.HolidayName;
            }
            else
            {
                AutoAttendantName = "aa-";
                HolidayName = "hd-";
            }
            ShowAttachHolidayDialog = true;
        }

        [RelayCommand]
        private void CloseAttachHolidayDialog()
        {
            ShowAttachHolidayDialog = false;
        }

        [RelayCommand]
        private async Task AttachHolidayToAutoAttendantAsync()
        {
            if (string.IsNullOrWhiteSpace(AutoAttendantName))
            {
                StatusMessage = "Error: Auto attendant name cannot be empty";
                return;
            }

            try
            {
                IsBusy = true;
                ShowAttachHolidayDialog = false;

                var variables = _mainWindowViewModel?.Variables;
                if (variables == null)
                {
                    StatusMessage = "Error: Variables not found";
                    return;
                }

                var holidayName = HolidayName;
                _loggingService.Log($"Attaching holiday {holidayName} to auto attendant {AutoAttendantName}", LogLevel.Info);

                var command = _powerShellCommandService.GetAttachHolidayToAutoAttendantCommand(holidayName, AutoAttendantName, variables.HolidayGreetingPromptDE);
                var result = await ExecutePowerShellCommandAsync(command, "AttachHolidayToAutoAttendant");
                
                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Successfully attached holiday '{holidayName}' to auto attendant '{AutoAttendantName}'";
                    _loggingService.Log($"Successfully attached holiday {holidayName} to auto attendant {AutoAttendantName}", LogLevel.Info);
                }
                else
                {
                    StatusMessage = $"Error attaching holiday to auto attendant: {result}";
                    _loggingService.Log($"Error attaching holiday {holidayName} to auto attendant {AutoAttendantName}: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in AttachHolidayToAutoAttendantAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
} 
