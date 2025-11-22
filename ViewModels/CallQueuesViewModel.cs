using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using teams_phonemanager.Services;
using teams_phonemanager.Models;
using System;
using System.Threading.Tasks;
using Avalonia.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace teams_phonemanager.ViewModels
{
    public partial class CallQueuesViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<ResourceAccount> _resourceAccounts = new();

        [ObservableProperty]
        private ObservableCollection<CallQueue> _callQueues = new();

        [ObservableProperty]
        private string _searchResourceAccountsText = string.Empty;

        [ObservableProperty]
        private string _searchCallQueuesText = string.Empty;

        [ObservableProperty]
        private bool _showCreateResourceAccountDialog;

        [ObservableProperty]
        private bool _showUpdateUsageLocationDialog;

        [ObservableProperty]
        private bool _showCreateCallQueueDialog;

        [ObservableProperty]
        private bool _showAssociateDialog;

        [ObservableProperty]
        private string _resourceAccountUpn = string.Empty;

        [ObservableProperty]
        private string _resourceAccountDisplayName = string.Empty;

        [ObservableProperty]
        private string _callQueueName = string.Empty;

        [ObservableProperty]
        private string _m365GroupId = string.Empty;

        public ObservableCollection<ResourceAccount> ResourceAccountsView => new ObservableCollection<ResourceAccount>(
            ResourceAccounts.Where(FilterResourceAccount)
        );
        public ObservableCollection<CallQueue> CallQueuesView => new ObservableCollection<CallQueue>(
            CallQueues.Where(FilterCallQueue)
        );

        public CallQueuesViewModel()
        {
            _mainWindowViewModel = Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow?.DataContext as MainWindowViewModel
                : null;

            _loggingService.Log("Call Queues page loaded", LogLevel.Info);

            ResourceAccounts.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ResourceAccountsView));
            CallQueues.CollectionChanged += (s, e) => OnPropertyChanged(nameof(CallQueuesView));
        }

        [RelayCommand]
        private async Task RetrieveResourceAccountsAsync()
        {
            try
            {
                IsBusy = true;
                ResourceAccounts.Clear();
                StatusMessage = "Retrieving resource accounts...";

                _loggingService.Log("Retrieving resource accounts starting with 'racq-'", LogLevel.Info);

                var command = _powerShellCommandService.GetRetrieveResourceAccountsCommand();
                var result = await ExecutePowerShellCommandAsync(command, "RetrieveResourceAccounts");
                
                if (!string.IsNullOrEmpty(result))
                {
                    ParseResourceAccountsFromResult(result);
                    StatusMessage = $"Found {ResourceAccounts.Count} resource accounts starting with 'racq-'";
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
        private async Task RetrieveCallQueuesAsync()
        {
            try
            {
                IsBusy = true;
                CallQueues.Clear();
                StatusMessage = "Retrieving call queues...";

                _loggingService.Log("Retrieving call queues containing 'cq-'", LogLevel.Info);

                var command = _powerShellCommandService.GetRetrieveCallQueuesCommand();
                var result = await ExecutePowerShellCommandAsync(command, "RetrieveCallQueues");
                
                if (!string.IsNullOrEmpty(result))
                {
                    ParseCallQueuesFromResult(result);
                    StatusMessage = $"Found {CallQueues.Count} call queues containing 'cq-'";
                    _loggingService.Log($"Retrieved {CallQueues.Count} call queues", LogLevel.Info);
                }
                else
                {
                    StatusMessage = "Error: No output from PowerShell command";
                    _loggingService.Log("Error retrieving call queues: No output from PowerShell command", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in RetrieveCallQueuesAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OpenCreateResourceAccountDialog()
        {
            var variables = _mainWindowViewModel?.Variables;
            if (variables != null && !string.IsNullOrEmpty(variables.Customer) && !string.IsNullOrEmpty(variables.CustomerGroupName))
            {
                ResourceAccountUpn = variables.RacqUPN;
                ResourceAccountDisplayName = variables.RacqDisplayName;
            }
            else
            {
                ResourceAccountUpn = "racq-";
                ResourceAccountDisplayName = "racq-";
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
        private void CloseCreateCallQueueDialog()
        {
            ShowCreateCallQueueDialog = false;
        }

        [RelayCommand]
        private void CloseAssociateDialog()
        {
            ShowAssociateDialog = false;
        }

        [RelayCommand]
        private async Task AssignLicenseAsync()
        {
            var variables = _mainWindowViewModel?.Variables;
            if (variables == null)
            {
                StatusMessage = "Error: Variables not found";
                return;
            }

            if (string.IsNullOrWhiteSpace(variables.RacqUPN))
            {
                StatusMessage = "Error: Resource Account UPN is not set. Please set variables first.";
                return;
            }

            if (string.IsNullOrWhiteSpace(variables.SkuId))
            {
                StatusMessage = "Error: SKU ID is not set. Please set the SKU ID variable first.";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Assigning license to resource account...";

                var command = _powerShellCommandService.GetAssignLicenseCommand(variables.RacqUPN, variables.SkuId);
                var result = await ExecutePowerShellCommandAsync(command, "AssignLicense");
                
                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"License assigned to resource account '{variables.RacqUPN}' successfully";
                    _loggingService.Log($"License assigned to resource account {variables.RacqUPN}", LogLevel.Info);
                }
                else
                {
                    StatusMessage = $"Error assigning license: {result}";
                    _loggingService.Log($"Error assigning license to {variables.RacqUPN}: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in AssignLicenseAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task GetM365GroupIdAsync()
        {
            var variables = _mainWindowViewModel?.Variables;
            if (variables == null)
            {
                StatusMessage = "Error: Variables not found";
                return;
            }

            if (string.IsNullOrWhiteSpace(variables.M365Group))
            {
                StatusMessage = "Error: M365 Group name is not set. Please set Customer and Customer Group Name variables first.";
                return;
            }

            try
            {
                IsBusy = true;
                StatusMessage = "Retrieving M365 Group ID...";

                var command = _powerShellCommandService.GetM365GroupIdCommand(variables.M365Group);
                var result = await ExecutePowerShellCommandAsync(command, "GetM365GroupId");
                
                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    // Parse the group ID from the result
                    var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("M365GROUPID:") && line.Length > 12)
                        {
                            var groupId = line.Substring(12).Trim();
                            variables.M365GroupId = groupId;
                            StatusMessage = $"M365 Group ID retrieved and saved: {groupId}";
                            _loggingService.Log($"M365 Group ID saved: {groupId}", LogLevel.Info);
                            return;
                        }
                    }
                    StatusMessage = "Error: Could not parse M365 Group ID from result";
                }
                else
                {
                    StatusMessage = $"Error retrieving M365 Group ID: {result}";
                    _loggingService.Log($"Error retrieving M365 Group ID: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in GetM365GroupIdAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
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

                var variables = _mainWindowViewModel?.Variables;
                if (variables == null)
                {
                    StatusMessage = "Error: Variables not found";
                    return;
                }

                if (string.IsNullOrWhiteSpace(variables.CsAppCqId))
                {
                    StatusMessage = "Error: Call Queue Application ID not found in variables";
                    return;
                }

                // Validate that the UPN includes a domain
                if (string.IsNullOrWhiteSpace(variables.MsFallbackDomain) || !variables.MsFallbackDomain.StartsWith("@"))
                {
                    StatusMessage = "Error: MS Fallback Domain is not set or invalid. Please set a valid domain (e.g., @yourdomain.com) in Variables.";
                    return;
                }

                if (!variables.RacqUPN.Contains("@"))
                {
                    StatusMessage = "Error: Resource Account UPN must include a domain name. Please check your MS Fallback Domain setting.";
                    return;
                }

                var command = _powerShellCommandService.GetCreateResourceAccountCommand(variables);
                var result = await ExecutePowerShellCommandAsync(command, "CreateResourceAccount");
                
                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Resource account '{ResourceAccountUpn}' created successfully";
                    _loggingService.Log($"Resource account {ResourceAccountUpn} created successfully", LogLevel.Info);
                    // Don't auto-refresh to avoid showing "Found ... resource accounts" message
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
            var variables = _mainWindowViewModel?.Variables;
            if (variables != null)
            {
                ResourceAccountUpn = variables.RacqUPN;
            }
            else
            {
                ResourceAccountUpn = "racq-";
            }
            ShowUpdateUsageLocationDialog = true;
        }

        [RelayCommand]
        private async Task UpdateUsageLocationAsync()
        {
            if (string.IsNullOrWhiteSpace(ResourceAccountUpn))
            {
                StatusMessage = "Error: Resource account UPN cannot be empty";
                return;
            }

            try
            {
                IsBusy = true;
                ShowUpdateUsageLocationDialog = false;

                var variables = _mainWindowViewModel?.Variables;
                if (variables == null)
                {
                    StatusMessage = "Error: Variables not found";
                    return;
                }

                _loggingService.Log($"Updating usage location for: {ResourceAccountUpn}", LogLevel.Info);

                var command = _powerShellCommandService.GetUpdateResourceAccountUsageLocationCommand(ResourceAccountUpn, variables.UsageLocation);
                var result = await ExecutePowerShellCommandAsync(command, "UpdateUsageLocation");
                
                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Usage location updated for '{ResourceAccountUpn}' to '{variables.UsageLocation}'";
                    _loggingService.Log($"Usage location updated for {ResourceAccountUpn}", LogLevel.Info);
                    // Don't auto-refresh to avoid showing "Found ... resource accounts" message
                }
                else
                {
                    StatusMessage = $"Error updating usage location: {result}";
                    _loggingService.Log($"Error updating usage location for {ResourceAccountUpn}: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in UpdateUsageLocationAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OpenCreateCallQueueDialog()
        {
            var variables = _mainWindowViewModel?.Variables;
            if (variables != null && !string.IsNullOrEmpty(variables.Customer) && !string.IsNullOrEmpty(variables.CustomerGroupName))
            {
                CallQueueName = variables.CqDisplayName;
                M365GroupId = variables.M365GroupId; // Use the actual group ID instead of name
            }
            else
            {
                CallQueueName = "cq-";
                M365GroupId = "";
            }
            ShowCreateCallQueueDialog = true;
        }

        [RelayCommand]
        private async Task CreateCallQueueAsync()
        {
            if (string.IsNullOrWhiteSpace(CallQueueName))
            {
                StatusMessage = "Error: Call queue name cannot be empty";
                return;
            }

            try
            {
                IsBusy = true;
                ShowCreateCallQueueDialog = false;

                var variables = _mainWindowViewModel?.Variables;
                if (variables == null)
                {
                    StatusMessage = "Error: Variables not found";
                    return;
                }

                _loggingService.Log($"Creating call queue: {CallQueueName}", LogLevel.Info);

                var command = _powerShellCommandService.GetCreateCallQueueCommand(CallQueueName, variables.LanguageId, variables.M365GroupId, variables);
                var result = await ExecutePowerShellCommandAsync(command, "CreateCallQueue");
                
                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Call queue '{CallQueueName}' created successfully";
                    _loggingService.Log($"Call queue {CallQueueName} created successfully", LogLevel.Info);
                    // Don't auto-refresh to avoid showing "Found ... call queues" message
                }
                else
                {
                    StatusMessage = $"Error creating call queue: {result}";
                    _loggingService.Log($"Error creating call queue {CallQueueName}: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in CreateCallQueueAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private void OpenAssociateDialog()
        {
            var variables = _mainWindowViewModel?.Variables;
            if (variables != null)
            {
                ResourceAccountUpn = variables.RacqUPN;
                CallQueueName = variables.CqDisplayName;
            }
            else
            {
                ResourceAccountUpn = "racq-";
                CallQueueName = "cq-";
            }
            ShowAssociateDialog = true;
        }

        [RelayCommand]
        private async Task AssociateResourceAccountWithCallQueueAsync()
        {
            if (string.IsNullOrWhiteSpace(ResourceAccountUpn) || string.IsNullOrWhiteSpace(CallQueueName))
            {
                StatusMessage = "Error: Resource account UPN and call queue name cannot be empty";
                return;
            }

            try
            {
                IsBusy = true;
                ShowAssociateDialog = false;

                _loggingService.Log($"Associating resource account {ResourceAccountUpn} with call queue {CallQueueName}", LogLevel.Info);

                var command = _powerShellCommandService.GetAssociateResourceAccountWithCallQueueCommand(ResourceAccountUpn, CallQueueName);
                var result = await ExecutePowerShellCommandAsync(command, "AssociateResourceAccountWithCallQueue");
                
                if (!string.IsNullOrEmpty(result) && result.Contains("SUCCESS"))
                {
                    StatusMessage = $"Successfully associated resource account '{ResourceAccountUpn}' with call queue '{CallQueueName}'";
                    _loggingService.Log($"Successfully associated resource account {ResourceAccountUpn} with call queue {CallQueueName}", LogLevel.Info);
                }
                else
                {
                    StatusMessage = $"Error associating resource account with call queue: {result}";
                    _loggingService.Log($"Error associating resource account {ResourceAccountUpn} with call queue {CallQueueName}: {result}", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in AssociateResourceAccountWithCallQueueAsync: {ex}", LogLevel.Error);
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

        private void ParseCallQueuesFromResult(string result)
        {
            var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                if (line.StartsWith("CALLQUEUE:") && line.Length > 10)
                {
                    var queueData = line.Substring(10).Split('|');
                    if (queueData.Length >= 4)
                    {
                        var queue = new CallQueue(
                            queueData[0].Trim(),
                            queueData[1].Trim(),
                            queueData[2].Trim(),
                            int.TryParse(queueData[3].Trim(), out var alertTime) ? alertTime : 0
                        );
                        CallQueues.Add(queue);
                    }
                }
            }
            OnPropertyChanged(nameof(CallQueuesView));
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

        private bool FilterCallQueue(object obj)
        {
            if (obj is not CallQueue queue)
                return false;

            if (string.IsNullOrWhiteSpace(SearchCallQueuesText))
                return true;

            var query = SearchCallQueuesText.Trim();
            return (queue.Name?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                || (queue.Identity?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                || (queue.RoutingMethod?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        partial void OnSearchResourceAccountsTextChanged(string value)
        {
            OnPropertyChanged(nameof(ResourceAccountsView));
        }

        partial void OnSearchCallQueuesTextChanged(string value)
        {
            OnPropertyChanged(nameof(CallQueuesView));
        }
    }
} 
