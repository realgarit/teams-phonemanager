using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using teams_phonemanager.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace teams_phonemanager.ViewModels
{
    public partial class CallQueuesViewModel : ViewModelBase
    {
        private readonly MainWindowViewModel? _mainWindowViewModel;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to the Call Queues page. This page will help you create the call queue for your phone system.";

        [ObservableProperty]
        private bool _isQueueCreated;

        [ObservableProperty]
        private string _queueStatus = string.Empty;

        [ObservableProperty]
        private bool _showConfirmation;

        [ObservableProperty]
        private string? _waitingMessage = string.Empty;

        public CallQueuesViewModel()
        {
            _mainWindowViewModel = Application.Current.MainWindow.DataContext as MainWindowViewModel;

            _loggingService.Log("Call Queues page loaded", LogLevel.Info);
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
        private async Task CreateCallQueueAsync()
        {
            try
            {
                IsBusy = true;
                ShowConfirmation = false;

                var variables = _mainWindowViewModel?.Variables;
                if (variables == null)
                {
                    _loggingService.Log("Variables not found. Please set variables first.", LogLevel.Error);
                    QueueStatus = "Error: Variables not found. Please set variables first.";
                    return;
                }

                if (!ValidateVariables(variables))
                {
                    return;
                }

                _loggingService.Log($"Creating Call Queue: {variables.CqDisplayName}", LogLevel.Info);
                WaitingMessage = "Creating call queue. This process takes approximately 4 minutes due to Microsoft's background replication. Please be patient.";

                var command = _powerShellCommandService.GetCreateCallQueueCommand(variables);
                var result = await ExecutePowerShellCommandAsync(command, "CreateCallQueue");
                
                if (!string.IsNullOrEmpty(result))
                {
                    QueueStatus = "Call Queue created successfully";
                    IsQueueCreated = true;
                    WaitingMessage = string.Empty;
                    _loggingService.Log($"Call Queue {variables.CqDisplayName} created successfully", LogLevel.Info);
                }
                else
                {
                    QueueStatus = "Error: No output from PowerShell command";
                    WaitingMessage = string.Empty;
                    _loggingService.Log($"Error creating Call Queue {variables.CqDisplayName}: No output from PowerShell command", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                QueueStatus = $"Error: {ex.Message}";
                WaitingMessage = string.Empty;
                _loggingService.Log($"Exception in CreateCallQueueAsync: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
} 
