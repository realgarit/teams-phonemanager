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
}
