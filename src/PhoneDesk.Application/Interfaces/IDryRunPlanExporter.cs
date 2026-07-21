using PhoneDesk.Planning;

namespace PhoneDesk.Services.Interfaces
{
    /// <summary>
    /// Serializes a <see cref="DryRunPlan"/> into a portable document for change-approval workflows.
    /// Pure text projection — produces a string, performs no file or network IO.
    /// </summary>
    public interface IDryRunPlanExporter
    {
        /// <summary>Serializes the plan as indented JSON.</summary>
        string ToJson(DryRunPlan plan);

        /// <summary>Serializes the plan as CSV, one row per planned object.</summary>
        string ToCsv(DryRunPlan plan);
    }
}
