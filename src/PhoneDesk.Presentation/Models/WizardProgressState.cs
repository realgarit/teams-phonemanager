using System.Collections.Generic;

namespace PhoneDesk.Models
{
    /// <summary>
    /// Guided Setup progress for the current app session. The Guided Setup page ViewModel is
    /// recreated on every navigation, so its step results live in the shared state. An operator
    /// can fix the Configuration and return to the step where they stopped.
    /// </summary>
    public sealed class WizardProgressState
    {
        public int CurrentStep { get; set; }

        public List<WizardStepSnapshot> Steps { get; } = new();
    }

    /// <param name="ConfigurationFingerprint">
    /// The names the provisioning scripts used (UPNs, group, call queue, auto attendant, number) when the
    /// step completed. A different current value means the step ran with earlier Configuration values.
    /// </param>
    public sealed record WizardStepSnapshot(
        bool IsCompleted,
        bool IsFailed,
        bool IsSkipped,
        string Result,
        string ErrorMessage,
        string ConfigurationFingerprint);
}
