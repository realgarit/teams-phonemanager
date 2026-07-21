using System.Collections.Generic;

namespace PhoneDesk.Services
{
    /// <summary>
    /// Raw outcome of running a PowerShell command through <see cref="Interfaces.IPowerShellContextService"/>:
    /// the flattened text output plus the structured error records captured from the error stream.
    /// This is the boundary DTO the Infrastructure layer produces; the Application layer maps it into
    /// a typed <see cref="OperationResult{T}"/>.
    /// </summary>
    public sealed class PowerShellExecutionResult
    {
        /// <summary>The combined text output (Information + Warning + result objects + flattened errors), unchanged.</summary>
        public string Output { get; init; } = string.Empty;

        /// <summary>True when the PowerShell pipeline reported errors (or a host-level exception occurred).</summary>
        public bool HadErrors { get; init; }

        /// <summary>Structured error records captured from the PowerShell error stream (or a host exception).</summary>
        public IReadOnlyList<PowerShellErrorInfo> Errors { get; init; } = System.Array.Empty<PowerShellErrorInfo>();
    }
}
