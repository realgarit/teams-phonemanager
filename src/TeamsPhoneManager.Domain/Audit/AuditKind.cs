namespace teams_phonemanager.Audit
{
    /// <summary>
    /// Distinguishes mutating operations from lower-level reads. Read-only entries are logged at a
    /// lower level and can be excluded from the history view. Pure enum (Domain).
    /// </summary>
    public enum AuditKind
    {
        /// <summary>A mutating operation (create / update / remove / connect).</summary>
        Operation,

        /// <summary>A read / query / export — logged at a lower level.</summary>
        Read
    }
}
