namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Service for displaying dialogs to the user.
/// This interface abstracts UI framework dependencies, allowing for better testability and separation of concerns.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a message dialog to the user.
    /// </summary>
    Task ShowMessageAsync(string title, string message);

    /// <summary>
    /// Shows a confirmation dialog and returns the user's choice.
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message);

    /// <summary>
    /// Shows a script preview dialog. Returns true if the user wants to execute, false to cancel.
    /// </summary>
    Task<bool> ShowScriptPreviewAsync(string title, string script);

    /// <summary>
    /// Shows a confirmation dialog with a script preview for destructive operations.
    /// Returns true if the user confirms execution, false to cancel.
    /// </summary>
    Task<bool> ShowConfirmationWithPreviewAsync(string title, string message, string script);
}
