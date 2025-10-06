using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using teams_phonemanager.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace teams_phonemanager.ViewModels
{
    public partial class M365GroupsViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to the M365 Groups page. This page will help you check and create the M365 group for your phone system.";

        [ObservableProperty]
        private bool _isGroupChecked;

        [ObservableProperty]
        private string _groupId = string.Empty;

        [ObservableProperty]
        private string _groupStatus = string.Empty;

        [ObservableProperty]
        private bool _showConfirmation;

        public M365GroupsViewModel()
        {
            _mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

            _loggingService.Log("M365 Groups page loaded", LogLevel.Info);
        }

        [RelayCommand]
        private void ShowVariablesConfirmation()
        {
            ShowConfirmation = true;
        }

        [RelayCommand]
        private void NavigateToVariablesPage()
        {
            NavigateToVariables();
        }

        [RelayCommand]
        private async Task CheckM365GroupAsync()
        {
            string m365group = string.Empty;
            try
            {
                IsBusy = true;
                ShowConfirmation = false;

                var variables = _mainWindowViewModel?.Variables;
                if (variables == null)
                {
                    _loggingService.Log("Variables not found. Please set variables first.", LogLevel.Error);
                    GroupStatus = "Error: Variables not found. Please set variables first.";
                    return;
                }

                if (!ValidateVariables(variables))
                {
                    return;
                }

                m365group = variables.M365Group;
                _loggingService.Log($"Checking M365 group: {m365group}", LogLevel.Info);

                var command = _powerShellCommandService.GetCreateM365GroupCommand(m365group);
                var result = await ExecutePowerShellCommandAsync(command, "CheckM365Group");
                
                if (!string.IsNullOrEmpty(result))
                {
                    var output = result.Trim();
                    GroupId = output.Split(':')[1].Trim();
                    GroupStatus = output.Contains("already exists") ? "Group already exists" : "Group created successfully";
                    IsGroupChecked = true;
                    _loggingService.Log($"M365 Group {m365group} check completed: {GroupStatus}", LogLevel.Info);
                }
                else
                {
                    GroupStatus = "Error: No output from PowerShell command";
                    _loggingService.Log($"Error checking M365 Group {m365group}: No output from PowerShell command", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                GroupStatus = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in CheckM365GroupAsync for group {m365group}: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
} 
