using Microsoft.Identity.Client;
using teams_phonemanager.Services.Interfaces;


namespace teams_phonemanager.Services
{
    /// <summary>
    /// Authenticates to Microsoft Graph using MSAL.NET with a browser popup.
    /// This bypasses the WAM (Web Account Manager) issues on Windows by using
    /// interactive browser authentication with system browser.
    /// </summary>
    public class MsalGraphAuthenticationService : IMsalGraphAuthenticationService
    {
        private readonly ILoggingService _loggingService;
        private readonly IPublicClientApplication _msalApp;
        
        // Microsoft Graph PowerShell SDK's default client ID
        // This is the same client ID used by Connect-MgGraph
        private const string ClientId = "14d82eec-204b-4c2f-b7e8-296a70dab67e";
        
        // Required scopes for the app
        private static readonly string[] Scopes = new[]
        {
            "User.ReadWrite.All",
            "Organization.Read.All",
            "Group.ReadWrite.All",
            "Directory.ReadWrite.All"
        };
        
        public MsalGraphAuthenticationService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            
            // Configure MSAL to use system browser for authentication
            // This bypasses WAM entirely and works on all platforms
            _msalApp = PublicClientApplicationBuilder
                .Create(ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, "common")
                .WithRedirectUri("http://localhost")  // Use localhost redirect for system browser
                .Build();
            
            _loggingService.Log("MSAL Graph authentication service initialized", LogLevel.Info);
        }
        
        public async Task<(bool Success, string? AccessToken, string? Account, string? ErrorMessage)> AuthenticateAsync(IntPtr? parentWindowHandle = null)
        {
            try
            {
                _loggingService.Log("Starting MSAL interactive authentication...", LogLevel.Info);
                
                AuthenticationResult? result = null;
                
                // First, try silent authentication with cached accounts
                var accounts = await _msalApp.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();
                
                if (firstAccount != null)
                {
                    try
                    {
                        _loggingService.Log($"Attempting silent authentication for cached account: {firstAccount.Username}", LogLevel.Info);
                        result = await _msalApp.AcquireTokenSilent(Scopes, firstAccount).ExecuteAsync();
                    }
                    catch (MsalUiRequiredException)
                    {
                        _loggingService.Log("Silent authentication failed, falling back to interactive...", LogLevel.Info);
                        // Silent auth failed, will do interactive below
                    }
                }
                
                // If silent failed or no cached account, do interactive
                if (result == null)
                {
                    var interactiveBuilder = _msalApp.AcquireTokenInteractive(Scopes);
                    
                    // Use system browser for authentication
                    interactiveBuilder = interactiveBuilder.WithUseEmbeddedWebView(false);
                    
                    // Set parent window handle if provided (for Windows)
                    if (parentWindowHandle.HasValue && parentWindowHandle.Value != IntPtr.Zero)
                    {
                        interactiveBuilder = interactiveBuilder.WithParentActivityOrWindow(parentWindowHandle.Value);
                    }
                    
                    _loggingService.Log("Opening browser for authentication...", LogLevel.Info);
                    result = await interactiveBuilder.ExecuteAsync();
                }
                
                if (result != null && !string.IsNullOrEmpty(result.AccessToken))
                {
                    _loggingService.Log($"Authentication successful for account: {result.Account?.Username}", LogLevel.Success);
                    return (true, result.AccessToken, result.Account?.Username, null);
                }
                
                return (false, null, null, "Authentication completed but no token received");
            }
            catch (MsalClientException ex) when (ex.ErrorCode == "authentication_canceled")
            {
                _loggingService.Log("Authentication was canceled by user", LogLevel.Warning);
                return (false, null, null, "Authentication was canceled");
            }
            catch (MsalServiceException ex)
            {
                var errorMessage = $"MSAL service error: {ex.Message}";
                _loggingService.Log(errorMessage, LogLevel.Error);
                return (false, null, null, errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Authentication error: {ex.Message}";
                _loggingService.Log(errorMessage, LogLevel.Error);
                return (false, null, null, errorMessage);
            }
        }
        
        public async Task SignOutAsync()
        {
            try
            {
                var accounts = await _msalApp.GetAccountsAsync();
                foreach (var account in accounts)
                {
                    await _msalApp.RemoveAsync(account);
                    _loggingService.Log($"Removed cached account: {account.Username}", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                _loggingService.Log($"Error during sign out: {ex.Message}", LogLevel.Warning);
            }
        }
        
        public async Task<bool> HasCachedAccountAsync()
        {
            var accounts = await _msalApp.GetAccountsAsync();
            return accounts.Any();
        }
    }
}
