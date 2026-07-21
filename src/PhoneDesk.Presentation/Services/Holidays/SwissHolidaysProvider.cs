using System;
using System.Collections.Generic;
using System.Linq;
using PhoneDesk.Holidays;
using PhoneDesk.Models;

namespace PhoneDesk.Services.Holidays
{
    /// <summary>
    /// Thin Presentation adapter over the framework-free <see cref="SwissHolidayCatalog"/>.
    /// Maps the canton display strings used by the UI (German names or 2-letter codes) to the
    /// stable <see cref="SwissCanton"/> codes, invokes the Domain rule engine, and converts the
    /// Domain <see cref="HolidayDate"/> values into the bindable <see cref="HolidayEntry"/>.
    /// </summary>
    public class SwissHolidaysProvider : IHolidaysProvider
    {
        public IEnumerable<HolidayEntry> GetHolidays(string country, string? canton, string? district, int year)
            => GetHolidaysDetailed(country, canton, district, year).Holidays;

        /// <summary>
        /// Like <see cref="GetHolidays"/> but also returns the <see cref="HolidayResultCompleteness"/>
        /// so callers can warn the user when only a canton-wide intersection could be produced.
        /// </summary>
        public SwissHolidaysResult GetHolidaysDetailed(string country, string? canton, string? district, int year)
        {
            if (!string.Equals(country, "Switzerland", StringComparison.OrdinalIgnoreCase))
                return new SwissHolidaysResult(Array.Empty<HolidayEntry>(), HolidayResultCompleteness.Complete);

            var code = MapCanton(canton);
            var result = SwissHolidayCatalog.Compute(code, district, year);

            var entries = result.Holidays
                .Select(h => new HolidayEntry(h.Date, h.Time, h.Name))
                .ToList();

            return new SwissHolidaysResult(entries, result.Completeness);
        }

        /// <summary>
        /// Maps a canton display string (German UI name or 2-letter code) to a <see cref="SwissCanton"/>.
        /// Unknown or empty names throw <see cref="ArgumentOutOfRangeException"/> (spec 5.6 — no silent
        /// empty result).
        /// </summary>
        public static SwissCanton MapCanton(string? canton)
        {
            if (string.IsNullOrWhiteSpace(canton))
                throw new ArgumentOutOfRangeException(nameof(canton), canton, "Kein Schweizer Kanton angegeben");

            return canton.Trim().ToLowerInvariant() switch
            {
                "aargau" or "ag" => SwissCanton.AG,
                "appenzell-innerrhoden" or "appenzell innerrhoden" or "ai" => SwissCanton.AI,
                "appenzell-ausserrhoden" or "appenzell ausserrhoden" or "ar" => SwissCanton.AR,
                "bern" or "be" => SwissCanton.BE,
                "basel-land" or "basel-landschaft" or "baselland" or "bl" => SwissCanton.BL,
                "basel-stadt" or "baselstadt" or "bs" => SwissCanton.BS,
                "fribourg" or "freiburg" or "fr" => SwissCanton.FR,
                "genf" or "genève" or "geneva" or "ge" => SwissCanton.GE,
                "glarus" or "gl" => SwissCanton.GL,
                "graubünden" or "graubunden" or "gr" => SwissCanton.GR,
                "jura" or "ju" => SwissCanton.JU,
                "luzern" or "lu" => SwissCanton.LU,
                "neuenburg" or "neuchâtel" or "neuchatel" or "ne" => SwissCanton.NE,
                "nidwalden" or "nw" => SwissCanton.NW,
                "obwalden" or "ow" => SwissCanton.OW,
                "schaffhausen" or "sh" => SwissCanton.SH,
                "schwyz" or "sz" => SwissCanton.SZ,
                "solothurn" or "so" => SwissCanton.SO,
                "st. gallen" or "st.gallen" or "sankt gallen" or "sg" => SwissCanton.SG,
                "tessin" or "ticino" or "ti" => SwissCanton.TI,
                "thurgau" or "tg" => SwissCanton.TG,
                "uri" or "ur" => SwissCanton.UR,
                "waadt" or "vaud" or "vd" => SwissCanton.VD,
                "wallis" or "valais" or "vs" => SwissCanton.VS,
                "zug" or "zg" => SwissCanton.ZG,
                "zürich" or "zurich" or "zh" => SwissCanton.ZH,
                _ => throw new ArgumentOutOfRangeException(nameof(canton), canton, "Unbekannter Schweizer Kanton"),
            };
        }
    }

    /// <summary>
    /// Presentation-level result carrying the bindable holiday entries plus the completeness flag.
    /// </summary>
    public sealed record SwissHolidaysResult(
        IReadOnlyList<HolidayEntry> Holidays,
        HolidayResultCompleteness Completeness);
}
