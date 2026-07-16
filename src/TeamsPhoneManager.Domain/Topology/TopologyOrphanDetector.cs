using System;
using System.Collections.Generic;
using System.Linq;

namespace teams_phonemanager.Topology
{
    /// <summary>
    /// Pure orphan-detection rules for the tenant dashboard (issue #64). No IO, no framework — fully
    /// unit-testable. The three rules mirror the acceptance criteria:
    /// <list type="bullet">
    /// <item>resource accounts with no AA/CQ association,</item>
    /// <item>call queues with no agents,</item>
    /// <item>phone numbers assigned to disabled accounts.</item>
    /// </list>
    /// </summary>
    public static class TopologyOrphanDetector
    {
        public static IReadOnlyList<OrphanFinding> Detect(
            IReadOnlyList<TopologyAutoAttendant> autoAttendants,
            IReadOnlyList<TopologyCallQueue> callQueues,
            IReadOnlyList<TopologyResourceAccount> resourceAccounts)
        {
            autoAttendants ??= Array.Empty<TopologyAutoAttendant>();
            callQueues ??= Array.Empty<TopologyCallQueue>();
            resourceAccounts ??= Array.Empty<TopologyResourceAccount>();

            var findings = new List<OrphanFinding>();

            // Every resource-account object id referenced by any AA or CQ.
            var referenced = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var aa in autoAttendants)
            {
                AddAll(referenced, aa.ResourceAccountObjectIds);
            }
            foreach (var cq in callQueues)
            {
                AddAll(referenced, cq.ResourceAccountObjectIds);
            }

            // Rule 1: resource account not referenced by any AA or CQ.
            foreach (var ra in resourceAccounts)
            {
                if (string.IsNullOrWhiteSpace(ra.ObjectId) || !referenced.Contains(ra.ObjectId))
                {
                    findings.Add(new OrphanFinding(
                        OrphanKind.ResourceAccountUnassociated,
                        ra.ObjectId,
                        DisplayLabel(ra.DisplayName, ra.UserPrincipalName),
                        "Resource account is not associated with any auto attendant or call queue."));
                }
            }

            // Rule 2: call queue with no agents and no groups.
            foreach (var cq in callQueues)
            {
                if (cq.HasNoAgents)
                {
                    findings.Add(new OrphanFinding(
                        OrphanKind.CallQueueWithoutAgents,
                        cq.Identity,
                        string.IsNullOrWhiteSpace(cq.Name) ? cq.Identity : cq.Name,
                        "Call queue has no agents and no distribution lists."));
                }
            }

            // Rule 3: phone number assigned to a disabled resource account.
            foreach (var ra in resourceAccounts)
            {
                if (ra.HasPhoneNumber && !ra.AccountEnabled)
                {
                    findings.Add(new OrphanFinding(
                        OrphanKind.PhoneNumberOnDisabledAccount,
                        ra.ObjectId,
                        DisplayLabel(ra.DisplayName, ra.UserPrincipalName),
                        $"Phone number {ra.PhoneNumber} is assigned to a disabled account."));
                }
            }

            return findings;
        }

        private static void AddAll(HashSet<string> set, IReadOnlyList<string>? ids)
        {
            if (ids is null)
            {
                return;
            }

            foreach (var id in ids)
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    set.Add(id);
                }
            }
        }

        private static string DisplayLabel(string? displayName, string? upn)
            => !string.IsNullOrWhiteSpace(displayName) ? displayName!
                : (!string.IsNullOrWhiteSpace(upn) ? upn! : "(unknown)");
    }
}
