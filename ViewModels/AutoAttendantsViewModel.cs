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
    public partial class AutoAttendantsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<ResourceAccount> _resourceAccounts = new();

        [ObservableProperty]
        private ObservableCollection<AutoAttendant> _autoAttendants = new();

        [ObservableProperty]
        private string _searchResourceAccountsText = string.Empty;

        [ObservableProperty]
        private string _searchAutoAttendantsText = string.Empty;

        [ObservableProperty]
        private bool _showCreateResourceAccountDialog;

        [ObservableProperty]
        private bool _showUpdateUsageLocationDialog;

        [ObservableProperty]
        private bool _showCreateAutoAttendantDialog;

        [ObservableProperty]
        private bool _showAssociateDialog;

        [ObservableProperty]
        private bool _showValidateCallQueueDialog;

        [ObservableProperty]
        private bool _showCreateCallTargetDialog;

        [ObservableProperty]
        private bool _showCreateDefaultCallFlowDialog;

        [ObservableProperty]
        private bool _showCreateAfterHoursCallFlowDialog;

        [ObservableProperty]
        private bool _showCreateAfterHoursScheduleDialog;

        [ObservableProperty]
        private bool _showCreateCallHandlingAssociationDialog;

        [ObservableProperty]
        private string _resourceAccountUpn = string.Empty;

        [ObservableProperty]
        private string _resourceAccountDisplayName = string.Empty;

        [ObservableProperty]
        private string _autoAttendantName = string.Empty;

        [ObservableProperty]
        private string _callQueueUpn = string.Empty;

        [ObservableProperty]
        private string _m365GroupId = string.Empty;

        public ObservableCollection<ResourceAccount> ResourceAccountsView => new ObservableCollection<ResourceAccount>(
            ResourceAccounts.Where(FilterResourceAccount)
        );
        public ObservableCollection<AutoAttendant> AutoAttendantsView => new ObservableCollection<AutoAttendant>(
            AutoAttendants.Where(FilterAutoAttendant)
        );

        public AutoAttendantsViewModel(
            IPowerShellContextService powerShellContextService,
            IPowerShellCommandService powerShellCommandService,
            ILoggingService loggingService,
            ISessionManager sessionManager,
            INavigationService navigationService,
            IErrorHandlingService errorHandlingService,
            IValidationService validationService,
            ISharedStateService sharedStateService,
            IDialogService dialogService)
            : base(powerShellContextService, powerShellCommandService, loggingService,
                  sessionManager, navigationService, errorHandlingService, validationService, sharedStateService, dialogService)
        {
            _loggingService.Log("Auto Attendants page loaded", LogLevel.Info);

            ResourceAccounts.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ResourceAccountsView));
            AutoAttendants.CollectionChanged += (s, e) => OnPropertyChanged(nameof(AutoAttendantsView));
        }

        [RelayCommand]
        private async Task RetrieveResourceAccountsAsync()
        {
            try
            {
                IsBusy = true;
                ResourceAccounts.Clear();
                StatusMessage = "Retrieving resource accounts...";

                _loggingService.Log("Retrieving resource accounts starting with 'raaa-'", LogLevel.Info);

                var command = _powerShellCommandService.GetRetrieveAutoAttendantResourceAccountsCommand();
                var result = await ExecutePowerShellCommandAsync(command, "RetrieveAutoAttendantResourceAccounts");
                
                if (!string.IsNullOrEmpty(result))
                {
                    ParseResourceAccountsFromResult(result);
                    StatusMessage = $"Found {ResourceAccounts.Count} resource accounts starting with 'raaa-'";
                    _loggingService.Log($"Retrieved {ResourceAccounts.Count} resource accounts", LogLevel.Info);
                }
                else
                {
                    StatusMessage = "Error: No output from PowerShell command";
                    _loggingService.Log("Error retrieving resource accounts: No output from PowerShell command", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in RetrieveResourceAccountsAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RetrieveAutoAttendantsAsync()
        {
            try
            {
                IsBusy = true;
                AutoAttendants.Clear();
                StatusMessage = "Retrieving auto attendants...";

                _loggingService.Log("Retrieving auto attendants containing 'aa-'", LogLevel.Info);

                var command = _powerShellCommandService.GetRetrieveAutoAttendantsCommand();
                var result = await ExecutePowerShellCommandAsync(command, "RetrieveAutoAttendants");
                
                if (!string.IsNullOrEmpty(result))
                {
                    ParseAutoAttendantsFromResult(result);
                    StatusMessage = $"Found {AutoAttendants.Count} auto attendants containing 'aa-'";
                    _loggingService.Log($"Retrieved {AutoAttendants.Count} auto attendants", LogLevel.Info);
                }
                else
                {
                    StatusMessage = "Error: No output from PowerShell command";
                    _loggingService.Log("Error retrieving auto attendants: No output from PowerShell command", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in RetrieveAutoAttendantsAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OpenCreateResourceAccountDialog()
        {
            var variables = _sharedStateService?.Variables;
            if (variables != null && !string.IsNullOrEmpty(variables.Customer) && !string.IsNullOrEmpty(variables.CustomerGroupName))
            {
                ResourceAccountUpn = variables.RaaaUPN;
                ResourceAccountDisplayName = variables.RaaaDisplayName;
            }
            else
            {
                ResourceAccountUpn = "raaa-";
                ResourceAccountDisplayName = "raaa-";
            }
            ShowCreateResourceAccountDialog = true;
        }

        [RelayCommand]
        private void CloseCreateResourceAccountDialog()
        {
            ShowCreateResourceAccountDialog = false;
        }

        [RelayCommand]
        private void CloseUpdateUsageLocationDialog()
        {
            ShowUpdateUsageLocationDialog = false;
        }


        [RelayCommand]
        private void CloseCreateAutoAttendantDialog()
        {
            ShowCreateAutoAttendantDialog = false;
        }

        [RelayCommand]
        private void CloseAssociateDialog()
        {
            ShowAssociateDialog = false;
        }

        [RelayCommand]
        private void OpenValidateCallQueueDialog()
        {
            var variables = _sharedStateService?.Variables;
            if (variables != null && !string.IsNullOrEmpty(variables.Customer) && !string.IsNullOrEmpty(variables.CustomerGroupName))
            {
                CallQueueUpn = variables.RacqUPN;
            }
            else
            {
                CallQueueUpn = "racq-";
            }
            ShowValidateCallQueueDialog = true;
        }

        [RelayCommand]
        private void CloseValidateCallQueueDialog()
        {
            ShowValidateCallQueueDialog = false;
        }

        [RelayCommand]
        private void OpenCreateCallTargetDialog()
        {
            ShowCreateCallTargetDialog = true;
        }

        [RelayCommand]
        private void CloseCreateCallTargetDialog()
        {
            ShowCreateCallTargetDialog = false;
        }

        [RelayCommand]
        private void OpenCreateDefaultCallFlowDialog()
        {
            ShowCreateDefaultCallFlowDialog = true;
        }

        [RelayCommand]
        private void CloseCreateDefaultCallFlowDialog()
        {
            ShowCreateDefaultCallFlowDialog = false;
        }

        [RelayCommand]
        private void OpenCreateAfterHoursCallFlowDialog()
        {
            ShowCreateAfterHoursCallFlowDialog = true;
        }

        [RelayCommand]
        private void CloseCreateAfterHoursCallFlowDialog()
        {
            ShowCreateAfterHoursCallFlowDialog = false;
        }

        [RelayCommand]
        private void OpenCreateAfterHoursScheduleDialog()
        {
            ShowCreateAfterHoursScheduleDialog = true;
        }

        [RelayCommand]
        private void CloseCreateAfterHoursScheduleDialog()
        {
            ShowCreateAfterHoursScheduleDialog = false;
        }

        [RelayCommand]
        private void OpenCreateCallHandlingAssociationDialog()
        {
            ShowCreateCallHandlingAssociationDialog = true;
        }

        [RelayCommand]
        private void CloseCreateCallHandlingAssociationDialog()
        {
            ShowCreateCallHandlingAssociationDialog = false;
        }

        [RelayCommand]
        private async Task CreateResourceAccountAsync()
        {
            if (string.IsNullOrWhiteSpace(ResourceAccountUpn) || string.IsNullOrWhiteSpace(ResourceAccountDisplayName))
            {
                StatusMessage = "Error: Resource account UPN and display name cannot be empty";
                return;
            }

            try
            {
                IsBusy = true;
                ShowCreateResourceAccountDialog = false;

                var variables = _sharedStateService?.Variables;
                if (variables == null)
                {
                    StatusMessage = "Error: Variables not found";
                    return;
                }

                if (string.IsNullOrWhiteSpace(variables.CsAppAaId))
                {
                    StatusMessage = "Error: Auto Attendant Application ID not found in variables";
                    return;
                }

                if (string.IsNullOrWhiteSpace(variables.MsFallbackDomain) || !variables.MsFallbackDomain.StartsWith("@"))
                {
                    StatusMessage = "Error: MS Fallback Domain is not set or invalid. Please set a valid domain (e.g., @yourdomain.com) in Variables.";
                    return;
                }

                // Build UPN: if user typed a value without @, append the domain
                var upn = ResourceAccountUpn;
                if (!upn.Contains("@"))
                {
                    upn = upn + variables.MsFallbackDomain;
                }

                var command = _powerShellCommandService.GetCreateAutoAttendantResourceAccountCommand(upn, ResourceAccountDisplayName, variables.CsAppAaId);
                var result = await PreviewAndExecuteAsync(command, "Create AA Resource Account");

                if (result == null)
                {
                    StatusMessage = "Operation cancelled by user";
                    return;
                }

                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Resource account '{ResourceAccountUpn}' created successfully";
                    _loggingService.Log($"Resource account {ResourceAccountUpn} created successfully", LogLevel.Info);
                    if (_sharedStateService?.AutoRefreshAfterOperations ?? true)
                        await RetrieveResourceAccountsAsync();
                }
                else
                {
                    StatusMessage = $"Error creating resource account: {result}";
                    _loggingService.Log($"Error creating resource account {ResourceAccountUpn}: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in CreateResourceAccountAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OpenUpdateUsageLocationDialog()
        {
            var variables = _sharedStateService?.Variables;
            if (variables != null)
            {
                ResourceAccountUpn = variables.RaaaUPN;
            }
            else
            {
                ResourceAccountUpn = "raaa-";
            }
            ShowUpdateUsageLocationDialog = true;
        }

        [RelayCommand]
        private void OpenCreateAutoAttendantDialog()
        {
            var variables = _sharedStateService?.Variables;
            if (variables != null && !string.IsNullOrEmpty(variables.Customer) && !string.IsNullOrEmpty(variables.CustomerGroupName))
            {
                AutoAttendantName = variables.AaDisplayName;
            }
            else
            {
                AutoAttendantName = "aa-";
            }
            ShowCreateAutoAttendantDialog = true;
        }

        [RelayCommand]
        private async Task CreateAutoAttendantAsync()
        {
            if (string.IsNullOrWhiteSpace(AutoAttendantName))
            {
                StatusMessage = "Error: Auto attendant name cannot be empty";
                return;
            }

            try
            {
                IsBusy = true;
                ShowCreateAutoAttendantDialog = false;

                var variables = _sharedStateService?.Variables;
                if (variables == null)
                {
                    StatusMessage = "Error: Variables not found";
                    return;
                }

                _loggingService.Log($"Creating auto attendant: {AutoAttendantName}", LogLevel.Info);

                var command = _powerShellCommandService.GetCreateSimpleAutoAttendantCommand(AutoAttendantName, variables.LanguageId, variables.TimeZoneId);
                var result = await PreviewAndExecuteAsync(command, "Create Auto Attendant");

                if (result == null)
                {
                    StatusMessage = "Operation cancelled by user";
                    return;
                }

                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Auto attendant '{AutoAttendantName}' created successfully";
                    _loggingService.Log($"Auto attendant {AutoAttendantName} created successfully", LogLevel.Info);
                    if (_sharedStateService?.AutoRefreshAfterOperations ?? true)
                        await RetrieveAutoAttendantsAsync();
                }
                else
                {
                    StatusMessage = $"Error creating auto attendant: {result}";
                    _loggingService.Log($"Error creating auto attendant {AutoAttendantName}: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in CreateAutoAttendantAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OpenAssociateDialog()
        {
            var variables = _sharedStateService?.Variables;
            if (variables != null)
            {
                ResourceAccountUpn = variables.RaaaUPN;
                AutoAttendantName = variables.AaDisplayName;
            }
            else
            {
                ResourceAccountUpn = "raaa-";
                AutoAttendantName = "aa-";
            }
            ShowAssociateDialog = true;
        }

        [RelayCommand]
        private async Task RemoveAutoAttendantAsync(string? aaName = null)
        {
            var name = aaName ?? AutoAttendantName;
            if (string.IsNullOrWhiteSpace(name))
            {
                StatusMessage = "Error: Auto attendant name cannot be empty";
                return;
            }

            try
            {
                IsBusy = true;

                var command = _powerShellCommandService.GetRemoveAutoAttendantCommand(name);
                var result = await ConfirmAndExecuteAsync(command,
                    $"This will permanently remove the auto attendant '{name}'. This action cannot be undone.",
                    "Remove Auto Attendant");

                if (result == null)
                {
                    StatusMessage = "Operation cancelled by user";
                    return;
                }

                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Auto attendant '{name}' removed successfully";
                    _loggingService.Log($"Auto attendant {name} removed successfully", LogLevel.Info);
                    if (_sharedStateService?.AutoRefreshAfterOperations ?? true)
                        await RetrieveAutoAttendantsAsync();
                }
                else
                {
                    StatusMessage = $"Error removing auto attendant: {result}";
                    _loggingService.Log($"Error removing auto attendant {name}: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in RemoveAutoAttendantAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RemoveScheduleAsync(string scheduleName)
        {
            if (string.IsNullOrWhiteSpace(scheduleName))
            {
                StatusMessage = "Error: Schedule name cannot be empty";
                return;
            }

            try
            {
                IsBusy = true;

                var command = _powerShellCommandService.GetRemoveScheduleCommand(scheduleName);
                var result = await ConfirmAndExecuteAsync(command,
                    $"This will permanently remove the schedule '{scheduleName}'. This action cannot be undone.",
                    "Remove Schedule");

                if (result == null)
                {
                    StatusMessage = "Operation cancelled by user";
                    return;
                }

                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Schedule '{scheduleName}' removed successfully";
                    _loggingService.Log($"Schedule {scheduleName} removed successfully", LogLevel.Info);
                }
                else
                {
                    StatusMessage = $"Error removing schedule: {result}";
                    _loggingService.Log($"Error removing schedule {scheduleName}: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in RemoveScheduleAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task RemoveResourceAccountAsync(string? upn = null)
        {
            var accountUpn = upn ?? ResourceAccountUpn;
            if (string.IsNullOrWhiteSpace(accountUpn))
            {
                StatusMessage = "Error: Resource account UPN cannot be empty";
                return;
            }

            try
            {
                IsBusy = true;

                var command = _powerShellCommandService.GetRemoveResourceAccountCommand(accountUpn);
                var result = await ConfirmAndExecuteAsync(command,
                    $"This will permanently remove the resource account '{accountUpn}'. This action cannot be undone.",
                    "Remove Resource Account");

                if (result == null)
                {
                    StatusMessage = "Operation cancelled by user";
                    return;
                }

                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Resource account '{accountUpn}' removed successfully";
                    _loggingService.Log($"Resource account {accountUpn} removed successfully", LogLevel.Info);
                    if (_sharedStateService?.AutoRefreshAfterOperations ?? true)
                        await RetrieveResourceAccountsAsync();
                }
                else
                {
                    StatusMessage = $"Error removing resource account: {result}";
                    _loggingService.Log($"Error removing resource account {accountUpn}: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in RemoveResourceAccountAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ParseResourceAccountsFromResult(string result)
        {
            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                if (line.StartsWith("RESOURCEACCOUNT:") && line.Length > 16)
                {
                    var accountData = line.Substring(16).Split('|');
                    if (accountData.Length >= 4)
                    {
                        var account = new ResourceAccount(
                            accountData[0].Trim(),
                            accountData[1].Trim(),
                            accountData[2].Trim(),
                            accountData[3].Trim()
                        );
                        ResourceAccounts.Add(account);
                    }
                }
            }
            OnPropertyChanged(nameof(ResourceAccountsView));
        }

        private void ParseAutoAttendantsFromResult(string result)
        {
            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                if (line.StartsWith("AUTOATTENDANT:") && line.Length > 14)
                {
                    var aaData = line.Substring(14).Split('|');
                    if (aaData.Length >= 4)
                    {
                        var autoAttendant = new AutoAttendant(
                            aaData[0].Trim(),
                            aaData[1].Trim(),
                            aaData[2].Trim(),
                            aaData[3].Trim()
                        );
                        AutoAttendants.Add(autoAttendant);
                    }
                }
            }
            OnPropertyChanged(nameof(AutoAttendantsView));
        }

        private bool FilterResourceAccount(object obj)
        {
            if (obj is not ResourceAccount account)
                return false;

            if (string.IsNullOrWhiteSpace(SearchResourceAccountsText))
                return true;

            var query = SearchResourceAccountsText.Trim();
            return (account.DisplayName?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                || (account.UserPrincipalName?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                || (account.Identity?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                || (account.UsageLocation?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool FilterAutoAttendant(object obj)
        {
            if (obj is not AutoAttendant autoAttendant)
                return false;

            if (string.IsNullOrWhiteSpace(SearchAutoAttendantsText))
                return true;

            var query = SearchAutoAttendantsText.Trim();
            return (autoAttendant.Name?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                || (autoAttendant.Identity?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                || (autoAttendant.LanguageId?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                || (autoAttendant.TimeZoneId?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        partial void OnSearchResourceAccountsTextChanged(string value)
        {
            OnPropertyChanged(nameof(ResourceAccountsView));
        }

        partial void OnSearchAutoAttendantsTextChanged(string value)
        {
            OnPropertyChanged(nameof(AutoAttendantsView));
        }
    }
} 
