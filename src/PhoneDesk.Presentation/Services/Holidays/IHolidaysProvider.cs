using System;
using System.Collections.Generic;
using PhoneDesk.Models;

namespace PhoneDesk.Services.Holidays
{
    public interface IHolidaysProvider
    {
        IEnumerable<HolidayEntry> GetHolidays(string country, string? canton, string? district, int year);
    }
}


