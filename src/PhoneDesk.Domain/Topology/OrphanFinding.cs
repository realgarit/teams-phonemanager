namespace PhoneDesk.Topology
{
    /// <summary>
    /// The categories of dangling / misconfigured objects the dashboard surfaces. Each maps to one
    /// acceptance-criteria rule for issue #64.
    /// </summary>
    public enum OrphanKind
    {
        /// <summary>A resource account that no auto attendant or call queue references.</summary>
        ResourceAccountUnassociated = 0,

        /// <summary>A call queue with neither direct agents nor any distribution list (no one to ring).</summary>
        CallQueueWithoutAgents = 1,

        /// <summary>A phone number assigned to a resource account whose Azure AD account is disabled.</summary>
        PhoneNumberOnDisabledAccount = 2,
    }

    /// <summary>
    /// One detected orphan. Pure Domain value; the presentation layer decides how to render it.
    /// <paramref name="EntityId"/> is a stable identifier (object id / identity) and
    /// <paramref name="EntityName"/> is the human-readable label.
    /// </summary>
    public sealed record OrphanFinding(
        OrphanKind Kind,
        string EntityId,
        string EntityName,
        string Detail);
}
