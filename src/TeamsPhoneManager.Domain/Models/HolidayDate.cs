namespace teams_phonemanager.Models
{
    /// <summary>
    /// Framework-free Domain value object for a holiday occurrence. Used by the inner layers
    /// (e.g. the holiday provider and script builders) where constructing the Presentation
    /// ObservableObject <c>HolidayEntry</c> would be an outward dependency.
    /// </summary>
    public sealed class HolidayDate : IHolidayEntry
    {
        public HolidayDate(DateTime date, TimeSpan time, string? name = null, DateTime? endDate = null, TimeSpan? endTime = null)
        {
            Date = date;
            Time = time;
            Name = name;
            EndDate = endDate;
            EndTime = endTime;
        }

        public DateTime Date { get; }
        public TimeSpan Time { get; }
        public string? Name { get; }
        public DateTime? EndDate { get; }
        public TimeSpan? EndTime { get; }

        public DateTime DateTime => Date.Date.Add(Time);
        public DateTime? EndDateTime => EndDate?.Date.Add(EndTime ?? TimeSpan.Zero);
        public bool HasEndDate => EndDate.HasValue;
    }
}
