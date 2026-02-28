using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using teams_phonemanager.Services;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.ViewModels
{
    public partial class ViewModelBase : ObservableObject
    {
        protected readonly IPowerShellContextService _powerShellContextService;
        protected readonly IPowerShellCommandService _powerShellCommandService;
        protected readonly ILoggingService _loggingService;
        protected readonly ISessionManager _sessionManager;
        protected readonly INavigationService _navigationService;
        protected readonly IErrorHandlingService _errorHandlingService;
        protected readonly IValidationService _validationService;
        protected readonly ISharedStateService? _sharedStateService;
        protected readonly IDialogService? _dialogService;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _waitingMessage = string.Empty;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private string _logMessage = string.Empty;

        partial void OnIsBusyChanged(bool value)
        {
            if (!value)
            {
                WaitingMessage = string.Empty;
            }
        }

        protected ViewModelBase(
            IPowerShellContextService powerShellContextService,
            IPowerShellCommandService powerShellCommandService,
            ILoggingService loggingService,
            ISessionManager sessionManager,
            INavigationService navigationService,
            IErrorHandlingService errorHandlingService,
            IValidationService validationService,
            ISharedStateService? sharedStateService = null,
            IDialogService? dialogService = null)
        {
            _powerShellContextService = powerShellContextService;
            _powerShellCommandService = powerShellCommandService;
            _loggingService = loggingService;
            _sessionManager = sessionManager;
            _navigationService = navigationService;
            _errorHandlingService = errorHandlingService;
            _validationService = validationService;
            _sharedStateService = sharedStateService;
            _dialogService = dialogService;
        }

        protected void UpdateStatus(string message)
        {
            StatusMessage = message;
            LogMessage = $"{DateTime.Now:HH:mm:ss} - {message}";
        }

        protected async Task<string> ExecutePowerShellCommandAsync(string command, string context = "")
        {
            return await ExecutePowerShellCommandAsync(command, null, context);
        }

        protected async Task<string> ExecutePowerShellCommandAsync(string command, Dictionary<string, string>? environmentVariables, string context = "")
        {
            // Check session expiry before executing commands that require a connection
            if (_sessionManager.IsSessionExpired && _sessionManager.IsSessionValid)
            {
                _sessionManager.ResetSession();
                await _errorHandlingService.HandleConnectionError(
                    "Session",
                    "Your session has expired (24h timeout). Please reconnect to Teams and Microsoft Graph.");
                return "ERROR: Session expired. Please reconnect.";
            }

            try
            {
                var result = await _powerShellContextService.ExecuteCommandAsync(command, environmentVariables);

                // Only treat as error if result contains ERROR: and not SUCCESS
                if (result.Contains("ERROR:") && !result.Contains("SUCCESS"))
                {
                    await _errorHandlingService.HandlePowerShellError(command, result, context);
                }

                return result;
            }
            catch (Exception ex)
            {
                await _errorHandlingService.HandlePowerShellError(command, ex.Message, context);
                return $"ERROR: {ex.Message}";
            }
        }

        protected async Task<bool> ValidatePrerequisites()
        {
            var validationResult = _validationService.ValidatePrerequisites();
            if (!validationResult.IsValid)
            {
                await _errorHandlingService.HandleValidationError(validationResult.GetErrorMessage(), GetType().Name);
                return false;
            }
            return true;
        }

        protected async Task<bool> ValidateVariables(Models.PhoneManagerVariables variables)
        {
            var validationResult = _validationService.ValidateVariables(variables);
            if (!validationResult.IsValid)
            {
                await _errorHandlingService.HandleValidationError(validationResult.GetErrorMessage(), GetType().Name);
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
            NavigateTo(Services.ConstantsService.Pages.Variables);
        }

        protected void NavigateToM365Groups()
        {
            NavigateTo(Services.ConstantsService.Pages.M365Groups);
        }

        protected void NavigateToCallQueues()
        {
            NavigateTo(Services.ConstantsService.Pages.CallQueues);
        }

        protected void NavigateToAutoAttendants()
        {
            NavigateTo(Services.ConstantsService.Pages.AutoAttendants);
        }

        protected void NavigateToHolidays()
        {
            NavigateTo(Services.ConstantsService.Pages.Holidays);
        }

        protected void NavigateToGetStarted()
        {
            NavigateTo(Services.ConstantsService.Pages.GetStarted);
        }

        protected void NavigateToWelcome()
        {
            NavigateTo(Services.ConstantsService.Pages.Welcome);
        }

        /// <summary>
        /// Shows a script preview dialog and executes the command if the user confirms.
        /// Returns the execution result, or null if the user cancelled.
        /// </summary>
        protected async Task<string?> PreviewAndExecuteAsync(string command, string context = "", Dictionary<string, string>? environmentVariables = null)
        {
            if (_dialogService != null && !(_sharedStateService?.SkipScriptPreview ?? false))
            {
                var confirmed = await _dialogService.ShowScriptPreviewAsync($"Preview: {context}", command);
                if (!confirmed)
                {
                    _loggingService.Log($"User cancelled script preview for: {context}", LogLevel.Info);
                    return null;
                }
            }

            return await ExecutePowerShellCommandAsync(command, environmentVariables, context);
        }

        /// <summary>
        /// Shows a confirmation dialog with script preview for destructive operations.
        /// Returns the execution result, or null if the user cancelled.
        /// </summary>
        protected async Task<string?> ConfirmAndExecuteAsync(string command, string confirmMessage, string context = "", Dictionary<string, string>? environmentVariables = null)
        {
            if (_dialogService != null && !(_sharedStateService?.SkipDeleteConfirmation ?? false))
            {
                var confirmed = await _dialogService.ShowConfirmationWithPreviewAsync(
                    $"Confirm: {context}",
                    confirmMessage,
                    command);
                if (!confirmed)
                {
                    _loggingService.Log($"User cancelled destructive operation: {context}", LogLevel.Info);
                    return null;
                }
            }

            return await ExecutePowerShellCommandAsync(command, environmentVariables, context);
        }
    }
}