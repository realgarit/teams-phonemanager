namespace PhoneDesk.Services
{
    /// <summary>
    /// Per-call context for <see cref="Interfaces.IThrottleRetryPolicy"/>.
    ///
    /// <see cref="IsIdempotent"/> is the safety gate: only operations that can be re-executed without
    /// changing the outcome (reads, connection checks) are auto-retried. A mutating batch (create M365
    /// group / resource account / call queue / auto attendant) may have partially applied before the 429,
    /// so it is declared non-idempotent and the policy surfaces the throttling result instead of silently
    /// re-running it.
    /// </summary>
    public sealed class ThrottleRetryContext
    {
        public ThrottleRetryContext(string operationName, bool isIdempotent)
        {
            OperationName = string.IsNullOrWhiteSpace(operationName) ? "operation" : operationName;
            IsIdempotent = isIdempotent;
        }

        /// <summary>Human-readable label used in retry log lines.</summary>
        public string OperationName { get; }

        /// <summary>True when the operation is safe to auto-retry on a throttle response.</summary>
        public bool IsIdempotent { get; }
    }
}
