using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    /// <summary>
    /// Service for handling and displaying errors.
    /// This service is now decoupled from UI framework dependencies via IDialogService.
    /// </summary>
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly ILoggingService _loggingService;
        private readonly IDialogService _dialogService;

        public ErrorHandlingService(ILoggingService loggingService, IDialogService dialogService)
        {
            _loggingService = loggingService;
            _dialogService = dialogService;
        }

        public async Task ShowContentDialogAsync(string title, string message, FluentAvalonia.UI.Controls.ContentDialogButton defaultButton = FluentAvalonia.UI.Controls.ContentDialogButton.Primary)
        {
            // Delegate to dialog service - defaultButton parameter kept for backward compatibility but ignored
            await _dialogService.ShowMessageAsync(title, message);
        }

        public async Task HandlePowerShellError(string command, string error, string context = "")
        {
            var cleanCommand = command?.Replace("\r", "").Replace("\n", " ") ?? "";
            var message = $"PowerShell Error in {context}:\nCommand: {cleanCommand}\nError: {error}";
            _loggingService.Log(message, LogLevel.Error);
            
            await _dialogService.ShowMessageAsync(
                ConstantsService.ErrorDialogTitles.PowerShellError,
                $"An error occurred while executing PowerShell command:\n\n{error}"
            );
        }

        public async Task HandleValidationError(string message, string context = "")
        {
            var fullMessage = $"Validation Error in {context}: {message}";
            _loggingService.Log(fullMessage, LogLevel.Warning);
            
            await _dialogService.ShowMessageAsync(
                ConstantsService.ErrorDialogTitles.ValidationError,
                message
            );
        }

        public async Task HandleConnectionError(string service, string error)
        {
            var message = $"Failed to connect to {service}: {error}";
            _loggingService.Log(message, LogLevel.Error);
            
            await _dialogService.ShowMessageAsync(
                ConstantsService.ErrorDialogTitles.ConnectionError,
                $"Failed to connect to {service}:\n\n{error}"
            );
        }

        public async Task HandleGenericError(string message, string context = "")
        {
            var fullMessage = $"Error in {context}: {message}";
            _loggingService.Log(fullMessage, LogLevel.Error);
            
            await _dialogService.ShowMessageAsync(
                ConstantsService.ErrorDialogTitles.Error,
                message
            );
        }

        public async Task<bool> HandleConfirmation(string message, string title = ConstantsService.ErrorDialogTitles.Confirmation)
        {
            _loggingService.Log($"User confirmation requested: {title} - {message}", LogLevel.Info);
            return await _dialogService.ShowConfirmationAsync(title, message);
        }

        public async Task ShowSuccess(string message, string title = ConstantsService.ErrorDialogTitles.Success)
        {
            _loggingService.Log($"Success: {title} - {message}", LogLevel.Success);
            await _dialogService.ShowMessageAsync(title, message);
        }

        public async Task ShowInfo(string message, string title = ConstantsService.ErrorDialogTitles.Information)
        {
            _loggingService.Log($"Info: {title} - {message}", LogLevel.Info);
            await _dialogService.ShowMessageAsync(title, message);
        }
    }
}
