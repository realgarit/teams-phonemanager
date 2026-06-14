namespace teams_phonemanager.Services.Interfaces
{
    /// <summary>
    /// Service for authenticating to Microsoft Graph using MSAL.
    /// This bypasses PowerShell's WAM requirement by obtaining tokens natively in C#.
    /// </summary>
    public interface IMsalGraphAuthenticationService
    {
        /// <summary>
        /// Authenticates to Microsoft Graph interactively using a browser popup.
        /// Returns a tuple of (success, accessToken, errorMessage).
        /// </summary>
        /// <param name="parentWindowHandle">Optional parent window handle for the browser popup (Windows only)</param>
        Task<(bool Success, string? AccessToken, string? Account, string? ErrorMessage)> AuthenticateAsync(IntPtr? parentWindowHandle = null);
        
        /// <summary>
        /// Signs out from Microsoft Graph, clearing cached tokens.
        /// </summary>
        Task SignOutAsync();
        
        /// <summary>
        /// Checks if there's a cached account that can be used for silent authentication.
        /// </summary>
        Task<bool> HasCachedAccountAsync();
    }
}
