using PhoneDesk.Holidays;
using PhoneDesk.Services.Holidays;

namespace PhoneDesk.Tests;

/// <summary>Independent 2026 fixtures transcribed from the official sources in docs/swiss-holidays.md.</summary>
public class SwissHolidayCatalogTests
{
    [Theory]
    [InlineData(SwissCanton.AG, "2026-01-01,2026-04-03,2026-05-14,2026-08-01,2026-12-25")]
    [InlineData(SwissCanton.AI, "2026-01-01,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-06-04,2026-08-01,2026-12-25")]
    [InlineData(SwissCanton.AR, "2026-01-01,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-08-01,2026-12-25")]
    [InlineData(SwissCanton.BE, "2026-01-01,2026-01-02,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-08-01,2026-12-25,2026-12-26")]
    [InlineData(SwissCanton.BL, "2026-01-01,2026-04-03,2026-04-06,2026-05-01,2026-05-14,2026-05-25,2026-08-01,2026-12-25,2026-12-26")]
    [InlineData(SwissCanton.BS, "2026-01-01,2026-04-03,2026-04-06,2026-05-01,2026-05-14,2026-05-25,2026-08-01,2026-12-25,2026-12-26")]
    [InlineData(SwissCanton.FR, "2026-01-01,2026-04-03,2026-05-14,2026-08-01,2026-12-25")]
    [InlineData(SwissCanton.GE, "2026-01-01,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-08-01,2026-09-10,2026-12-25,2026-12-31")]
    [InlineData(SwissCanton.GL, "2026-01-01,2026-04-03,2026-04-06,2026-04-09,2026-05-14,2026-08-01,2026-11-01,2026-12-25,2026-12-26")]
    [InlineData(SwissCanton.GR, "2026-01-01,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-08-01,2026-12-25,2026-12-26")]
    [InlineData(SwissCanton.JU, "2026-01-01,2026-04-03,2026-04-06,2026-05-01,2026-05-14,2026-05-25,2026-06-04,2026-08-01,2026-12-25")]
    [InlineData(SwissCanton.LU, "2026-01-01,2026-04-03,2026-05-14,2026-06-04,2026-08-01,2026-08-15,2026-11-01,2026-12-25,2026-12-26")]
    [InlineData(SwissCanton.NE, "2026-01-01,2026-03-01,2026-04-03,2026-05-01,2026-05-14,2026-08-01,2026-12-25")]
    [InlineData(SwissCanton.NW, "2026-01-01,2026-04-03,2026-05-14,2026-06-04,2026-08-01,2026-08-15,2026-11-01,2026-12-08,2026-12-25")]
    [InlineData(SwissCanton.OW, "2026-01-01,2026-04-03,2026-05-14,2026-06-04,2026-08-01,2026-08-15,2026-11-01,2026-12-08,2026-12-25")]
    [InlineData(SwissCanton.SG, "2026-01-01,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-08-01,2026-11-01,2026-12-25,2026-12-26")]
    [InlineData(SwissCanton.SH, "2026-01-01,2026-04-03,2026-04-06,2026-05-01,2026-05-14,2026-05-25,2026-08-01,2026-12-25,2026-12-26")]
    [InlineData(SwissCanton.SO, "2026-01-01,2026-04-03,2026-05-01,2026-05-14,2026-08-01,2026-12-25")]
    [InlineData(SwissCanton.SZ, "2026-01-01,2026-03-19,2026-04-03,2026-05-14,2026-06-04,2026-08-01,2026-08-15,2026-11-01,2026-12-25")]
    [InlineData(SwissCanton.TG, "2026-01-01,2026-01-02,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-08-01,2026-12-25,2026-12-26")]
    [InlineData(SwissCanton.TI, "2026-01-01,2026-01-06,2026-04-06,2026-05-14,2026-08-01,2026-08-15,2026-11-01,2026-12-25,2026-12-26")]
    [InlineData(SwissCanton.UR, "2026-01-01,2026-04-03,2026-05-14,2026-06-04,2026-08-01,2026-08-15,2026-11-01,2026-12-08,2026-12-25")]
    [InlineData(SwissCanton.VD, "2026-01-01,2026-01-02,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-08-01,2026-09-21,2026-12-25")]
    [InlineData(SwissCanton.VS, "2026-01-01,2026-03-19,2026-05-14,2026-06-04,2026-08-01,2026-08-15,2026-11-01,2026-12-08,2026-12-25")]
    [InlineData(SwissCanton.ZG, "2026-01-01,2026-04-03,2026-05-14,2026-06-04,2026-08-01,2026-08-15,2026-11-01,2026-12-08,2026-12-25")]
    [InlineData(SwissCanton.ZH, "2026-01-01,2026-04-03,2026-04-06,2026-05-01,2026-05-14,2026-05-25,2026-08-01,2026-12-25,2026-12-26")]
    public void Statutory2026DatesMatchOfficialSources(SwissCanton canton, string expected)
    {
        var dates = SwissHolidayCatalog.Compute(canton, null, 2026).Holidays
            .Where(h => h.Category == HolidayCategory.Statutory).Select(h => h.Date.ToString("yyyy-MM-dd"));
        Assert.Equal(expected.Split(','), dates);
    }

