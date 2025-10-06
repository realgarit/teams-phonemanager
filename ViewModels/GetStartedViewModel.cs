using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using teams_phonemanager.Services;
using System.Threading.Tasks;
using System;

namespace teams_phonemanager.ViewModels
{
    public partial class GetStartedViewModel : ViewModelBase
    {
        [ObservableProperty]
        private bool _modulesChecked;

        [ObservableProperty]
        private bool _teamsConnected;

        [ObservableProperty]
        private bool _graphConnected;

        [ObservableProperty]
        private bool _canProceed;

        public bool CanConnectTeams => ModulesChecked && !TeamsConnected;
        public bool CanConnectGraph => ModulesChecked && !GraphConnected;

        public GetStartedViewModel()
        {
            _modulesChecked = _sessionManager.ModulesChecked;
            _teamsConnected = _sessionManager.TeamsConnected;
            _graphConnected = _sessionManager.GraphConnected;
            UpdateCanProceed();
            
            _loggingService.Log("Get Started page loaded", LogLevel.Info);
        }

        [RelayCommand]
        private void NavigateToPage(string page)
        {
            if (!CanProceed)
            {
                _loggingService.Log("Cannot navigate: prerequisites not met", LogLevel.Warning);
                return;
            }

            NavigateTo(page);
        }

        [RelayCommand]
        private async Task CheckModulesAsync()
        {
            try
            {
                IsBusy = true;
                _loggingService.Log("Checking PowerShell modules...", LogLevel.Info);

                var command = _powerShellCommandService.GetCheckModulesCommand();
                var result = await ExecutePowerShellCommandAsync(command, "CheckModules");
                
                ModulesChecked = (result.Contains("MicrosoftTeams module is available") || result.Contains("MicrosoftTeams module installed successfully")) && 
                                (result.Contains("Microsoft.Graph module is available") || result.Contains("Microsoft.Graph module installed successfully"));
                
                _sessionManager.UpdateModulesChecked(ModulesChecked);
                
                if (ModulesChecked)
                {
                    _loggingService.Log("PowerShell modules are available", LogLevel.Success);
                    foreach (var line in result.Split('\n'))
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            _loggingService.Log(line.Trim(), LogLevel.Info);
                        }
                    }
                }
                else
                {
                    var errorMessage = "One or more required modules could not be installed";
                    _loggingService.Log(errorMessage, LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error checking PowerShell modules: {ex.Message}", LogLevel.Error);
                ModulesChecked = false;
                _sessionManager.UpdateModulesChecked(false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ConnectTeamsAsync()
        {
            try
            {
                if (!ModulesChecked)
                {
                    _loggingService.Log("Please check modules first", LogLevel.Warning);
                    return;
                }

                IsBusy = true;
                _loggingService.Log("Connecting to Microsoft Teams...", LogLevel.Info);

                var command = _powerShellCommandService.GetConnectTeamsCommand();
                var result = await ExecutePowerShellCommandAsync(command, "ConnectTeams");
                TeamsConnected = result.Contains("SUCCESS:");
                
                string? tenantId = null;
                string? tenantName = null;
                
                if (TeamsConnected)
                {
                    _loggingService.Log("Connected to Microsoft Teams successfully", LogLevel.Success);
                    
                    foreach (var line in result.Split('\n'))
                    {
                        if (line.Contains("Connected to tenant:"))
                        {
                            var parts = line.Split(':')[1].Trim().Split('(');
                            if (parts.Length >= 2)
                            {
                                tenantName = parts[0].Trim();
                                tenantId = parts[1].Trim(')').Trim();
                            }
                        }
                    }
                }
                else
                {
                    _loggingService.Log($"Error connecting to Microsoft Teams: {result}", LogLevel.Error);
                }
                
                _sessionManager.UpdateTeamsConnection(TeamsConnected);
                if (tenantId != null && tenantName != null)
                {
                    _sessionManager.UpdateTeamsTenantInfo(tenantId, tenantName);
                }
                
                UpdateCanProceed();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error connecting to Microsoft Teams: {ex.Message}", LogLevel.Error);
                TeamsConnected = false;
                _sessionManager.UpdateTeamsConnection(false);
                UpdateCanProceed();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task ConnectGraphAsync()
        {
            try
            {
                if (!ModulesChecked)
                {
                    _loggingService.Log("Please check modules first", LogLevel.Warning);
                    return;
                }

                IsBusy = true;
                _loggingService.Log("Connecting to Microsoft Graph...", LogLevel.Info);

                var command = _powerShellCommandService.GetConnectGraphCommand();
                var result = await ExecutePowerShellCommandAsync(command, "ConnectGraph");
                GraphConnected = result.Contains("SUCCESS:");
                
                string? account = null;
                
                if (GraphConnected)
                {
                    _loggingService.Log($"Connected to Microsoft Graph successfully\n{result}", LogLevel.Success);
                    
                    foreach (var line in result.Split('\n'))
                    {
                        if (line.Contains("Connected as:"))
                        {
                            account = line.Split(':')[1].Trim();
                        }
                    }
                }
                else
                {
                    _loggingService.Log($"Error connecting to Microsoft Graph: {result}", LogLevel.Error);
                }
                
                _sessionManager.UpdateGraphConnection(GraphConnected, account);
                UpdateCanProceed();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error connecting to Microsoft Graph: {ex.Message}", LogLevel.Error);
                GraphConnected = false;
                _sessionManager.UpdateGraphConnection(false);
                UpdateCanProceed();
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DisconnectTeamsAsync()
        {
            try
            {
                IsBusy = true;
                _loggingService.Log("Disconnecting from Microsoft Teams...", LogLevel.Info);

                var command = _powerShellCommandService.GetDisconnectTeamsCommand();
                var result = await ExecutePowerShellCommandAsync(command, "DisconnectTeams");
                TeamsConnected = false;
                _sessionManager.UpdateTeamsConnection(false);
                _loggingService.Log("Disconnected from Microsoft Teams", LogLevel.Info);
                UpdateCanProceed();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error disconnecting from Microsoft Teams: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private async Task DisconnectGraphAsync()
        {
            try
            {
                IsBusy = true;
                _loggingService.Log("Disconnecting from Microsoft Graph...", LogLevel.Info);

                var command = _powerShellCommandService.GetDisconnectGraphCommand();
                var result = await ExecutePowerShellCommandAsync(command, "DisconnectGraph");
                GraphConnected = false;
                _sessionManager.UpdateGraphConnection(false);
                _loggingService.Log("Disconnected from Microsoft Graph", LogLevel.Info);
                UpdateCanProceed();
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error disconnecting from Microsoft Graph: {ex.Message}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateCanProceed()
        {
            CanProceed = ModulesChecked && TeamsConnected && GraphConnected;
        }

        partial void OnModulesCheckedChanged(bool value) 
        { 
            UpdateCanProceed();
            OnPropertyChanged(nameof(CanConnectTeams));
            OnPropertyChanged(nameof(CanConnectGraph));
        }
        
        partial void OnTeamsConnectedChanged(bool value) 
        { 
            UpdateCanProceed();
            OnPropertyChanged(nameof(CanConnectTeams));
        }
        
        partial void OnGraphConnectedChanged(bool value) 
        { 
            UpdateCanProceed();
            OnPropertyChanged(nameof(CanConnectGraph));
        }
    }
} 
