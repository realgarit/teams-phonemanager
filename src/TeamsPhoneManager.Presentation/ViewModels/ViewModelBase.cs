using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Threading;
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

        /// <summary>True while a cancellable PowerShell operation is running; drives the Cancel affordance.</summary>
        [ObservableProperty]
        private bool _isCancellable;

        /// <summary>Determinate progress value (0..100). Only meaningful when <see cref="IsProgressIndeterminate"/> is false.</summary>
        [ObservableProperty]
        private double _progressValue;

        /// <summary>True when the running operation reports no percentage and the UI should show an indeterminate bar.</summary>
        [ObservableProperty]
        private bool _isProgressIndeterminate = true;

        /// <summary>Human-readable progress line surfaced from native PowerShell <c>Write-Progress</c> records.</summary>
        [ObservableProperty]
        private string _progressText = string.Empty;

        /// <summary>
        /// Token source for the operation currently in flight. Cancelling it stops the PowerShell pipeline
        /// cooperatively (the runspace stays reusable). Null when no operation is running.
        /// </summary>
        private CancellationTokenSource? _operationCts;

        partial void OnIsBusyChanged(bool value)
        {
            if (!value)
            {
                WaitingMessage = string.Empty;
            }
        }

        /// <summary>
        /// Requests cancellation of the operation currently in flight. Safe to call when nothing is running.
        /// Cancellation is cooperative: the PowerShell pipeline is stopped and the persistent runspace is
        /// left open for the next command.
        /// </summary>
        [RelayCommand]
        protected void CancelOperation()
        {
            if (_operationCts is { IsCancellationRequested: false })
            {
                _loggingService.Log("Cancellation requested by user", LogLevel.Info);
                StatusMessage = "Cancelling…";
                _operationCts.Cancel();
            }
        }

        private void ResetProgress()
        {
            ProgressValue = 0;
            IsProgressIndeterminate = true;
            ProgressText = string.Empty;
        }

        private void OnPowerShellProgress(PowerShellProgress update)
        {
            IsProgressIndeterminate = update.IsIndeterminate;
            ProgressValue = update.IsIndeterminate ? 0 : update.PercentComplete;
            ProgressText = update.DisplayText;
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

        protected async Task<OperationResult<string>> ExecutePowerShellCommandAsync(string command, string context = "")
        {
            return await ExecutePowerShellCommandAsync(command, null, context);
        }

        protected async Task<OperationResult<string>> ExecutePowerShellCommandAsync(string command, Dictionary<string, string>? environmentVariables, string context = "")
        {
            // Check session expiry before executing commands that require a connection
            if (_sessionManager.IsSessionExpired && _sessionManager.IsSessionValid)
            {
                _sessionManager.ResetSession();
                await _errorHandlingService.HandleConnectionError(
                    "Session",
                    "Your session has expired (24h timeout). Please reconnect to Teams and Microsoft Graph.");
                return PowerShellOperationResultMapper.Failure(
                    OperationErrorCategory.AuthSession,
                    "Session expired. Please reconnect.",
                    "ERROR: Session expired. Please reconnect.");
            }

            using var cts = new CancellationTokenSource();
            _operationCts = cts;
            IsCancellable = true;
            ResetProgress();

            // Progress<T> marshals callbacks back onto the captured (UI) SynchronizationContext, so the
            // observable progress properties are always updated on the UI thread.
            var progress = new Progress<PowerShellProgress>(OnPowerShellProgress);

            try
            {
                var execution = await _powerShellContextService.ExecuteCommandWithDetailsAsync(command, environmentVariables, progress, cts.Token);
                var result = PowerShellOperationResultMapper.Map(execution);

                // Surface a user-facing error only for a reportable failure (error marker without success marker).
                if (result.ShouldReportError)
                {
                    await _errorHandlingService.HandlePowerShellError(command, result.RawOutput, context);
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                // User cancelled: this is not an error, so we deliberately do NOT raise the error dialog.
                // The runspace remains reusable, so the next operation runs normally.
                _loggingService.Log($"Operation cancelled: {context}", LogLevel.Warning);
                StatusMessage = "Operation cancelled.";
                return PowerShellOperationResultMapper.Failure(
                    OperationErrorCategory.Cancelled,
                    "Operation cancelled by user.",
                    "ERROR: Operation cancelled by user.");
            }
            catch (Exception ex)
            {
                await _errorHandlingService.HandlePowerShellError(command, ex.Message, context);
                return PowerShellOperationResultMapper.Failure(
                    OperationErrorCategory.Unknown,
                    ex.Message,
                    $"ERROR: {ex.Message}");
            }
            finally
            {
                _operationCts = null;
                IsCancellable = false;
                ResetProgress();
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

        /// <summary>
        /// Wraps an async operation with IsBusy tracking and error handling.
        /// Reduces the try/catch/finally boilerplate in every command method.
        /// </summary>
        protected async Task RunBusyAsync(Func<Task> action, string context = "", string? waitingMessage = null)
        {
            try
            {
                WaitingMessage = waitingMessage ?? string.Empty;
                IsBusy = true;
                await action();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                _loggingService.Log($"Exception in {context}: {ex}", LogLevel.Error);
            }
            finally
            {
                IsBusy = false;
            }
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
        protected async Task<OperationResult<string>?> PreviewAndExecuteAsync(string command, string context = "", Dictionary<string, string>? environmentVariables = null)
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
        protected async Task<OperationResult<string>?> ConfirmAndExecuteAsync(string command, string confirmMessage, string context = "", Dictionary<string, string>? environmentVariables = null)
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