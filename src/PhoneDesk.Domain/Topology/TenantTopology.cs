using System;
using System.Collections.Generic;

namespace PhoneDesk.Topology
{
    /// <summary>
    /// An immutable, read-only snapshot of the tenant's telephony topology plus the orphans detected
    /// within it. Assembled by the Application layer from raw PowerShell output; consumed by the
    /// Dashboard. Framework-free by design (Clean Architecture Dependency Rule).
    /// </summary>
    public sealed class TenantTopology
    {
        public TenantTopology(
            IReadOnlyList<TopologyAutoAttendant> autoAttendants,
            IReadOnlyList<TopologyCallQueue> callQueues,
            IReadOnlyList<TopologyResourceAccount> resourceAccounts,
            IReadOnlyList<TopologyGroup> groups,
            IReadOnlyList<OrphanFinding> orphans,
            DateTimeOffset retrievedAtUtc)
        {
            AutoAttendants = autoAttendants;
            CallQueues = callQueues;
            ResourceAccounts = resourceAccounts;
            Groups = groups;
            Orphans = orphans;
            RetrievedAtUtc = retrievedAtUtc;
        }

        public IReadOnlyList<TopologyAutoAttendant> AutoAttendants { get; }
        public IReadOnlyList<TopologyCallQueue> CallQueues { get; }
        public IReadOnlyList<TopologyResourceAccount> ResourceAccounts { get; }
        public IReadOnlyList<TopologyGroup> Groups { get; }
        public IReadOnlyList<OrphanFinding> Orphans { get; }
        public DateTimeOffset RetrievedAtUtc { get; }

        public static TenantTopology Empty { get; } = new(
            Array.Empty<TopologyAutoAttendant>(),
            Array.Empty<TopologyCallQueue>(),
            Array.Empty<TopologyResourceAccount>(),
            Array.Empty<TopologyGroup>(),
            Array.Empty<OrphanFinding>(),
            DateTimeOffset.MinValue);
    }
}
