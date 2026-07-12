namespace teams_phonemanager.Services
{
    /// <summary>
    /// Classifies why a PowerShell-backed operation failed. Lives in the Application layer so
    /// Presentation can branch on typed categories instead of sniffing raw output strings.
    /// </summary>
    public enum OperationErrorCategory
    {
        /// <summary>No error — the operation succeeded.</summary>
        None = 0,

        /// <summary>Authentication or session problem (expired session, unauthorized, token/AADSTS failures).</summary>
        AuthSession,

        /// <summary>The service throttled the request (HTTP 429 / TooManyRequests / rate limiting).</summary>
        Throttling,

        /// <summary>A referenced object could not be found (404 / not found / does not exist).</summary>
        NotFound,

        /// <summary>Input or parameter validation failure (invalid argument, parameter binding).</summary>
        Validation,

        /// <summary>An error occurred but it does not match a more specific category.</summary>
        Unknown,

        /// <summary>The operation was cancelled cooperatively by the user before it completed.</summary>
        Cancelled
    }
}
