using CommunityToolkit.Mvvm.ComponentModel;

namespace teams_phonemanager.Models
{
    public partial class HolidayEntry : ObservableObject
    {
        [ObservableProperty]
        private DateTime _date = DateTime.Now;

        [ObservableProperty]
        private TimeSpan _time = new TimeSpan(9, 0, 0); // Default to 9:00 AM

        [ObservableProperty]
        private string? _name; // Holiday name for predefined sets

        public HolidayEntry()
        {
        }

        public HolidayEntry(DateTime date, TimeSpan time)
        {
            Date = date;
            Time = time;
        }

        public HolidayEntry(DateTime date, TimeSpan time, string? name)
        {
            Date = date;
            Time = time;
            Name = name;
        }

        public DateTime DateTime => Date.Date.Add(Time);

        public string DisplayText => string.IsNullOrEmpty(Name) 
            ? $"{Date:dd.MM.yyyy} {Time:hh\\:mm}" 
            : $"{Date:dd.MM.yyyy} {Time:hh\\:mm} - {Name}";
    }
}
