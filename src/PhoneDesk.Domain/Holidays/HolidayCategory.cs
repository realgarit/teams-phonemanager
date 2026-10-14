namespace PhoneDesk.Holidays;

/// <summary>Legal scope of a suggested date, not a guarantee that a business is closed.</summary>
public enum HolidayCategory
{
    Custom = 0,
    Statutory = 1,
    AdditionalCantonalRestDay = 2,
    SundayObservance = 3
}
