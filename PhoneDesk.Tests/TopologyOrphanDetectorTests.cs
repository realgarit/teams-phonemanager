using System;
using System.Collections.Generic;
using System.Linq;
using PhoneDesk.Topology;

namespace PhoneDesk.Tests
{
    public class TopologyOrphanDetectorTests
    {
        private static TopologyResourceAccount Ra(
            string objectId, string? phone = null, bool enabled = true,
            ResourceAccountKind kind = ResourceAccountKind.CallQueue)
            => new($"RA {objectId}", $"{objectId}@contoso.com", objectId, phone, kind, enabled);

        private static TopologyAutoAttendant Aa(string id, params string[] raIds)
            => new($"AA {id}", id, "en-US", "UTC", raIds, Array.Empty<string>(), Array.Empty<string>());

        private static TopologyCallQueue Cq(string id, string[]? agents = null, string[]? dls = null, string[]? ras = null)
            => new($"CQ {id}", id, "Attendant", 30,
                agents ?? Array.Empty<string>(), dls ?? Array.Empty<string>(), ras ?? Array.Empty<string>());

        [Fact]
        public void UnassociatedResourceAccount_IsFlagged()
        {
            var findings = TopologyOrphanDetector.Detect(
                new[] { Aa("aa1", "ra-used") },
                Array.Empty<TopologyCallQueue>(),
                new[] { Ra("ra-used"), Ra("ra-orphan") });

            var orphans = findings.Where(f => f.Kind == OrphanKind.ResourceAccountUnassociated).ToList();
            Assert.Single(orphans);
            Assert.Equal("ra-orphan", orphans[0].EntityId);
        }

        [Fact]
        public void ResourceAccount_ReferencedByCallQueue_IsNotFlagged()
        {
            var findings = TopologyOrphanDetector.Detect(
                Array.Empty<TopologyAutoAttendant>(),
                new[] { Cq("cq1", agents: new[] { "a" }, ras: new[] { "ra-used" }) },
                new[] { Ra("ra-used") });

            Assert.DoesNotContain(findings, f => f.Kind == OrphanKind.ResourceAccountUnassociated);
        }

        [Fact]
        public void CallQueueWithoutAgentsOrGroups_IsFlagged()
        {
            var findings = TopologyOrphanDetector.Detect(
                Array.Empty<TopologyAutoAttendant>(),
                new[] { Cq("cq-empty"), Cq("cq-ok", agents: new[] { "agent1" }) },
                Array.Empty<TopologyResourceAccount>());

            var orphans = findings.Where(f => f.Kind == OrphanKind.CallQueueWithoutAgents).ToList();
            Assert.Single(orphans);
            Assert.Equal("cq-empty", orphans[0].EntityId);
        }

        [Fact]
        public void CallQueueWithOnlyDistributionList_IsNotFlaggedAsNoAgents()
        {
            var findings = TopologyOrphanDetector.Detect(
                Array.Empty<TopologyAutoAttendant>(),
                new[] { Cq("cq1", dls: new[] { "group-1" }) },
                Array.Empty<TopologyResourceAccount>());

            Assert.DoesNotContain(findings, f => f.Kind == OrphanKind.CallQueueWithoutAgents);
        }

        [Fact]
        public void PhoneNumberOnDisabledAccount_IsFlagged()
        {
            var findings = TopologyOrphanDetector.Detect(
                Array.Empty<TopologyAutoAttendant>(),
                new[] { Cq("cq1", ras: new[] { "ra-dis", "ra-ok" }) },
                new[]
                {
                    Ra("ra-dis", phone: "+41000", enabled: false),
                    Ra("ra-ok", phone: "+41111", enabled: true),
                });

            var orphans = findings.Where(f => f.Kind == OrphanKind.PhoneNumberOnDisabledAccount).ToList();
            Assert.Single(orphans);
            Assert.Equal("ra-dis", orphans[0].EntityId);
            Assert.Contains("+41000", orphans[0].Detail);
        }

        [Fact]
        public void DisabledAccountWithoutPhoneNumber_IsNotFlaggedForDisabledPhone()
        {
            var findings = TopologyOrphanDetector.Detect(
                Array.Empty<TopologyAutoAttendant>(),
                new[] { Cq("cq1", ras: new[] { "ra-dis" }) },
                new[] { Ra("ra-dis", phone: null, enabled: false) });

            Assert.DoesNotContain(findings, f => f.Kind == OrphanKind.PhoneNumberOnDisabledAccount);
        }

        [Fact]
        public void Detect_ToleratesNullCollections()
        {
            var findings = TopologyOrphanDetector.Detect(null!, null!, null!);
            Assert.Empty(findings);
        }
    }
}
