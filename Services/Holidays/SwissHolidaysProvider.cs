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
                { "Dreikönigstag", SwissHolidayFormulaEvaluator.Epiphany },
                { "San Giuseppe", SwissHolidayFormulaEvaluator.StJosephsDay },
                { "Karfreitag", SwissHolidayFormulaEvaluator.GoodFriday },
                { "Ostermontag", SwissHolidayFormulaEvaluator.EasterMonday },
                { "Tag der Arbeit", SwissHolidayFormulaEvaluator.LabourDay },
                { "Auffahrt", SwissHolidayFormulaEvaluator.AscensionDay },
                { "Pfingstmontag", SwissHolidayFormulaEvaluator.WhitMonday },
                { "Fronleichnam", SwissHolidayFormulaEvaluator.CorpusChristi },
                { "San Pietro e Paolo", SwissHolidayFormulaEvaluator.StPeterAndPaul },
                { "Bundesfeier", SwissHolidayFormulaEvaluator.SwissNationalDay },
                { "Mariä Himmelfahrt", SwissHolidayFormulaEvaluator.AssumptionDay },
                { "Allerheiligen", SwissHolidayFormulaEvaluator.AllSaintsDay },
                { "Mariä Empfängnis", SwissHolidayFormulaEvaluator.ImmaculateConception },
                { "Weihnachtstag", SwissHolidayFormulaEvaluator.Christmas },
                { "Stephanstag", SwissHolidayFormulaEvaluator.StStephensDay },
                { "Näfelser Fahrt", SwissHolidayFormulaEvaluator.NaefelserFahrt },
                { "Jeûne genevois", SwissHolidayFormulaEvaluator.JeuneGenevois },
                { "Restauration de la République", SwissHolidayFormulaEvaluator.RestaurationRepublique }
            };

            // Get holidays for specific canton
            var cantonHolidays = GetCantonHolidays(canton, district);
            foreach (var name in cantonHolidays)
            {
                if (pool.TryGetValue(name, out var f))
                {
                    var date = f(year);
                    list.Add(new HolidayEntry(date, new TimeSpan(0, 0, 0), name));
                }
            }

            return list;
        }

        private static string[] GetCantonHolidays(string? canton, string? district)
        {
            if (string.IsNullOrEmpty(canton))
                return Array.Empty<string>();

            var cantonLower = canton.ToLowerInvariant();

            return cantonLower switch
            {
                "aargau" => GetAargauHolidays(district),
                "bern" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Weihnachtstag", "Stephanstag" },
                "basel-land" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Weihnachtstag", "Stephanstag" },
                "fribourg" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag", "Stephanstag" },
                "genf" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Jeûne genevois", "Weihnachtstag", "Restauration de la République" },
                "glarus" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Näfelser Fahrt", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Allerheiligen", "Weihnachtstag", "Stephanstag" },
                "luzern" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag", "Stephanstag" },
                "solothurn" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Weihnachtstag", "Stephanstag" },
                "schwyz" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag", "Stephanstag" },
                "tessin" => new[] { "Neujahr", "Dreikönigstag", "San Giuseppe", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Fronleichnam", "San Pietro e Paolo", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag", "Stephanstag" },
                "thurgau" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag", "Stephanstag" },
                "zug" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag", "Stephanstag" },
                "zürich" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Weihnachtstag", "Stephanstag" },
                _ => Array.Empty<string>()
            };
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


