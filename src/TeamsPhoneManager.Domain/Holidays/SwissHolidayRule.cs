using System;
using System.Collections.Generic;

namespace teams_phonemanager.Holidays
{
    /// <summary>
    /// A single holiday rule (spec 5.3). <see cref="Dates"/> returns <c>IEnumerable&lt;DateTime&gt;</c>
    /// so a rule can yield conditional substitute days (Geneva/Neuchâtel). <see cref="Applies"/>
    /// gates a whole rule for a given year (Appenzell Innerrhoden's conditional St Stephen's Day).
    /// </summary>
    public sealed record SwissHolidayRule(
        SwissHolidayId Id,
        Func<int, IEnumerable<DateTime>> Dates,
        LegalSource Source,
        string DisplayName,
        TimeSpan StartTime = default,
        bool IsRegional = false,
        Func<int, bool>? Applies = null);
}
