using System;
using System.Collections.Generic;
using teams_phonemanager.Models;
using teams_phonemanager.Services.Holidays;

namespace teams_phonemanager.Services.Holidays
{
    public class SwissHolidaysProvider : IHolidaysProvider
    {
        public IEnumerable<HolidayEntry> GetHolidays(string country, string? canton, string? district, int year)
        {
            if (!string.Equals(country, "Switzerland", StringComparison.OrdinalIgnoreCase))
                return Array.Empty<HolidayEntry>();

            var list = new List<HolidayEntry>();

            // General Swiss holiday pool (programmatic formulas)
            var pool = new Dictionary<string, Func<int, DateTime>>
            {
                { "Neujahr", SwissHolidayFormulaEvaluator.NewYear },
                { "Berchtoldstag", SwissHolidayFormulaEvaluator.Berchtoldstag },
                { "Karfreitag", SwissHolidayFormulaEvaluator.GoodFriday },
                { "Ostermontag", SwissHolidayFormulaEvaluator.EasterMonday },
                { "Auffahrt", SwissHolidayFormulaEvaluator.AscensionDay },
                { "Pfingstmontag", SwissHolidayFormulaEvaluator.WhitMonday },
                { "Fronleichnam", SwissHolidayFormulaEvaluator.CorpusChristi },
                { "Bundesfeier", SwissHolidayFormulaEvaluator.SwissNationalDay },
                { "Mariä Himmelfahrt", SwissHolidayFormulaEvaluator.AssumptionDay },
                { "Allerheiligen", SwissHolidayFormulaEvaluator.AllSaintsDay },
                { "Mariä Empfängnis", SwissHolidayFormulaEvaluator.ImmaculateConception },
                { "Weihnachtstag", SwissHolidayFormulaEvaluator.Christmas },
                { "Stephanstag", SwissHolidayFormulaEvaluator.StStephensDay }
            };

            // Selection for Luzern as requested (from the provided list)
            if (string.Equals(canton, "Luzern", StringComparison.OrdinalIgnoreCase))
            {
                string[] lucerne = new[]
                {
                    "Neujahr",
                    "Berchtoldstag",
                    "Karfreitag",
                    "Ostermontag",
                    "Auffahrt",
                    "Pfingstmontag",
                    "Fronleichnam",
                    "Bundesfeier",
                    "Mariä Himmelfahrt",
                    "Allerheiligen",
                    "Weihnachtstag",
                    "Stephanstag"
                };

                foreach (var name in lucerne)
                {
                    if (pool.TryGetValue(name, out var f))
                    {
                        var date = f(year);
                        list.Add(new HolidayEntry(date, new TimeSpan(0, 0, 0), name));
                    }
                }
            }
            // Selection for Aargau Bezirke
            else if (string.Equals(canton, "Aargau", StringComparison.OrdinalIgnoreCase))
            {
                var aargauHolidays = GetAargauHolidays(district);
                foreach (var name in aargauHolidays)
                {
                    if (pool.TryGetValue(name, out var f))
                    {
                        var date = f(year);
                        list.Add(new HolidayEntry(date, new TimeSpan(0, 0, 0), name));
                    }
                }
            }
            else
            {
                // For other cantons we can later plug in their specific sets
            }

            return list;
        }

        private static string[] GetAargauHolidays(string? district)
        {
            if (string.IsNullOrEmpty(district))
                return Array.Empty<string>();

            // Extract the main district name from the display string
            var mainDistrict = district.Split('(')[0].Trim().ToLowerInvariant();

            return mainDistrict switch
            {
                "aarau" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Weihnachtstag", "Stephanstag" },
                "baden" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Weihnachtstag", "Stephanstag" },
                "bremgarten" => new[] { "Neujahr", "Karfreitag", "Auffahrt", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Weihnachtstag", "Stephanstag" },
                "muri" => new[] { "Neujahr", "Karfreitag", "Auffahrt", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag" },
                "rheinfelden" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Allerheiligen", "Weihnachtstag", "Stephanstag" },
                "zurzach" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Auffahrt", "Fronleichnam", "Bundesfeier", "Allerheiligen", "Weihnachtstag", "Stephanstag" },
                // Default fallback
                _ => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Weihnachtstag", "Stephanstag" }
            };
        }
    }
}


