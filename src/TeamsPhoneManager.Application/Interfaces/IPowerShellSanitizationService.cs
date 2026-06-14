namespace teams_phonemanager.Services.Interfaces;

/// <summary>
/// Service for sanitizing user inputs before they are used in PowerShell commands.
/// Prevents command injection attacks by escaping dangerous characters.
/// </summary>
public interface IPowerShellSanitizationService
{
    /// <summary>
    /// Sanitizes a string value for safe use in PowerShell commands.
    /// Escapes single quotes and removes dangerous characters.
    /// </summary>
    string SanitizeString(string input);

    /// <summary>
    /// Validates and sanitizes an identifier (e.g., UPN, display name).
    /// Only allows alphanumeric characters, hyphens, underscores, @ and dots.
    /// </summary>
    string SanitizeIdentifier(string input);
}
