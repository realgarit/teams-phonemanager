using System;
using System.Collections.Generic;
using System.Linq;

namespace PhoneDesk.Services
{
    /// <summary>
    /// Maps raw PowerShell execution output into a typed <see cref="OperationResult{T}"/>.
    ///
    /// This is the single place that understands the output markers (<c>SUCCESS</c> / <c>ERROR:</c>) and
    /// the wording of PowerShell / Graph / Teams error messages. Keeping the marker + category parsing here
    /// (rather than scattered across ViewModels) is the whole point of issue #60: Presentation consumes the
    /// typed result, and the markers themselves are never changed.
    ///
    /// Pure and side-effect-free, so it is trivially unit-testable.
    /// </summary>
    public static class PowerShellOperationResultMapper
    {
        private const string SuccessMarker = "SUCCESS";
        private const string ErrorMarker = "ERROR:";

        // Category keyword tables. Checked in priority order (throttling → auth → not-found → validation).
        private static readonly string[] ThrottlingKeywords =
        {
            "429", "throttl", "toomanyrequests", "too many requests", "rate limit", "retry after", "retry-after"
        };

        private static readonly string[] AuthSessionKeywords =
        {
            "session expired", "session has expired", "please reconnect", "unauthorized", "401",
            "authentication", "aadsts", "invalidauthenticationtoken", "access token", "token has expired",
            "token is expired", "not connected", "interactiveauthentication", "forbidden", "403"
        };

        private static readonly string[] NotFoundKeywords =
        {
            "not found", "notfound", "404", "does not exist", "could not be found", "cannot find", "no such"
        };

        private static readonly string[] ValidationKeywords =
        {
            "cannot validate argument", "parameterbindingexception", "parameter binding", "is not valid",
            "not a valid", "invalid", "validation", "must be", "is required", "missing required"
        };

        /// <summary>
        /// Maps a raw execution result into a typed <see cref="OperationResult{T}"/> of the output string.
        /// </summary>
        public static OperationResult<string> Map(PowerShellExecutionResult execution, string? correlationId = null)
        {
            ArgumentNullException.ThrowIfNull(execution);

            var output = execution.Output ?? string.Empty;
            var errors = execution.Errors ?? Array.Empty<PowerShellErrorInfo>();

            var hasSuccessMarker = output.Contains(SuccessMarker, StringComparison.Ordinal);
            var hasErrorMarker = output.Contains(ErrorMarker, StringComparison.Ordinal);
            var anyError = hasErrorMarker || execution.HadErrors || errors.Count > 0;

            if (!anyError)
            {
                return OperationResult<string>.Create(
                    isSuccess: true,
                    value: output,
                    category: OperationErrorCategory.None,
                    errorMessage: null,
                    errors: errors,
                    correlationId: correlationId ?? NewCorrelationId(),
                    rawOutput: output,
                    hasSuccessMarker: hasSuccessMarker,
                    hasErrorMarker: hasErrorMarker);
            }

            var category = Categorize(output, errors);
            var errorMessage = BuildErrorMessage(output, errors);

            return OperationResult<string>.Create(
                isSuccess: false,
                value: output,
                category: category,
                errorMessage: errorMessage,
                errors: errors,
                correlationId: correlationId ?? NewCorrelationId(),
                rawOutput: output,
                hasSuccessMarker: hasSuccessMarker,
                hasErrorMarker: hasErrorMarker);
        }

        /// <summary>
        /// Builds a typed failure directly, without a PowerShell round-trip (e.g. a pre-flight session-expiry
        /// check). <paramref name="output"/> is the text the UI historically displayed for this failure.
        /// </summary>
        public static OperationResult<string> Failure(
            OperationErrorCategory category,
            string errorMessage,
            string output,
            string? correlationId = null)
        {
            output ??= string.Empty;
            return OperationResult<string>.Create(
                isSuccess: false,
                value: output,
                category: category,
                errorMessage: errorMessage,
                errors: Array.Empty<PowerShellErrorInfo>(),
                correlationId: correlationId ?? NewCorrelationId(),
                rawOutput: output,
                hasSuccessMarker: output.Contains(SuccessMarker, StringComparison.Ordinal),
                hasErrorMarker: output.Contains(ErrorMarker, StringComparison.Ordinal));
        }

        /// <summary>
        /// Classifies error text into a typed category. Considers the combined output plus every structured
        /// error record (message, category info, raw text). Priority: throttling, auth/session, not-found,
        /// validation, then unknown.
        /// </summary>
        public static OperationErrorCategory Categorize(string output, IReadOnlyList<PowerShellErrorInfo> errors)
        {
            var haystack = BuildHaystack(output, errors);
            if (haystack.Length == 0)
            {
                return OperationErrorCategory.Unknown;
            }

            if (ContainsAny(haystack, ThrottlingKeywords)) return OperationErrorCategory.Throttling;
            if (ContainsAny(haystack, AuthSessionKeywords)) return OperationErrorCategory.AuthSession;
            if (ContainsAny(haystack, NotFoundKeywords)) return OperationErrorCategory.NotFound;
            if (ContainsAny(haystack, ValidationKeywords)) return OperationErrorCategory.Validation;

            return OperationErrorCategory.Unknown;
        }

        private static string BuildHaystack(string output, IReadOnlyList<PowerShellErrorInfo> errors)
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(output)) parts.Add(output);
            if (errors != null)
            {
                foreach (var e in errors)
                {
                    if (!string.IsNullOrEmpty(e.Message)) parts.Add(e.Message);
                    if (!string.IsNullOrEmpty(e.CategoryInfo)) parts.Add(e.CategoryInfo);
                    if (!string.IsNullOrEmpty(e.RawText)) parts.Add(e.RawText);
                    if (!string.IsNullOrEmpty(e.ExceptionType)) parts.Add(e.ExceptionType);
                }
            }
            return string.Join("\n", parts).ToLowerInvariant();
        }

        private static bool ContainsAny(string haystack, string[] keywords)
            => keywords.Any(k => haystack.Contains(k, StringComparison.Ordinal));

        private static string BuildErrorMessage(string output, IReadOnlyList<PowerShellErrorInfo> errors)
        {
            var firstStructured = errors?.FirstOrDefault(e => !string.IsNullOrWhiteSpace(e.Message));
            if (firstStructured != null)
            {
                return firstStructured.Message;
            }

            // Fall back to the first ERROR: line in the output.
            if (!string.IsNullOrEmpty(output))
            {
                foreach (var line in output.Split('\n'))
                {
                    var idx = line.IndexOf(ErrorMarker, StringComparison.Ordinal);
                    if (idx >= 0)
                    {
                        return line[(idx + ErrorMarker.Length)..].Trim();
                    }
                }
            }

            return "An unspecified PowerShell error occurred.";
        }

        private static string NewCorrelationId() => Guid.NewGuid().ToString("N");
    }
}
