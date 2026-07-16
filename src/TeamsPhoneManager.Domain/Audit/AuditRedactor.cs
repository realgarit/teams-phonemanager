using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace teams_phonemanager.Audit
{
    /// <summary>
    /// Scrubs secrets from audit content. Pure, deterministic, framework-free (Domain) so both the
    /// Infrastructure writer and the unit tests share one source of truth for redaction.
    ///
    /// Two complementary strategies:
    ///  * key-based — any parameter whose <b>name</b> looks sensitive (secret / token / password / …)
    ///    has its value replaced wholesale, regardless of the value's shape;
    ///  * value-based — any string is scanned for token-shaped substrings (JWTs, "Bearer …" headers,
    ///    long secret blobs) which are replaced in place. This catches secrets that leak into error
    ///    text or an unexpected parameter.
    ///
    /// The guarantee this backs: no access token or client secret ever appears in a persisted audit
    /// record. Redaction is applied by the writer immediately before serialization.
    /// </summary>
    public static class AuditRedactor
    {
        /// <summary>The literal written in place of redacted content.</summary>
        public const string Placeholder = "***REDACTED***";

        // Parameter names that must never have their value persisted.
        private static readonly string[] SensitiveKeyFragments =
        {
            "secret", "password", "pwd", "token", "credential", "apikey", "api_key",
            "authorization", "bearer", "certificate", "privatekey", "private_key",
            "connectionstring", "connection_string", "clientsecret", "accesskey", "sharedkey"
        };

        // JWT (two or three dot-separated base64url segments beginning with the "eyJ" header).
        private static readonly Regex JwtPattern = new(
            @"eyJ[A-Za-z0-9_-]{5,}\.[A-Za-z0-9_-]{5,}(?:\.[A-Za-z0-9_-]+)?",
            RegexOptions.Compiled);

        // "Bearer <token>" / "Authorization: Bearer <token>".
        private static readonly Regex BearerPattern = new(
            @"(?i)bearer\s+[A-Za-z0-9_\-\.=]+",
            RegexOptions.Compiled);

        /// <summary>Returns a copy of the record with every free-text and parameter field scrubbed.</summary>
        public static AuditRecord Redact(AuditRecord record)
        {
            ArgumentNullException.ThrowIfNull(record);

            return record with
            {
                Operation = RedactText(record.Operation) ?? string.Empty,
                Target = RedactText(record.Target),
                ErrorDetail = RedactText(record.ErrorDetail),
                Parameters = RedactParameters(record.Parameters)
            };
        }

        /// <summary>Scrubs token-shaped substrings from a free-text value. Null in, null out.</summary>
        public static string? RedactText(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            var scrubbed = JwtPattern.Replace(value, Placeholder);
            scrubbed = BearerPattern.Replace(scrubbed, Placeholder);
            return scrubbed;
        }

        /// <summary>
        /// Scrubs a parameter map: sensitive keys have their value fully replaced; all other values are
        /// still scanned for token-shaped substrings.
        /// </summary>
        public static IReadOnlyDictionary<string, string>? RedactParameters(IReadOnlyDictionary<string, string>? parameters)
        {
            if (parameters is null || parameters.Count == 0)
            {
                return parameters;
            }

            var result = new Dictionary<string, string>(parameters.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in parameters)
            {
                result[kvp.Key] = IsSensitiveKey(kvp.Key)
                    ? Placeholder
                    : RedactText(kvp.Value) ?? string.Empty;
            }
            return result;
        }

        private static bool IsSensitiveKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            foreach (var fragment in SensitiveKeyFragments)
            {
                if (key.Contains(fragment, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
