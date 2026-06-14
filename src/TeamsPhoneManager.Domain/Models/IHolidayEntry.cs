namespace teams_phonemanager.Models
{
    /// <summary>
    /// Read-only Domain contract for a holiday occurrence (optionally a multi-day range).
    /// Implemented by the Presentation <c>HolidayEntry</c> (ObservableObject, for binding) and by
    /// the Domain <see cref="HolidayDate"/> POCO that the framework-free layers construct.
    /// </summary>
    public interface IHolidayEntry
    {
        DateTime Date { get; }
        TimeSpan Time { get; }
        string? Name { get; }
        DateTime? EndDate { get; }
        TimeSpan? EndTime { get; }
        DateTime DateTime { get; }
        DateTime? EndDateTime { get; }
        bool HasEndDate { get; }
    }
}
