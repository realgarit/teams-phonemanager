using System;
using teams_phonemanager.Topology;

namespace teams_phonemanager.Services.Interfaces
{
    /// <summary>
    /// Assembles a <see cref="TenantTopology"/> from the flattened text output produced by the
    /// read-only dashboard PowerShell query (see the Infrastructure DashboardScriptBuilder). Pure
    /// transformation — parses the marker lines, correlates references, and runs orphan detection.
    /// Kept as an Application port so the assembly logic is unit-testable without a runspace.
    /// </summary>
    public interface ITenantTopologyAssembler
    {
        TenantTopology Assemble(string? rawOutput, DateTimeOffset retrievedAtUtc);
    }
}
