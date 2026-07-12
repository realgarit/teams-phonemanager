namespace teams_phonemanager.Services
{
    /// <summary>
    /// Tunable knobs for throttle-aware retry and bulk pacing. Defaults come from
    /// <see cref="ConstantsService.Throttling"/> (Domain) so there is a single source of truth; callers
    /// can supply an alternative instance (e.g. a settings-backed one) without changing the policy code.
    /// Immutable so it can be shared as a singleton.
    /// </summary>
    public sealed class ThrottleRetryOptions
    {
        /// <summary>Maximum total attempts for an idempotent operation (initial call included). Values &lt; 1 are treated as 1.</summary>
        public int MaxAttempts { get; init; } = ConstantsService.Throttling.MaxRetryAttempts;

        /// <summary>Base delay for exponential backoff: delay = BaseDelaySeconds * 2^(attempt-1).</summary>
        public double BaseDelaySeconds { get; init; } = ConstantsService.Throttling.BaseDelaySeconds;

        /// <summary>Upper bound for a single computed backoff wait (before jitter).</summary>
        public double MaxDelaySeconds { get; init; } = ConstantsService.Throttling.MaxDelaySeconds;

        /// <summary>Fraction of the base delay used as additive jitter (0..1).</summary>
        public double JitterFactor { get; init; } = ConstantsService.Throttling.JitterFactor;

        /// <summary>Ceiling applied to a server-provided <c>Retry-After</c> value.</summary>
        public double MaxRetryAfterSeconds { get; init; } = ConstantsService.Throttling.MaxRetryAfterSeconds;

        /// <summary>Base delay inserted between items of a bulk loop.</summary>
        public double BulkInterItemDelaySeconds { get; init; } = ConstantsService.Throttling.BulkInterItemDelaySeconds;

        /// <summary>Upper bound for the adaptive inter-item delay after repeated throttling.</summary>
        public double BulkMaxInterItemDelaySeconds { get; init; } = ConstantsService.Throttling.BulkMaxInterItemDelaySeconds;

        /// <summary>Factor the inter-item delay grows by after a throttle event; its reciprocal is used to recover on success.</summary>
        public double BulkPaceMultiplierOnThrottle { get; init; } = ConstantsService.Throttling.BulkPaceMultiplierOnThrottle;

        /// <summary>Shared immutable instance built from the <see cref="ConstantsService.Throttling"/> defaults.</summary>
        public static ThrottleRetryOptions Default { get; } = new();
    }
}
