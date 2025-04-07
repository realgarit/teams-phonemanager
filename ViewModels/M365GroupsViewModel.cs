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
        private readonly PowerShellService _powerShellService;
        private readonly LoggingService _loggingService;
        private readonly SessionManager _sessionManager;
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
            _powerShellService = PowerShellService.Instance;
            _loggingService = LoggingService.Instance;
            _sessionManager = SessionManager.Instance;
            _mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

            _loggingService.Log("M365 Groups page loaded", LogLevel.Info);
        }

        [RelayCommand]
        private void ShowVariablesConfirmation()
        {
            ShowConfirmation = true;
        }

        [RelayCommand]
        private void NavigateToVariables()
        {
            if (_mainWindowViewModel != null)
            {
                _mainWindowViewModel.NavigateTo("Variables");
                _loggingService.Log("Navigated to Variables page", LogLevel.Info);
            }
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

                m365group = variables.M365Group;
                _loggingService.Log($"Checking M365 group: {m365group}", LogLevel.Info);

                var command = $@"
# Check if M365 group already exists
$existingGroup = Get-MgGroup -Filter ""displayName eq '{m365group}'"" -ErrorAction SilentlyContinue
if ($existingGroup) 
{{
    Write-Host ""{m365group} already exists. Please check, otherwise SFO will be salty!""
    # Get the ID of the existing M365 group
    $global:m365groupId = $existingGroup.Id
    Write-Host ""{m365group} was found successfully with ID: $global:m365groupId""
    return
}} 

try {{
    # Create M365 group
    $newGroup = New-MgGroup -DisplayName ""{m365group}"" `
        -MailEnabled:$False `
        -MailNickName ""{m365group}"" `
        -SecurityEnabled `
        -GroupTypes @(""Unified"")

    # Get the ID of the created M365 group
    $global:m365groupId = $newGroup.Id
    Write-Host ""{m365group} created successfully with ID: $global:m365groupId""
}}
catch {{
    Write-Host ""{m365group} failed to create: $_""
    exit
}}";

                var result = await _powerShellService.ExecuteCommandAsync(command);
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