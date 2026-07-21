using System.Collections.Generic;
using PhoneDesk.Models;

namespace PhoneDesk.Holidays
{
    /// <summary>
    /// Output of the Swiss holiday engine: the computed, sorted and de-duplicated holidays for
    /// one canton and year, plus a <see cref="HolidayResultCompleteness"/> flag so callers can
    /// warn the user when the list is only a canton-wide intersection (spec 5.6).
    /// </summary>
    public sealed record SwissHolidayResult(
        IReadOnlyList<HolidayDate> Holidays,
        HolidayResultCompleteness Completeness);
}
