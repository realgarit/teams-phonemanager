namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Service for handling and displaying errors.
/// </summary>
public interface IErrorHandlingService
{
    Task HandlePowerShellError(string command, string error, string context = "");
    Task HandleValidationError(string message, string context = "");
    Task HandleConnectionError(string service, string error);
    Task HandleGenericError(string message, string context = "");
    Task<bool> HandleConfirmation(string message, string title = "Confirmation");
    Task ShowSuccess(string message, string title = "Success");
    Task ShowInfo(string message, string title = "Information");
    Task ShowContentDialogAsync(string title, string message, FluentAvalonia.UI.Controls.ContentDialogButton defaultButton = FluentAvalonia.UI.Controls.ContentDialogButton.Primary);
}
