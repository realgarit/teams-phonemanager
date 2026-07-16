namespace teams_phonemanager.Audit
{
    /// <summary>How an audited operation ended. Pure enum (Domain), serialized by name in the log.</summary>
    public enum AuditOutcome
    {
        /// <summary>The operation completed without an error signal.</summary>
        Success,

        /// <summary>The operation reported an error.</summary>
        Failure,

        /// <summary>The operation was cancelled cooperatively before completing.</summary>
        Cancelled
    }
}
