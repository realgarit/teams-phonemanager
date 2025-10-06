using System.Windows;

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

        public void HandlePowerShellError(string command, string error, string context = "")
        {
            var message = $"PowerShell Error in {context}:\nCommand: {command}\nError: {error}";
            LoggingService.Instance.Log(message, LogLevel.Error);
            
            MessageBox.Show(
                $"An error occurred while executing PowerShell command:\n\n{error}",
                "PowerShell Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public void HandleValidationError(string message, string context = "")
        {
            var fullMessage = $"Validation Error in {context}: {message}";
            LoggingService.Instance.Log(fullMessage, LogLevel.Warning);
            
            MessageBox.Show(
                message,
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        public void HandleConnectionError(string service, string error)
        {
            var message = $"Failed to connect to {service}: {error}";
            LoggingService.Instance.Log(message, LogLevel.Error);
            
            MessageBox.Show(
                $"Failed to connect to {service}:\n\n{error}",
                "Connection Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public void HandleGenericError(string message, string context = "")
        {
            var fullMessage = $"Error in {context}: {message}";
            LoggingService.Instance.Log(fullMessage, LogLevel.Error);
            
            MessageBox.Show(
                message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        public bool HandleConfirmation(string message, string title = "Confirmation")
        {
            LoggingService.Instance.Log($"User confirmation requested: {title} - {message}", LogLevel.Info);
            
            var result = MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            return result == MessageBoxResult.Yes;
        }

        public void ShowSuccess(string message, string title = "Success")
        {
            LoggingService.Instance.Log($"Success: {title} - {message}", LogLevel.Success);
            
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        public void ShowInfo(string message, string title = "Information")
        {
            LoggingService.Instance.Log($"Info: {title} - {message}", LogLevel.Info);
            
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}