    [Theory]
    [InlineData(SwissCanton.AI, "2026-08-15,2026-11-01,2026-12-08") ]
    [InlineData(SwissCanton.GL, "2026-05-25") ]
    [InlineData(SwissCanton.JU, "2026-01-02,2026-06-23,2026-08-15,2026-11-01") ]
    [InlineData(SwissCanton.LU, "2026-12-08") ]
    [InlineData(SwissCanton.NW, "2026-03-19") ]
    [InlineData(SwissCanton.OW, "2026-09-25") ]
    [InlineData(SwissCanton.SZ, "2026-01-06,2026-04-06,2026-05-25,2026-12-08,2026-12-26") ]
    [InlineData(SwissCanton.TG, "2026-05-01") ]
    [InlineData(SwissCanton.TI, "2026-03-19,2026-05-01,2026-05-25,2026-06-04,2026-06-29,2026-12-08") ]
    [InlineData(SwissCanton.UR, "2026-01-06,2026-03-19,2026-04-06,2026-05-25,2026-12-26") ]
    public void AdditionalCantonalRestDaysHaveTheirOwnCategory(SwissCanton canton, string expected)
    {
        var dates = SwissHolidayCatalog.Compute(canton, null, 2026).Holidays
            .Where(h => h.Category == HolidayCategory.AdditionalCantonalRestDay).Select(h => h.Date.ToString("yyyy-MM-dd"));
        Assert.Equal(expected.Split(','), dates);
    }

    [Theory]
    [InlineData(SwissCanton.FR, "fr-catholic", "2026-01-01,2026-04-03,2026-05-14,2026-06-04,2026-08-01,2026-08-15,2026-11-01,2026-12-08,2026-12-25") ]
    [InlineData(SwissCanton.FR, "fr-reformed", "2026-01-01,2026-01-02,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-08-01,2026-12-25,2026-12-26") ]
    [InlineData(SwissCanton.SO, "so-bucheggberg", "2026-01-01,2026-04-03,2026-05-01,2026-05-14,2026-08-01,2026-12-25") ]
    [InlineData(SwissCanton.SO, "so-other", "2026-01-01,2026-04-03,2026-05-01,2026-05-14,2026-06-04,2026-08-01,2026-08-15,2026-11-01,2026-12-25") ]
    [InlineData(SwissCanton.AG, "Aarau", "2026-01-01,2026-01-02,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-08-01,2026-12-25,2026-12-26") ]
    [InlineData(SwissCanton.AG, "Baden", "2026-01-01,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-06-04,2026-08-01,2026-12-25,2026-12-26") ]
    [InlineData(SwissCanton.AG, "Bremgarten", "2026-01-01,2026-04-03,2026-05-14,2026-06-04,2026-08-01,2026-08-15,2026-11-01,2026-12-25,2026-12-26") ]
    [InlineData(SwissCanton.AG, "Muri", "2026-01-01,2026-04-03,2026-05-14,2026-06-04,2026-08-01,2026-08-15,2026-11-01,2026-12-08,2026-12-25") ]
    [InlineData(SwissCanton.AG, "Rheinfelden", "2026-01-01,2026-04-03,2026-04-06,2026-05-14,2026-05-25,2026-08-01,2026-11-01,2026-12-25,2026-12-26") ]
    [InlineData(SwissCanton.AG, "Zurzach", "2026-01-01,2026-01-02,2026-04-03,2026-05-14,2026-06-04,2026-08-01,2026-11-01,2026-12-25,2026-12-26") ]
    public void RegionalSelectionsAreCompleteAndExact(SwissCanton canton, string region, string expected)
    {
        var result = SwissHolidayCatalog.Compute(canton, region, 2026);
        Assert.Equal(HolidayResultCompleteness.Complete, result.Completeness);
        Assert.Equal(expected.Split(','), result.Holidays.Where(h => h.Category == HolidayCategory.Statutory)
            .Select(h => h.Date.ToString("yyyy-MM-dd")));
    }

