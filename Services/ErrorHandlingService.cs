using Avalonia.Controls;
using Material.Dialog;
using Material.Dialog.Icons;

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

        public async Task HandlePowerShellError(string command, string error, string context = "")
        {
            var cleanCommand = command?.Replace("\r", "").Replace("\n", " ") ?? "";
            var message = $"PowerShell Error in {context}:\nCommand: {cleanCommand}\nError: {error}";
            LoggingService.Instance.Log(message, LogLevel.Error);
            
            var window = GetMainWindow();
            if (window != null)
            {
                await DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams
                {
                    ContentHeader = ConstantsService.ErrorDialogTitles.PowerShellError,
                    SupportingText = $"An error occurred while executing PowerShell command:\n\n{error}",
                    DialogIcon = DialogIconKind.Error,
                    DialogHeaderIcon = DialogIconKind.Error,
                    StartupLocation = WindowStartupLocation.CenterOwner,
                    WindowTitle = ConstantsService.ErrorDialogTitles.PowerShellError
                }).ShowDialog(window);
            }
        }

        public async Task HandleValidationError(string message, string context = "")
        {
            var fullMessage = $"Validation Error in {context}: {message}";
            LoggingService.Instance.Log(fullMessage, LogLevel.Warning);
            
            var window = GetMainWindow();
            if (window != null)
            {
                await DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams
                {
                    ContentHeader = ConstantsService.ErrorDialogTitles.ValidationError,
                    SupportingText = message,
                    DialogIcon = DialogIconKind.Warning,
                    DialogHeaderIcon = DialogIconKind.Warning,
                    StartupLocation = WindowStartupLocation.CenterOwner,
                    WindowTitle = ConstantsService.ErrorDialogTitles.ValidationError
                }).ShowDialog(window);
            }
        }

        public async Task HandleConnectionError(string service, string error)
        {
            var message = $"Failed to connect to {service}: {error}";
            LoggingService.Instance.Log(message, LogLevel.Error);
            
            var window = GetMainWindow();
            if (window != null)
            {
                await DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams
                {
                    ContentHeader = ConstantsService.ErrorDialogTitles.ConnectionError,
                    SupportingText = $"Failed to connect to {service}:\n\n{error}",
                    DialogIcon = DialogIconKind.Error,
                    DialogHeaderIcon = DialogIconKind.Error,
                    StartupLocation = WindowStartupLocation.CenterOwner,
                    WindowTitle = ConstantsService.ErrorDialogTitles.ConnectionError
                }).ShowDialog(window);
            }
        }

        public async Task HandleGenericError(string message, string context = "")
        {
            var fullMessage = $"Error in {context}: {message}";
            LoggingService.Instance.Log(fullMessage, LogLevel.Error);
            
            var window = GetMainWindow();
            if (window != null)
            {
                await DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams
                {
                    ContentHeader = ConstantsService.ErrorDialogTitles.Error,
                    SupportingText = message,
                    DialogIcon = DialogIconKind.Error,
                    DialogHeaderIcon = DialogIconKind.Error,
                    StartupLocation = WindowStartupLocation.CenterOwner,
                    WindowTitle = ConstantsService.ErrorDialogTitles.Error
                }).ShowDialog(window);
            }
        }

        public async Task<bool> HandleConfirmation(string message, string title = ConstantsService.ErrorDialogTitles.Confirmation)
        {
            LoggingService.Instance.Log($"User confirmation requested: {title} - {message}", LogLevel.Info);
            
            var window = GetMainWindow();
            if (window != null)
            {
                var result = await DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams
                {
                    ContentHeader = title,
                    SupportingText = message,
                    DialogIcon = DialogIconKind.Help,
                    DialogHeaderIcon = DialogIconKind.Help,
                    StartupLocation = WindowStartupLocation.CenterOwner,
                    WindowTitle = title
                }).ShowDialog(window);
                
                // For confirmation dialogs, we'll use a simple approach - user can close with OK
                return result != null;
            }
            return false;
        }

        public async Task ShowSuccess(string message, string title = ConstantsService.ErrorDialogTitles.Success)
        {
            LoggingService.Instance.Log($"Success: {title} - {message}", LogLevel.Success);
            
            var window = GetMainWindow();
            if (window != null)
            {
                await DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams
                {
                    ContentHeader = title,
                    SupportingText = message,
                    DialogIcon = DialogIconKind.Success,
                    DialogHeaderIcon = DialogIconKind.Success,
                    StartupLocation = WindowStartupLocation.CenterOwner,
                    WindowTitle = title
                }).ShowDialog(window);
            }
        }

        public async Task ShowInfo(string message, string title = ConstantsService.ErrorDialogTitles.Information)
        {
            LoggingService.Instance.Log($"Info: {title} - {message}", LogLevel.Info);
            
            var window = GetMainWindow();
            if (window != null)
            {
                await DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams
                {
                    ContentHeader = title,
                    SupportingText = message,
                    DialogIcon = DialogIconKind.Info,
                    DialogHeaderIcon = DialogIconKind.Info,
                    StartupLocation = WindowStartupLocation.CenterOwner,
                    WindowTitle = title
                }).ShowDialog(window);
            }
        }
    }
}
