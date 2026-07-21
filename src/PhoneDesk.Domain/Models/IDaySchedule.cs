namespace PhoneDesk.Models
{
    /// <summary>
    /// Read-only Domain contract for a single day's business-hours configuration.
    /// The Presentation <c>DaySchedule</c> (an ObservableObject, for two-way binding) implements
    /// this so the framework-free script builders can consume it without depending on the UI.
    /// </summary>
    public interface IDaySchedule
    {
        string DayName { get; }
        bool IsEnabled { get; }
        TimeSpan Hours1Start { get; }
        TimeSpan Hours1End { get; }
        TimeSpan Hours2Start { get; }
        TimeSpan Hours2End { get; }
        bool HasSecondRange { get; }
        bool HasEffectiveSecondRange { get; }
    }
}
