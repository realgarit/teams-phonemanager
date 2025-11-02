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
                var result = await _powerShellContextService.ExecuteCommandAsync(command);
                
                // Only treat as error if result contains ERROR: and not SUCCESS
                if (result.Contains("ERROR:") && !result.Contains("SUCCESS"))
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
            NavigateTo(ConstantsService.Pages.Variables);
        }

        protected void NavigateToM365Groups()
        {
            NavigateTo(ConstantsService.Pages.M365Groups);
        }

        protected void NavigateToCallQueues()
        {
            NavigateTo(ConstantsService.Pages.CallQueues);
        }

        protected void NavigateToAutoAttendants()
        {
            NavigateTo(ConstantsService.Pages.AutoAttendants);
        }

        protected void NavigateToHolidays()
        {
            NavigateTo(ConstantsService.Pages.Holidays);
        }

        protected void NavigateToGetStarted()
        {
            NavigateTo(ConstantsService.Pages.GetStarted);
        }

        protected void NavigateToWelcome()
        {
            NavigateTo(ConstantsService.Pages.Welcome);
        }
    }
}