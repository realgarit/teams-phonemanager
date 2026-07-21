using System.Collections.Generic;

namespace PhoneDesk.Topology
{
    /// <summary>
    /// Distinguishes what a Teams resource account (application instance) is wired to, derived from
    /// its Azure AD application id. Pure Domain value — no framework dependency.
    /// </summary>
    public enum ResourceAccountKind
    {
        Unknown = 0,
        AutoAttendant = 1,
        CallQueue = 2,
    }

    /// <summary>
    /// A read-only snapshot of a resource account in the tenant telephony topology.
    /// <paramref name="ObjectId"/> is the Azure AD object id used to correlate this account with the
    /// auto attendants / call queues that reference it.
    /// </summary>
    public sealed record TopologyResourceAccount(
        string DisplayName,
        string UserPrincipalName,
        string ObjectId,
        string? PhoneNumber,
        ResourceAccountKind Kind,
        bool AccountEnabled)
    {
        public bool HasPhoneNumber => !string.IsNullOrWhiteSpace(PhoneNumber);
    }

    /// <summary>Read-only snapshot of an auto attendant and the ids it relates to.</summary>
    public sealed record TopologyAutoAttendant(
        string Name,
        string Identity,
        string LanguageId,
        string TimeZoneId,
        IReadOnlyList<string> ResourceAccountObjectIds,
        IReadOnlyList<string> HolidayScheduleIds,
        IReadOnlyList<string> CallTargetObjectIds);

    /// <summary>Read-only snapshot of a call queue and the ids it relates to.</summary>
    public sealed record TopologyCallQueue(
        string Name,
        string Identity,
        string RoutingMethod,
        int AgentAlertTime,
        IReadOnlyList<string> AgentObjectIds,
        IReadOnlyList<string> DistributionListIds,
        IReadOnlyList<string> ResourceAccountObjectIds)
    {
        /// <summary>A queue has no way to reach a human when it has neither direct agents nor any group.</summary>
        public bool HasNoAgents => AgentObjectIds.Count == 0 && DistributionListIds.Count == 0;
    }

    /// <summary>Read-only snapshot of an M365 group referenced by the topology.</summary>
    public sealed record TopologyGroup(
        string DisplayName,
        string Id,
        string MailNickname,
        string Description);
}
