using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PhoneDesk.Services
{
    /// <summary>
    /// Parses a server-provided <c>Retry-After</c> hint out of PowerShell / Graph / Teams error text.
    /// Microsoft Graph and Teams surface throttling as an HTTP 429 whose <c>Retry-After</c> header (seconds)
    /// is echoed into the flattened error string; honoring it is more accurate than blind backoff.
    ///
    /// Pure and side-effect-free so it is trivially unit-testable. Only the numeric-seconds form is parsed;
    /// an HTTP-date form (rare for Graph throttling) is ignored and the caller falls back to backoff.
    /// </summary>
    public static class ThrottleInfo
    {
        // Matches "Retry-After: 30", "Retry-After 30", "retry after 8s", "retry after 30 seconds", etc.
        // The separator class only allows quotes/colon/equals/whitespace, so a digit must be the first
        // meaningful token after the phrase — this avoids false positives like "retry after failure 30".
        private static readonly Regex RetryAfterRegex = new(
            "retry[-\\s]?after[\"'\\s:=]*?(\\d+(?:\\.\\d+)?)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <summary>
        /// Attempts to extract a non-negative <c>Retry-After</c> duration from <paramref name="text"/>.
        /// Returns false (and <see cref="TimeSpan.Zero"/>) when no numeric hint is present.
        /// </summary>
        public static bool TryGetRetryAfter(string? text, out TimeSpan retryAfter)
        {
            retryAfter = TimeSpan.Zero;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            var match = RetryAfterRegex.Match(text);
            if (!match.Success)
            {
                return false;
            }

            if (!double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds)
                || seconds < 0)
            {
                return false;
            }

            retryAfter = TimeSpan.FromSeconds(seconds);
            return true;
        }
    }
}