    [Theory]
    [InlineData(SwissCanton.AG)]
    [InlineData(SwissCanton.FR)]
    [InlineData(SwissCanton.SO)]
    public void MissingOrUnknownRegionNeverClaimsCompleteness(SwissCanton canton)
    {
        foreach (var region in new string?[] { null, "", "wrong-region" })
            Assert.Equal(HolidayResultCompleteness.CantonWideIntersectionOnly,
                SwissHolidayCatalog.Compute(canton, region, 2026).Completeness);
    }

    [Theory]
    [InlineData(2023, 4, 13)]
    [InlineData(2025, 4, 3)]
    [InlineData(2026, 4, 9)]
    [InlineData(2027, 4, 1)]
    public void NaefelserFahrtRespectsHolyWeek(int year, int month, int day)
        => Assert.Equal(new DateTime(year, month, day), SwissHolidayFormulaEvaluator.NaefelserFahrt(year));

    [Theory]
    [InlineData(2025, true)]
    [InlineData(2026, false)]
    [InlineData(2027, true)]
    [InlineData(2028, false)]
    [InlineData(2029, true)]
    [InlineData(2030, true)]
    public void BothAppenzellCantonsRespectStephanstagException(int year, bool included)
    {
        foreach (var canton in new[] { SwissCanton.AI, SwissCanton.AR })
            Assert.Equal(included, SwissHolidayCatalog.Compute(canton, null, year).Holidays
                .Any(h => h.Date == new DateTime(year, 12, 26)));
    }

    [Theory]
    [InlineData(2023, 1, 2)]
    [InlineData(2022, 12, 26)]
    [InlineData(2027, 8, 2)]
    public void GenevaDoesNotApplyNonLabourActEmployerSubstitutes(int year, int month, int day)
        => Assert.DoesNotContain(SwissHolidayCatalog.Compute(SwissCanton.GE, null, year).Holidays,
            h => h.Date == new DateTime(year, month, day));

    [Theory]
    [InlineData(2023, 1, 2, true)]
    [InlineData(2026, 1, 2, false)]
    [InlineData(2022, 12, 26, true)]
    [InlineData(2026, 12, 26, false)]
    public void NeuchatelKeepsItsDifferentSubstituteRule(int year, int month, int day, bool included)
        => Assert.Equal(included, SwissHolidayCatalog.Compute(SwissCanton.NE, null, year).Holidays
            .Any(h => h.Date == new DateTime(year, month, day)));

    [Fact]
    public void SolothurnMayDayStartsAtNoonInBothRegions()
    {
        foreach (var region in new[] { "so-bucheggberg", "so-other" })
            Assert.Equal(TimeSpan.FromHours(12), Assert.Single(SwissHolidayCatalog.Compute(SwissCanton.SO, region, 2026)
                .Holidays, h => h.Date == new DateTime(2026, 5, 1)).Time);
    }

    [Theory]
    [InlineData(2025, 4, 20)]
    [InlineData(2026, 4, 5)]
    [InlineData(2027, 3, 28)]
    [InlineData(2028, 4, 16)]
    [InlineData(2029, 4, 1)]
    [InlineData(2030, 4, 21)]
    public void MovableDatesMatchReferenceCalendar(int year, int month, int day)
    {
        var easter = new DateTime(year, month, day);
        Assert.Equal(easter, SwissHolidayFormulaEvaluator.EasterSunday(year));
        Assert.Equal(easter.AddDays(-2), SwissHolidayFormulaEvaluator.GoodFriday(year));
        Assert.Equal(easter.AddDays(1), SwissHolidayFormulaEvaluator.EasterMonday(year));
        Assert.Equal(easter.AddDays(39), SwissHolidayFormulaEvaluator.AscensionDay(year));
        Assert.Equal(easter.AddDays(50), SwissHolidayFormulaEvaluator.WhitMonday(year));
        Assert.Equal(easter.AddDays(60), SwissHolidayFormulaEvaluator.CorpusChristi(year));
    }

    [Fact]
    public void EveryCantonPreservesFederalHolidayWeekendsAndSundayClassification()
    {
        foreach (var canton in Enum.GetValues<SwissCanton>())
        foreach (var year in Enumerable.Range(2025, 6))
        {
            var result = SwissHolidayCatalog.Compute(canton, null, year);
            Assert.Single(result.Holidays, h => h.Date == new DateTime(year, 8, 1) && h.Category == HolidayCategory.Statutory);
            Assert.All(result.Holidays.Where(h => h.Category == HolidayCategory.SundayObservance),
                h => Assert.Equal(DayOfWeek.Sunday, h.Date.DayOfWeek));
            Assert.DoesNotContain(result.Holidays, h => h.Category == HolidayCategory.Custom);
            Assert.All(SwissHolidayCatalog.GetRules(canton, null).Rules, r => Assert.StartsWith("https://", r.Source.Url));
        }
    }
}
