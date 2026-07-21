using System.Collections.Generic;
using PhoneDesk.Models;
using PhoneDesk.Planning;

namespace PhoneDesk.Services.Interfaces
{
    /// <summary>
    /// Builds a structured <see cref="DryRunPlan"/> from configuration inputs. The plan is a read-only
    /// projection of what a run would create/change; building it performs no tenant mutation and executes
    /// no PowerShell. It derives entirely from <see cref="IPhoneManagerVariables"/> (the same inputs the
    /// frozen script builders consume) so the preview can never diverge from actual execution.
    /// </summary>
    public interface IDryRunPlanBuilder
    {
        /// <summary>Builds a single-entry plan for one configuration (used by the setup wizard).</summary>
        DryRunPlan BuildWizardPlan(IPhoneManagerVariables variables);

        /// <summary>Builds a multi-entry plan, one entry per configuration (used by bulk CSV operations).</summary>
        DryRunPlan BuildBulkPlan(IReadOnlyList<IPhoneManagerVariables> entries);
    }
}
