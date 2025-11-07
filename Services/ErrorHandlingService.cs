using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace teams_phonemanager.Services
{
    public class ErrorHandlingService
    {
        private static ErrorHandlingService? _instance;

        private ErrorHandlingService() { }

        public static ErrorHandlingService Instance
        {
            get
            {
                _instance ??= new ErrorHandlingService();
                return _instance;
            }
        }

        private Window? GetMainWindow()
        {
            return Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
        }

        private async Task ShowContentDialogAsync(string title, string message, ContentDialogButton defaultButton = ContentDialogButton.Primary)
        {
            var window = GetMainWindow();
            if (window != null)
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    PrimaryButtonText = "OK",
                    DefaultButton = defaultButton
                };
                await dialog.ShowAsync(window);
            }
        }

        private async Task<bool> ShowConfirmationDialogAsync(string title, string message)
        {
            var window = GetMainWindow();
            if (window != null)
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    PrimaryButtonText = "OK",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary
                };
                var result = await dialog.ShowAsync(window);
                return result == ContentDialogResult.Primary;
            }
            return false;
        }

        public async Task HandlePowerShellError(string command, string error, string context = "")
        {
            var cleanCommand = command?.Replace("\r", "").Replace("\n", " ") ?? "";
            var message = $"PowerShell Error in {context}:\nCommand: {cleanCommand}\nError: {error}";
            LoggingService.Instance.Log(message, LogLevel.Error);
            
            await ShowContentDialogAsync(
                ConstantsService.ErrorDialogTitles.PowerShellError,
                $"An error occurred while executing PowerShell command:\n\n{error}"
            );
        }

        public async Task HandleValidationError(string message, string context = "")
        {
            var fullMessage = $"Validation Error in {context}: {message}";
            LoggingService.Instance.Log(fullMessage, LogLevel.Warning);
            
            await ShowContentDialogAsync(
                ConstantsService.ErrorDialogTitles.ValidationError,
                message
            );
        }

        public async Task HandleConnectionError(string service, string error)
        {
            var message = $"Failed to connect to {service}: {error}";
            LoggingService.Instance.Log(message, LogLevel.Error);
            
            await ShowContentDialogAsync(
                ConstantsService.ErrorDialogTitles.ConnectionError,
                $"Failed to connect to {service}:\n\n{error}"
            );
        }

        public async Task HandleGenericError(string message, string context = "")
        {
            var fullMessage = $"Error in {context}: {message}";
            LoggingService.Instance.Log(fullMessage, LogLevel.Error);
            
            await ShowContentDialogAsync(
                ConstantsService.ErrorDialogTitles.Error,
                message
            );
        }

        public async Task<bool> HandleConfirmation(string message, string title = ConstantsService.ErrorDialogTitles.Confirmation)
        {
            LoggingService.Instance.Log($"User confirmation requested: {title} - {message}", LogLevel.Info);
            return await ShowConfirmationDialogAsync(title, message);
        }

        public async Task ShowSuccess(string message, string title = ConstantsService.ErrorDialogTitles.Success)
        {
            LoggingService.Instance.Log($"Success: {title} - {message}", LogLevel.Success);
            await ShowContentDialogAsync(title, message);
        }

        public async Task ShowInfo(string message, string title = ConstantsService.ErrorDialogTitles.Information)
        {
            LoggingService.Instance.Log($"Info: {title} - {message}", LogLevel.Info);
            await ShowContentDialogAsync(title, message);
        }
    }
}
