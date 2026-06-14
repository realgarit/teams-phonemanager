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
                { "St. Josef", SwissHolidayFormulaEvaluator.StJosephsDay },
                { "Bruderklausenfest", SwissHolidayFormulaEvaluator.BruderklausenFest },
                { "Mauritiustag", SwissHolidayFormulaEvaluator.MauritiusTag },
                { "Lundi du Jeûne", SwissHolidayFormulaEvaluator.LundiDuJeune },
                { "Jahrestag Republik Neuenburg", SwissHolidayFormulaEvaluator.RepublicDayNeuchatel },
                { "Unabhängigkeitstag Jura", SwissHolidayFormulaEvaluator.JurassianIndependenceDay },
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
                "basel-land" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Weihnachtstag", "Stephanstag" },
                "fribourg" => new[] { "Neujahr", "Karfreitag", "Auffahrt", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag" },
                "genf" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Jeûne genevois", "Weihnachtstag", "Restauration de la République" },
                "glarus" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Näfelser Fahrt", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Allerheiligen", "Weihnachtstag", "Stephanstag" },
                "luzern" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag", "Stephanstag" },
                "solothurn" => new[] { "Neujahr", "St. Josef", "Karfreitag", "Tag der Arbeit", "Auffahrt", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Weihnachtstag" },
                "schwyz" => new[] { "Neujahr", "Dreikönigstag", "St. Josef", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag", "Stephanstag" },
                "tessin" => new[] { "Neujahr", "Dreikönigstag", "San Giuseppe", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Fronleichnam", "San Pietro e Paolo", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag", "Stephanstag" },
                "thurgau" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Weihnachtstag", "Stephanstag" },
                "zug" => new[] { "Neujahr", "Karfreitag", "Auffahrt", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag" },
                "zürich" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Weihnachtstag", "Stephanstag" },
                "uri" => new[] { "Neujahr", "Dreikönigstag", "St. Josef", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag", "Stephanstag" },
                "obwalden" => new[] { "Neujahr", "Karfreitag", "Auffahrt", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Bruderklausenfest", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag" },
                "nidwalden" => new[] { "Neujahr", "St. Josef", "Karfreitag", "Auffahrt", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag" },
                "basel-stadt" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Weihnachtstag", "Stephanstag" },
                "schaffhausen" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Weihnachtstag", "Stephanstag" },
                "appenzell-ausserrhoden" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Weihnachtstag", "Stephanstag" },
                "appenzell-innerrhoden" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Mauritiustag", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag", "Stephanstag" },
                "st. gallen" => new[] { "Neujahr", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Allerheiligen", "Weihnachtstag", "Stephanstag" },
                "graubünden" => new[] { "Neujahr", "Ostermontag", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Weihnachtstag", "Stephanstag" },
                "waadt" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Auffahrt", "Pfingstmontag", "Bundesfeier", "Lundi du Jeûne", "Weihnachtstag" },
                "wallis" => new[] { "Neujahr", "St. Josef", "Auffahrt", "Fronleichnam", "Bundesfeier", "Mariä Himmelfahrt", "Allerheiligen", "Mariä Empfängnis", "Weihnachtstag" },
                "neuenburg" => new[] { "Neujahr", "Jahrestag Republik Neuenburg", "Karfreitag", "Tag der Arbeit", "Auffahrt", "Bundesfeier", "Weihnachtstag" },
                "jura" => new[] { "Neujahr", "Berchtoldstag", "Karfreitag", "Ostermontag", "Tag der Arbeit", "Auffahrt", "Pfingstmontag", "Fronleichnam", "Bundesfeier", "Unabhängigkeitstag Jura", "Mariä Himmelfahrt", "Allerheiligen", "Weihnachtstag" },
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


