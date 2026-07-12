using System;
using System.Threading;
using System.Threading.Tasks;

namespace teams_phonemanager.Services.Interfaces
{
    /// <summary>
    /// Executes a PowerShell-backed operation with automatic retry for throttle-classified failures
    /// (HTTP 429 / <see cref="OperationErrorCategory.Throttling"/>). Retries use exponential backoff with
    /// jitter, honor a server-provided <c>Retry-After</c> when present, and stop after a configurable
    /// maximum number of attempts. Non-throttling failures are never retried, and non-idempotent
    /// operations (per <see cref="ThrottleRetryContext.IsIdempotent"/>) are not auto-retried at all.
    /// </summary>
    public interface IThrottleRetryPolicy
    {
        /// <summary>
        /// Runs <paramref name="operation"/>, retrying on throttling per the policy. The delegate receives
        /// the (possibly cancelled) token and returns the typed result the policy inspects. A cancellation
        /// during a backoff wait aborts promptly with <see cref="OperationCanceledException"/>.
        /// </summary>
        Task<OperationResult<string>> ExecuteAsync(
            Func<CancellationToken, Task<OperationResult<string>>> operation,
            ThrottleRetryContext context,
            CancellationToken cancellationToken = default);
    }
}
