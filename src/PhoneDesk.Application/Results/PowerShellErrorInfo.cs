namespace PhoneDesk.Services
{
    /// <summary>
    /// Structured view of a single PowerShell <c>ErrorRecord</c>, surfaced as a framework-free POCO
    /// so the Presentation layer never has to touch System.Management.Automation types.
    /// Captures the exception type, message and failing command instead of a single flattened string.
    /// </summary>
    public sealed class PowerShellErrorInfo
    {
        /// <summary>Full type name of the underlying exception (e.g. <c>System.Management.Automation.RuntimeException</c>).</summary>
        public string ExceptionType { get; init; } = string.Empty;

        /// <summary>Human-readable exception message.</summary>
        public string Message { get; init; } = string.Empty;

        /// <summary>The command (or invocation line) that produced the error, when available.</summary>
        public string FailingCommand { get; init; } = string.Empty;

        /// <summary>PowerShell category info string (e.g. <c>NotSpecified: (:) [], RuntimeException</c>).</summary>
        public string CategoryInfo { get; init; } = string.Empty;

        /// <summary>Raw <c>ErrorRecord.ToString()</c> for diagnostics / audit logging.</summary>
        public string RawText { get; init; } = string.Empty;
    }
}
