using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using teams_phonemanager.Services;

namespace teams_phonemanager.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        protected readonly PowerShellContextService _powerShellContextService;
        protected readonly PowerShellCommandService _powerShellCommandService;
        protected readonly LoggingService _loggingService;
        protected readonly SessionManager _sessionManager;
        protected readonly NavigationService _navigationService;
        protected readonly ErrorHandlingService _errorHandlingService;
        protected readonly ValidationService _validationService;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private string _logMessage = string.Empty;

        protected ViewModelBase()
        {
            _powerShellContextService = PowerShellContextService.Instance;
            _powerShellCommandService = PowerShellCommandService.Instance;
            _loggingService = LoggingService.Instance;
            _sessionManager = SessionManager.Instance;
            _navigationService = NavigationService.Instance;
            _errorHandlingService = ErrorHandlingService.Instance;
            _validationService = ValidationService.Instance;
        }

        protected void UpdateStatus(string message)
        {
            StatusMessage = message;
            LogMessage = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        protected async Task<string> ExecutePowerShellCommandAsync(string command, string context = "")
        {
            try
            {
                _loggingService.Log($"Executing PowerShell command in {context}", LogLevel.Info);
                
                // Check connection status before executing
                var connectionStatus = await _powerShellContextService.GetConnectionStatusAsync();
                _loggingService.Log($"Connection status before command: {connectionStatus}", LogLevel.Info);
                
                var result = await _powerShellContextService.ExecuteCommandAsync(command);
                
                if (result.Contains("ERROR:"))
                {
                    _errorHandlingService.HandlePowerShellError(command, result, context);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _errorHandlingService.HandlePowerShellError(command, ex.Message, context);
                return $"ERROR: {ex.Message}";
            }
        }

        protected bool ValidatePrerequisites()
        {
            var validationResult = _validationService.ValidatePrerequisites();
            if (!validationResult.IsValid)
            {
                _errorHandlingService.HandleValidationError(validationResult.GetErrorMessage(), GetType().Name);
                return false;
            }
            return true;
        }

        protected bool ValidateVariables(Models.PhoneManagerVariables variables)
        {
            var validationResult = _validationService.ValidateVariables(variables);
            if (!validationResult.IsValid)
            {
                _errorHandlingService.HandleValidationError(validationResult.GetErrorMessage(), GetType().Name);
                return false;
            }
            return true;
        }

        protected void NavigateTo(string page)
        {
            _navigationService.NavigateTo(page);
        }

        protected void NavigateToVariables()
        {
            _navigationService.NavigateToVariables();
        }

        protected void NavigateToM365Groups()
        {
            _navigationService.NavigateToM365Groups();
        }

        protected void NavigateToCallQueues()
        {
            _navigationService.NavigateToCallQueues();
        }

        protected void NavigateToAutoAttendants()
        {
            _navigationService.NavigateToAutoAttendants();
        }

        protected void NavigateToHolidays()
        {
            _navigationService.NavigateToHolidays();
        }

        protected void NavigateToGetStarted()
        {
            _navigationService.NavigateToGetStarted();
        }

        protected void NavigateToWelcome()
        {
            _navigationService.NavigateToWelcome();
        }
    }
} 
