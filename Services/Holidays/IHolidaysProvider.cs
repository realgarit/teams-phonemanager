using System;
using System.Collections.Generic;
using teams_phonemanager.Models;

namespace teams_phonemanager.Services.Holidays
{
    public interface IHolidaysProvider
    {
        IEnumerable<HolidayEntry> GetHolidays(string country, string? canton, string? district, int year);
    }
}


