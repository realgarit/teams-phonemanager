namespace teams_phonemanager.Services
{
    /// <summary>
    /// Framework-free snapshot of PowerShell progress, forwarded from the Infrastructure layer to the
    /// Presentation layer via <see cref="System.IProgress{T}"/>. Produced by observing the PowerShell
    /// <c>Progress</c> stream (native <c>Write-Progress</c> records emitted by cmdlets) — it never changes
    /// the generated script text.
    ///
    /// When <see cref="PercentComplete"/> is negative the operation is indeterminate (the cmdlet did not
    /// report a percentage); otherwise it is a determinate value in the 0..100 range.
    /// </summary>
    public sealed record PowerShellProgress
    {
        /// <summary>High-level activity description (PowerShell <c>ProgressRecord.Activity</c>).</summary>
        public string Activity { get; init; } = string.Empty;

        /// <summary>Current status line (PowerShell <c>ProgressRecord.StatusDescription</c>).</summary>
        public string StatusDescription { get; init; } = string.Empty;

        /// <summary>The specific sub-operation in progress (PowerShell <c>ProgressRecord.CurrentOperation</c>).</summary>
        public string CurrentOperation { get; init; } = string.Empty;

        /// <summary>0..100 for determinate progress, or a negative value when indeterminate.</summary>
        public int PercentComplete { get; init; } = -1;

        /// <summary>True when no meaningful percentage is available and the UI should show an indeterminate indicator.</summary>
        public bool IsIndeterminate => PercentComplete < 0;

        /// <summary>
        /// A single human-readable line combining the non-empty parts, for direct display in a busy overlay.
        /// </summary>
        public string DisplayText
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>(3);
                if (!string.IsNullOrWhiteSpace(Activity)) parts.Add(Activity.Trim());
                if (!string.IsNullOrWhiteSpace(StatusDescription)) parts.Add(StatusDescription.Trim());
                if (!string.IsNullOrWhiteSpace(CurrentOperation)) parts.Add(CurrentOperation.Trim());
                return string.Join(" — ", parts);
            }
        }
    }
}
