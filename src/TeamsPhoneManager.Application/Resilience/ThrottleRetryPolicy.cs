using System;
using System.Threading;
using System.Threading.Tasks;
using teams_phonemanager.Services.Interfaces;

namespace teams_phonemanager.Services
{
    /// <summary>
    /// Throttle-aware retry policy (see <see cref="IThrottleRetryPolicy"/>).
    ///
    /// Retry safety: only <see cref="OperationErrorCategory.Throttling"/> failures are retryable, and only
    /// when the operation is declared idempotent. A non-idempotent operation that is throttled is returned
    /// as-is (with a warning) rather than silently re-run, because a mutating batch may have partially
    /// applied before the 429. Every other outcome — success, validation, not-found, auth — is returned on
    /// the first attempt.
    ///
    /// The delay and jitter sources are injectable so the backoff schedule is deterministic and instant
    /// under unit test; in production they default to <see cref="Task.Delay(TimeSpan, CancellationToken)"/>
    /// and <see cref="Random.Shared"/>.
    /// </summary>
    public sealed class ThrottleRetryPolicy : IThrottleRetryPolicy
    {
        private readonly ILoggingService _loggingService;
        private readonly ThrottleRetryOptions _options;
        private readonly Func<TimeSpan, CancellationToken, Task> _delay;
        private readonly Func<double> _jitterSource;

        public ThrottleRetryPolicy(
            ILoggingService loggingService,
            ThrottleRetryOptions? options = null,
            Func<TimeSpan, CancellationToken, Task>? delay = null,
            Func<double>? jitterSource = null)
        {
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _options = options ?? ThrottleRetryOptions.Default;
            _delay = delay ?? Task.Delay;
            _jitterSource = jitterSource ?? (() => Random.Shared.NextDouble());
        }

        public async Task<OperationResult<string>> ExecuteAsync(
            Func<CancellationToken, Task<OperationResult<string>>> operation,
            ThrottleRetryContext context,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);
            ArgumentNullException.ThrowIfNull(context);

            var maxAttempts = context.IsIdempotent ? Math.Max(1, _options.MaxAttempts) : 1;
            var attempt = 0;

            while (true)
            {
                attempt++;
                cancellationToken.ThrowIfCancellationRequested();

                var result = await operation(cancellationToken);

                if (result.Category != OperationErrorCategory.Throttling)
                {
                    return result;
                }

                // Throttled. Decide whether another attempt is warranted.
                if (!context.IsIdempotent)
                {
                    _loggingService.Log(
                        $"Throttled by Microsoft Graph/Teams while running {context.OperationName}; not auto-retrying a non-idempotent operation. Re-run it once throttling subsides.",
                        LogLevel.Warning);
                    return result;
                }

                if (attempt >= maxAttempts)
                {
                    _loggingService.Log(
                        $"Throttled by Microsoft Graph/Teams; giving up on {context.OperationName} after {attempt}/{maxAttempts} attempts.",
                        LogLevel.Warning);
                    return result;
                }

                var wait = ComputeDelay(attempt, result);
                _loggingService.Log(
                    $"Throttled by Microsoft Graph/Teams, retrying in {wait.TotalSeconds:0.#}s (attempt {attempt + 1}/{maxAttempts}) for {context.OperationName}.",
                    LogLevel.Warning);

                // A cancellation during the backoff wait aborts promptly (Task.Delay throws).
                await _delay(wait, cancellationToken);
            }
        }

        /// <summary>
        /// Computes the wait before the next attempt: a server-provided <c>Retry-After</c> is authoritative
        /// (clamped to <see cref="ThrottleRetryOptions.MaxRetryAfterSeconds"/>); otherwise exponential
        /// backoff (capped at <see cref="ThrottleRetryOptions.MaxDelaySeconds"/>) plus additive jitter.
        /// </summary>
        private TimeSpan ComputeDelay(int attempt, OperationResult<string> result)
        {
            if (ThrottleInfo.TryGetRetryAfter(result.RawOutput, out var retryAfter)
                || ThrottleInfo.TryGetRetryAfter(result.ErrorMessage, out retryAfter))
            {
                var seconds = Math.Min(retryAfter.TotalSeconds, _options.MaxRetryAfterSeconds);
                return TimeSpan.FromSeconds(Math.Max(0, seconds));
            }

            var backoff = _options.BaseDelaySeconds * Math.Pow(2, attempt - 1);
            backoff = Math.Min(backoff, _options.MaxDelaySeconds);

            var jitter = _jitterSource() * _options.JitterFactor * _options.BaseDelaySeconds;
            return TimeSpan.FromSeconds(backoff + jitter);
        }
    }
}
