using System.Text.RegularExpressions;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services;

/// <summary>
/// Service for sanitizing user inputs before they are used in PowerShell commands.
/// Prevents command injection attacks by escaping dangerous characters.
/// </summary>
public partial class PowerShellSanitizationService : IPowerShellSanitizationService
{
    // Pattern for safe identifiers (UPNs, display names, etc.)
    [GeneratedRegex(@"^[a-zA-Z0-9\-_@.\s]+$", RegexOptions.Compiled)]
    private static partial Regex SafeIdentifierPattern();

    /// <summary>
    /// Sanitizes a string value for safe use in PowerShell commands.
    /// Escapes single quotes and removes dangerous characters.
    /// </summary>
    public string SanitizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // Escape single quotes for PowerShell (double them)
        var sanitized = input.Replace("'", "''");

        // Remove dangerous PowerShell characters
        sanitized = sanitized.Replace("`", "")   // Backtick (escape character)
                            .Replace("$", "")     // Variable expansion
                            .Replace(";", "")     // Command separator
                            .Replace("|", "")     // Pipeline
                            .Replace("&", "")     // Command separator
                            .Replace("<", "")     // Input redirection
                            .Replace(">", "");    // Output redirection

        return sanitized;
    }

    /// <summary>
    /// Validates and sanitizes an identifier (e.g., UPN, display name).
    /// Only allows alphanumeric characters, hyphens, underscores, @ and dots.
    /// </summary>
    public string SanitizeIdentifier(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Identifier cannot be null or whitespace.", nameof(input));

        if (!SafeIdentifierPattern().IsMatch(input))
            throw new ArgumentException($"Invalid identifier: {input}. Only alphanumeric characters, hyphens, underscores, @, dots, and spaces are allowed.", nameof(input));

        // Still escape single quotes for PowerShell
        return input.Replace("'", "''");
    }
}
