using System;
using System.Collections.Generic;
using System.Linq;
using PhoneDesk.Models;
using PhoneDesk.Services.Holidays;

namespace PhoneDesk.Holidays
{
    /// <summary>
    /// Framework-free Swiss holiday catalog with explicit statutory, additional cantonal rest day
    /// and Sunday-observance categories. Official sources and regional scope are documented in
    /// docs/swiss-holidays.md. Unresolved AG/FR/SO regions return a flagged common subset;
    /// the presentation requires a complete regional choice before adding dates.
    /// </summary>
    public static class SwissHolidayCatalog
    {
        private static readonly DateOnly VerifiedAt = new DateOnly(2026, 10, 14);

        // ---- Formula map: standard holidays whose date is a pure function of the year. ----
        private static readonly Dictionary<SwissHolidayId, Func<int, DateTime>> Formula = new()
        {
            [SwissHolidayId.NewYear] = SwissHolidayFormulaEvaluator.NewYear,
            [SwissHolidayId.BerchtoldsDay] = SwissHolidayFormulaEvaluator.Berchtoldstag,
            [SwissHolidayId.Epiphany] = SwissHolidayFormulaEvaluator.Epiphany,
            [SwissHolidayId.StJoseph] = SwissHolidayFormulaEvaluator.StJosephsDay,
            [SwissHolidayId.GoodFriday] = SwissHolidayFormulaEvaluator.GoodFriday,
            [SwissHolidayId.EasterSunday] = SwissHolidayFormulaEvaluator.EasterSunday,
            [SwissHolidayId.EasterMonday] = SwissHolidayFormulaEvaluator.EasterMonday,
            [SwissHolidayId.LabourDay] = SwissHolidayFormulaEvaluator.LabourDay,
            [SwissHolidayId.Ascension] = SwissHolidayFormulaEvaluator.AscensionDay,
            [SwissHolidayId.PentecostSunday] = SwissHolidayFormulaEvaluator.PentecostSunday,
            [SwissHolidayId.WhitMonday] = SwissHolidayFormulaEvaluator.WhitMonday,
            [SwissHolidayId.CorpusChristi] = SwissHolidayFormulaEvaluator.CorpusChristi,
            [SwissHolidayId.StPeterAndPaul] = SwissHolidayFormulaEvaluator.StPeterAndPaul,
            [SwissHolidayId.NationalDay] = SwissHolidayFormulaEvaluator.SwissNationalDay,
            [SwissHolidayId.Assumption] = SwissHolidayFormulaEvaluator.AssumptionDay,
            [SwissHolidayId.FederalFast] = SwissHolidayFormulaEvaluator.FederalFast,
            [SwissHolidayId.AllSaints] = SwissHolidayFormulaEvaluator.AllSaintsDay,
            [SwissHolidayId.ImmaculateConception] = SwissHolidayFormulaEvaluator.ImmaculateConception,
            [SwissHolidayId.Christmas] = SwissHolidayFormulaEvaluator.Christmas,
            [SwissHolidayId.StStephensDay] = SwissHolidayFormulaEvaluator.StStephensDay,
            [SwissHolidayId.NaefelserFahrt] = SwissHolidayFormulaEvaluator.NaefelserFahrt,
            [SwissHolidayId.JeuneGenevois] = SwissHolidayFormulaEvaluator.JeuneGenevois,
            [SwissHolidayId.LundiDuJeune] = SwissHolidayFormulaEvaluator.LundiDuJeune,
            [SwissHolidayId.NeuchatelRepublicDay] = SwissHolidayFormulaEvaluator.RepublicDayNeuchatel,
            [SwissHolidayId.JuraIndependenceDay] = SwissHolidayFormulaEvaluator.JurassianIndependenceDay,
            [SwissHolidayId.MauritiusDay] = SwissHolidayFormulaEvaluator.MauritiusTag,
            [SwissHolidayId.BruderKlaus] = SwissHolidayFormulaEvaluator.BruderklausenFest,
            [SwissHolidayId.GenevaRestauration] = SwissHolidayFormulaEvaluator.RestaurationRepublique,
        };

        // ---- Default German / canton-appropriate display names (spec 3, 6.2). ----
        private static readonly Dictionary<SwissHolidayId, string> Names = new()
        {
            [SwissHolidayId.NewYear] = "Neujahr",
            [SwissHolidayId.BerchtoldsDay] = "Berchtoldstag",
            [SwissHolidayId.Epiphany] = "Dreikönigstag",
            [SwissHolidayId.StJoseph] = "Josefstag",
            [SwissHolidayId.GoodFriday] = "Karfreitag",
            [SwissHolidayId.EasterSunday] = "Ostersonntag",
            [SwissHolidayId.EasterMonday] = "Ostermontag",
            [SwissHolidayId.LabourDay] = "Tag der Arbeit",
            [SwissHolidayId.Ascension] = "Auffahrt",
            [SwissHolidayId.PentecostSunday] = "Pfingstsonntag",
            [SwissHolidayId.WhitMonday] = "Pfingstmontag",
            [SwissHolidayId.CorpusChristi] = "Fronleichnam",
            [SwissHolidayId.StPeterAndPaul] = "Peter und Paul",
            [SwissHolidayId.NationalDay] = "Bundesfeier",
            [SwissHolidayId.Assumption] = "Mariä Himmelfahrt",
            [SwissHolidayId.FederalFast] = "Eidg. Dank-, Buss- und Bettag",
            [SwissHolidayId.AllSaints] = "Allerheiligen",
            [SwissHolidayId.ImmaculateConception] = "Mariä Empfängnis",
            [SwissHolidayId.Christmas] = "Weihnachtstag",
            [SwissHolidayId.StStephensDay] = "Stephanstag",
            [SwissHolidayId.NaefelserFahrt] = "Näfelser Fahrt",
            [SwissHolidayId.JeuneGenevois] = "Jeûne genevois",
            [SwissHolidayId.LundiDuJeune] = "Lundi du Jeûne",
            [SwissHolidayId.NeuchatelRepublicDay] = "Jahrestag Republik Neuenburg",
            [SwissHolidayId.JuraIndependenceDay] = "Unabhängigkeitstag Jura",
            [SwissHolidayId.MauritiusDay] = "Mauritiustag",
            [SwissHolidayId.BruderKlaus] = "Bruderklausenfest",
            [SwissHolidayId.GenevaRestauration] = "Restauration de la République",
        };

        // ---- Official source URLs per canton (spec section 4). ----
        private static readonly Dictionary<SwissCanton, string> CantonUrl = new()
        {
            [SwissCanton.AG] = "https://gesetzessammlungen.ag.ch/app/de/texts_of_law/961.200",
            [SwissCanton.AI] = "https://ai.clex.ch/app/de/texts_of_law/822.200",
            [SwissCanton.AR] = "https://ar.clex.ch/app/de/texts_of_law/822.11",
            [SwissCanton.BE] = "https://www.belex.sites.be.ch/app/de/texts_of_law/555.1",
            [SwissCanton.BL] = "https://bl.clex.ch/app/de/texts_of_law/547",
            [SwissCanton.BS] = "https://www.gesetzessammlung.bs.ch/app/de/texts_of_law/811.100",
            [SwissCanton.FR] = "https://www.fr.ch/de/arbeit-und-unternehmen/arbeitnehmer/feiertage",
            [SwissCanton.GE] = "https://silgeneve.ch/legis/data/rsg_j1_45.htm",
            [SwissCanton.GL] = "https://www.gl.ch/verwaltung/volkswirtschaft-und-inneres/wirtschaft-und-arbeit/arbeit/arbeitsinspektorat/feiertagstagsregelung-sonntagsverkaeufe.html/999",
            [SwissCanton.GR] = "https://www.gr-lex.gr.ch/app/de/texts_of_law/520.100",
            [SwissCanton.JU] = "https://rsju.jura.ch/fr/viewdocument.html?download=1&id=38948&idn=20105",
            [SwissCanton.LU] = "https://srl.lu.ch/app/de/texts_of_law/855",
            [SwissCanton.NE] = "https://www.ne.ch/themes/economie-et-emploi/jours-feries-officiels",
            [SwissCanton.NW] = "https://gesetze.nw.ch/app/de/texts_of_law/921.1",
            [SwissCanton.OW] = "https://gdb.ow.ch/app/de/texts_of_law/975.2",
            [SwissCanton.SG] = "https://www.gesetzessammlung.sg.ch/app/de/texts_of_law/552.1",
            [SwissCanton.SH] = "https://rechtsbuch.sh.ch/app/de/texts_of_law/900.200",
            [SwissCanton.SO] = "https://bgs.so.ch/api/de/versions/649/pdf_file",
            [SwissCanton.SZ] = "https://www.sz.ch/public/upload/assets/36693/Feiertagsregelung_Kanton_Schwyz.pdf",
            [SwissCanton.TG] = "https://awa.tg.ch/public/upload/assets/190722/ArZ-Merkblatt_Feier-_und_Ruhetage_2024_-_2026_-_Version_f%C3%BCr_HP.pdf?fp=1",
            [SwissCanton.TI] = "https://m3.ti.ch/CAN/RLeggi/public/index.php/raccolta-leggi/legge/num/569",
            [SwissCanton.UR] = "https://www.ur.ch/_docn/404812/%C3%9Cbersicht_Feiertage_Kanton_Uri_gem._ArG_und_LSG.pdf",
            [SwissCanton.VD] = "https://www.vd.ch/fileadmin/user_upload/themes/economie_emploi/emploi/fichiers_pdf/FAQ-droit_du_travail-DGEM.pdf",
            [SwissCanton.VS] = "https://lex.vs.ch/app/fr/texts_of_law/822.100",
            [SwissCanton.ZG] = "https://zg.ch/de/wirtschaft-arbeit/arbeitsbedingungen/arbeits-und-ruhezeiten",
            [SwissCanton.ZH] = "https://www.zh.ch/de/wirtschaft-arbeit/arbeitsbedingungen/arbeitsssicherheit-gesundheitsschutz/arbeits-ruhezeiten/feiertage.html",
        };

        private static LegalSource Src(SwissCanton canton)
        {
            return new LegalSource(CantonUrl[canton], $"Kanton {canton} – amtliche Rechtsgrundlage", VerifiedAt);
        }

        /// <summary>
        /// Computes the holidays for a canton and year. <paramref name="district"/> is only
        /// meaningful for Aargau, Freiburg and Solothurn; it is ignored elsewhere.
        /// </summary>
        public static SwissHolidayResult Compute(SwissCanton canton, string? district, int year)
        {
            var (rules, completeness) = GetRules(canton, district);

            var produced = new List<(DateTime Date, TimeSpan Time, string Name, SwissHolidayId Id, HolidayCategory Category)>();
            foreach (var rule in rules)
            {
                if (rule.Applies != null && !rule.Applies(year))
                    continue;

                foreach (var date in rule.Dates(year))
                    produced.Add((date, rule.StartTime, rule.DisplayName, rule.Id, rule.Category));
            }

            // Spec 5.5: the 1st of August is always emitted, regardless of region resolution.
            if (rules.All(r => r.Id != SwissHolidayId.NationalDay))
            {
                produced.Add((SwissHolidayFormulaEvaluator.SwissNationalDay(year), TimeSpan.Zero,
                    Names[SwissHolidayId.NationalDay], SwissHolidayId.NationalDay, HolidayCategory.Statutory));
            }

            // Spec 5.10: de-duplicate by (date, id), then sort by date and start time.
            var holidays = produced
                .GroupBy(h => new { h.Date, h.Id })
                .Select(g => g.First())
                .OrderBy(h => h.Date)
                .ThenBy(h => h.Time)
                .Select(h => new HolidayDate(h.Date, h.Time, h.Name, category: h.Category))
                .ToList();

            return new SwissHolidayResult(holidays, completeness);
        }

        // ---------------------------------------------------------------------------------
        // Rule construction helpers
        // ---------------------------------------------------------------------------------

        private static SwissHolidayRule Rule(
            SwissCanton canton,
            SwissHolidayId id,
            TimeSpan? startTime = null,
            bool regional = false,
            Func<int, bool>? applies = null)
        {
            var formula = Formula[id];
            return new SwissHolidayRule(
                id,
                year => new[] { formula(year) },
                Src(canton),
                Names[id],
                startTime ?? TimeSpan.Zero,
                regional,
                applies,
                CategoryFor(canton, id));
        }

        private static SwissHolidayRule Rule(
            SwissCanton canton,
            SwissHolidayId id,
            Func<int, IEnumerable<DateTime>> dates,
            string? name = null)
            => new SwissHolidayRule(id, dates, Src(canton), name ?? Names[id], Category: CategoryFor(canton, id));

        // ---------------------------------------------------------------------------------
        // Conditional / substitute date generators (spec 5.8)
        // ---------------------------------------------------------------------------------

        /// <summary>The date, plus <paramref name="substitute"/> when the date falls on a Sunday.</summary>
        private static IEnumerable<DateTime> WithSundaySubstitute(DateTime date, DateTime substitute)
        {
            yield return date;
            if (date.DayOfWeek == DayOfWeek.Sunday)
                yield return substitute;
        }

        /// <summary>
        /// Spec 5.9: Appenzell Innerrhoden's St Stephen's Day (26 Dec) is a rest day only if it
        /// does not create three consecutive rest days. Rest days are Sundays plus AI holidays;
        /// 25 Dec is always a holiday, so three-in-a-row occurs iff 24 Dec is a Sunday (24-25-26)
        /// or 27 Dec is a Sunday (25-26-27) — i.e. exactly when 26 Dec is a Tuesday or a Saturday.
        /// </summary>
        private static bool IncludeAiStStephensDay(int year)
        {
            var dow = new DateTime(year, 12, 26).DayOfWeek;
            return dow != DayOfWeek.Tuesday && dow != DayOfWeek.Saturday;
        }

        // ---------------------------------------------------------------------------------
        // Canton rule sets (spec section 4)
        // ---------------------------------------------------------------------------------

        /// <summary>
        /// Returns the raw rule set (and completeness) for a canton/district. Exposed for
        /// verification (e.g. asserting every rule carries a <see cref="LegalSource"/>).
        /// </summary>
        public static (IReadOnlyList<SwissHolidayRule> Rules, HolidayResultCompleteness Completeness)
            GetRules(SwissCanton canton, string? district)
        {
            return canton switch
            {
                SwissCanton.AG => Aargau(district),
                SwissCanton.AI => (AppenzellInnerrhoden(), HolidayResultCompleteness.Complete),
                SwissCanton.AR => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.EasterSunday,
                    SwissHolidayId.EasterMonday, SwissHolidayId.Ascension, SwissHolidayId.PentecostSunday,
                    SwissHolidayId.WhitMonday, SwissHolidayId.NationalDay, SwissHolidayId.FederalFast,
                    SwissHolidayId.Christmas).Append(Rule(canton, SwissHolidayId.StStephensDay,
                        applies: IncludeAiStStephensDay)).ToList(), HolidayResultCompleteness.Complete),
                SwissCanton.BE => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.BerchtoldsDay, SwissHolidayId.GoodFriday,
                    SwissHolidayId.EasterSunday, SwissHolidayId.EasterMonday, SwissHolidayId.Ascension,
                    SwissHolidayId.PentecostSunday, SwissHolidayId.WhitMonday, SwissHolidayId.NationalDay,
                    SwissHolidayId.FederalFast, SwissHolidayId.Christmas, SwissHolidayId.StStephensDay),
                    HolidayResultCompleteness.Complete),
                SwissCanton.BL => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.EasterSunday,
                    SwissHolidayId.EasterMonday, SwissHolidayId.LabourDay, SwissHolidayId.Ascension,
                    SwissHolidayId.PentecostSunday, SwissHolidayId.WhitMonday, SwissHolidayId.NationalDay,
                    SwissHolidayId.FederalFast, SwissHolidayId.Christmas, SwissHolidayId.StStephensDay),
                    HolidayResultCompleteness.Complete),
                SwissCanton.BS => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.EasterSunday,
                    SwissHolidayId.EasterMonday, SwissHolidayId.LabourDay, SwissHolidayId.Ascension,
                    SwissHolidayId.PentecostSunday, SwissHolidayId.WhitMonday, SwissHolidayId.NationalDay,
                    SwissHolidayId.FederalFast, SwissHolidayId.Christmas, SwissHolidayId.StStephensDay),
                    HolidayResultCompleteness.Complete),
                SwissCanton.FR => Fribourg(district),
                SwissCanton.GE => (Geneva(), HolidayResultCompleteness.Complete),
                SwissCanton.GL => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.NaefelserFahrt, SwissHolidayId.GoodFriday,
                    SwissHolidayId.EasterSunday, SwissHolidayId.EasterMonday, SwissHolidayId.Ascension,
                    SwissHolidayId.PentecostSunday, SwissHolidayId.WhitMonday, SwissHolidayId.NationalDay,
                    SwissHolidayId.FederalFast, SwissHolidayId.AllSaints, SwissHolidayId.Christmas,
                    SwissHolidayId.StStephensDay), HolidayResultCompleteness.Complete),
                SwissCanton.GR => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.EasterSunday,
                    SwissHolidayId.EasterMonday, SwissHolidayId.Ascension, SwissHolidayId.PentecostSunday,
                    SwissHolidayId.WhitMonday, SwissHolidayId.NationalDay, SwissHolidayId.FederalFast,
                    SwissHolidayId.Christmas, SwissHolidayId.StStephensDay), HolidayResultCompleteness.Complete),
                SwissCanton.JU => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.BerchtoldsDay, SwissHolidayId.GoodFriday,
                    SwissHolidayId.EasterSunday, SwissHolidayId.EasterMonday, SwissHolidayId.LabourDay,
                    SwissHolidayId.Ascension, SwissHolidayId.PentecostSunday, SwissHolidayId.WhitMonday,
                    SwissHolidayId.CorpusChristi, SwissHolidayId.JuraIndependenceDay, SwissHolidayId.NationalDay,
                    SwissHolidayId.Assumption, SwissHolidayId.AllSaints, SwissHolidayId.Christmas),
                    HolidayResultCompleteness.Complete),
                SwissCanton.LU => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.EasterSunday,
                    SwissHolidayId.Ascension, SwissHolidayId.PentecostSunday, SwissHolidayId.CorpusChristi,
                    SwissHolidayId.NationalDay, SwissHolidayId.Assumption, SwissHolidayId.FederalFast,
                    SwissHolidayId.AllSaints, SwissHolidayId.ImmaculateConception, SwissHolidayId.Christmas,
                    SwissHolidayId.StStephensDay), HolidayResultCompleteness.Complete),
                SwissCanton.NE => (Neuchatel(), HolidayResultCompleteness.Complete),
                SwissCanton.NW => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.StJoseph, SwissHolidayId.GoodFriday,
                    SwissHolidayId.EasterSunday, SwissHolidayId.Ascension, SwissHolidayId.PentecostSunday,
                    SwissHolidayId.CorpusChristi, SwissHolidayId.NationalDay, SwissHolidayId.Assumption,
                    SwissHolidayId.FederalFast, SwissHolidayId.AllSaints, SwissHolidayId.ImmaculateConception,
                    SwissHolidayId.Christmas), HolidayResultCompleteness.Complete),
                SwissCanton.OW => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.EasterSunday,
                    SwissHolidayId.Ascension, SwissHolidayId.PentecostSunday, SwissHolidayId.CorpusChristi,
                    SwissHolidayId.NationalDay, SwissHolidayId.Assumption, SwissHolidayId.FederalFast,
                    SwissHolidayId.BruderKlaus, SwissHolidayId.AllSaints, SwissHolidayId.ImmaculateConception,
                    SwissHolidayId.Christmas), HolidayResultCompleteness.Complete),
                SwissCanton.SG => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.EasterSunday,
                    SwissHolidayId.EasterMonday, SwissHolidayId.Ascension, SwissHolidayId.PentecostSunday,
                    SwissHolidayId.WhitMonday, SwissHolidayId.NationalDay, SwissHolidayId.FederalFast,
                    SwissHolidayId.AllSaints, SwissHolidayId.Christmas, SwissHolidayId.StStephensDay),
                    HolidayResultCompleteness.Complete),
                SwissCanton.SH => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.EasterSunday,
                    SwissHolidayId.EasterMonday, SwissHolidayId.LabourDay, SwissHolidayId.Ascension,
                    SwissHolidayId.PentecostSunday, SwissHolidayId.WhitMonday, SwissHolidayId.NationalDay,
                    SwissHolidayId.FederalFast, SwissHolidayId.Christmas, SwissHolidayId.StStephensDay),
                    HolidayResultCompleteness.Complete),
                SwissCanton.SO => Solothurn(district),
                SwissCanton.SZ => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.Epiphany, SwissHolidayId.StJoseph,
                    SwissHolidayId.GoodFriday, SwissHolidayId.EasterSunday, SwissHolidayId.EasterMonday,
                    SwissHolidayId.Ascension, SwissHolidayId.PentecostSunday, SwissHolidayId.WhitMonday,
                    SwissHolidayId.CorpusChristi, SwissHolidayId.NationalDay, SwissHolidayId.Assumption,
                    SwissHolidayId.FederalFast, SwissHolidayId.AllSaints, SwissHolidayId.ImmaculateConception,
                    SwissHolidayId.Christmas, SwissHolidayId.StStephensDay), HolidayResultCompleteness.Complete),
                SwissCanton.TG => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.BerchtoldsDay, SwissHolidayId.GoodFriday,
                    SwissHolidayId.EasterSunday, SwissHolidayId.EasterMonday, SwissHolidayId.LabourDay,
                    SwissHolidayId.Ascension, SwissHolidayId.PentecostSunday, SwissHolidayId.WhitMonday,
                    SwissHolidayId.NationalDay, SwissHolidayId.FederalFast, SwissHolidayId.Christmas,
                    SwissHolidayId.StStephensDay), HolidayResultCompleteness.Complete),
                SwissCanton.TI => (Ticino(), HolidayResultCompleteness.Complete),
                SwissCanton.UR => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.Epiphany, SwissHolidayId.StJoseph,
                    SwissHolidayId.GoodFriday, SwissHolidayId.EasterMonday, SwissHolidayId.Ascension,
                    SwissHolidayId.WhitMonday, SwissHolidayId.CorpusChristi, SwissHolidayId.NationalDay,
                    SwissHolidayId.Assumption, SwissHolidayId.AllSaints, SwissHolidayId.ImmaculateConception,
                    SwissHolidayId.Christmas, SwissHolidayId.StStephensDay), HolidayResultCompleteness.Complete),
                SwissCanton.VD => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.BerchtoldsDay, SwissHolidayId.GoodFriday,
                    SwissHolidayId.EasterMonday, SwissHolidayId.Ascension, SwissHolidayId.WhitMonday,
                    SwissHolidayId.NationalDay, SwissHolidayId.LundiDuJeune, SwissHolidayId.Christmas),
                    HolidayResultCompleteness.Complete),
                SwissCanton.VS => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.StJoseph, SwissHolidayId.Ascension,
                    SwissHolidayId.CorpusChristi, SwissHolidayId.NationalDay, SwissHolidayId.Assumption,
                    SwissHolidayId.AllSaints, SwissHolidayId.ImmaculateConception, SwissHolidayId.Christmas),
                    HolidayResultCompleteness.Complete),
                SwissCanton.ZG => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.Ascension,
                    SwissHolidayId.CorpusChristi, SwissHolidayId.NationalDay, SwissHolidayId.Assumption,
                    SwissHolidayId.AllSaints, SwissHolidayId.ImmaculateConception, SwissHolidayId.Christmas),
                    HolidayResultCompleteness.Complete),
                SwissCanton.ZH => (Simple(canton,
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.EasterMonday,
                    SwissHolidayId.LabourDay, SwissHolidayId.Ascension, SwissHolidayId.WhitMonday,
                    SwissHolidayId.NationalDay, SwissHolidayId.Christmas, SwissHolidayId.StStephensDay),
                    HolidayResultCompleteness.Complete),
                _ => throw new ArgumentOutOfRangeException(nameof(canton), canton, "Unbekannter Schweizer Kanton"),
            };
        }

        // Legal categories are derived from the cantonal legislation, not the number of days.
        // See docs/swiss-holidays.md for the source and article for every canton.
        private static HolidayCategory CategoryFor(SwissCanton canton, SwissHolidayId id)
        {
            if (id is SwissHolidayId.EasterSunday or SwissHolidayId.PentecostSunday or SwissHolidayId.FederalFast)
                return HolidayCategory.SundayObservance;
            var additional = canton switch
            {
                SwissCanton.AI => id is SwissHolidayId.Assumption or SwissHolidayId.AllSaints or SwissHolidayId.ImmaculateConception,
                SwissCanton.GL => id == SwissHolidayId.WhitMonday,
                SwissCanton.JU => id is SwissHolidayId.BerchtoldsDay or SwissHolidayId.JuraIndependenceDay or SwissHolidayId.Assumption or SwissHolidayId.AllSaints,
                SwissCanton.LU => id == SwissHolidayId.ImmaculateConception,
                SwissCanton.NW => id == SwissHolidayId.StJoseph,
                SwissCanton.OW => id == SwissHolidayId.BruderKlaus,
                SwissCanton.SZ => id is SwissHolidayId.Epiphany or SwissHolidayId.EasterMonday or SwissHolidayId.WhitMonday or SwissHolidayId.ImmaculateConception or SwissHolidayId.StStephensDay,
                SwissCanton.TG => id == SwissHolidayId.LabourDay,
                SwissCanton.TI => id is SwissHolidayId.StJoseph or SwissHolidayId.LabourDay or SwissHolidayId.WhitMonday or SwissHolidayId.CorpusChristi or SwissHolidayId.StPeterAndPaul or SwissHolidayId.ImmaculateConception,
                SwissCanton.UR => id is SwissHolidayId.Epiphany or SwissHolidayId.StJoseph or SwissHolidayId.EasterMonday or SwissHolidayId.WhitMonday or SwissHolidayId.StStephensDay,
                _ => false
            };
            return additional ? HolidayCategory.AdditionalCantonalRestDay : HolidayCategory.Statutory;
        }

        private static (IReadOnlyList<SwissHolidayRule> Rules, HolidayResultCompleteness Completeness) Fribourg(string? region)
        {
            const SwissCanton c = SwissCanton.FR;
            var rules = Simple(c, SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.Ascension,
                SwissHolidayId.NationalDay, SwissHolidayId.Christmas).ToList();
            if (region == "fr-catholic")
                rules.AddRange(Simple(c, SwissHolidayId.CorpusChristi, SwissHolidayId.Assumption,
                    SwissHolidayId.AllSaints, SwissHolidayId.ImmaculateConception));
            else if (region == "fr-reformed")
                rules.AddRange(Simple(c, SwissHolidayId.BerchtoldsDay, SwissHolidayId.EasterMonday,
                    SwissHolidayId.WhitMonday, SwissHolidayId.StStephensDay));
            return (rules, region is "fr-catholic" or "fr-reformed"
                ? HolidayResultCompleteness.Complete : HolidayResultCompleteness.CantonWideIntersectionOnly);
        }

        private static IReadOnlyList<SwissHolidayRule> Simple(SwissCanton canton, params SwissHolidayId[] ids)
            => ids.Select(id => Rule(canton, id)).ToList();

        // ---- Appenzell Innerrhoden (spec 4 AI, 5.9): Mauritius excluded, conditional Stephanstag. ----
        private static IReadOnlyList<SwissHolidayRule> AppenzellInnerrhoden()
        {
            const SwissCanton c = SwissCanton.AI;
            return new List<SwissHolidayRule>
            {
                Rule(c, SwissHolidayId.NewYear),
                Rule(c, SwissHolidayId.GoodFriday),
                Rule(c, SwissHolidayId.EasterSunday),
                Rule(c, SwissHolidayId.EasterMonday),
                Rule(c, SwissHolidayId.Ascension),
                Rule(c, SwissHolidayId.PentecostSunday),
                Rule(c, SwissHolidayId.WhitMonday),
                Rule(c, SwissHolidayId.CorpusChristi),
                Rule(c, SwissHolidayId.NationalDay),
                Rule(c, SwissHolidayId.Assumption),
                Rule(c, SwissHolidayId.FederalFast),
                Rule(c, SwissHolidayId.AllSaints),
                Rule(c, SwissHolidayId.ImmaculateConception),
                Rule(c, SwissHolidayId.Christmas),
                Rule(c, SwissHolidayId.StStephensDay, applies: IncludeAiStStephensDay),
            };
        }

        // Geneva LJF Art. 1(2) substitutes apply only to employers outside the federal Labour Act.
        // They are customer-specific, never part of this general canton selection.
        private static IReadOnlyList<SwissHolidayRule> Geneva()
        {
            const SwissCanton c = SwissCanton.GE;
            return new List<SwissHolidayRule>
            {
                Rule(c, SwissHolidayId.NewYear),
                Rule(c, SwissHolidayId.GoodFriday),
                Rule(c, SwissHolidayId.EasterMonday),
                Rule(c, SwissHolidayId.Ascension),
                Rule(c, SwissHolidayId.WhitMonday),
                Rule(c, SwissHolidayId.NationalDay),
                Rule(c, SwissHolidayId.JeuneGenevois),
                Rule(c, SwissHolidayId.Christmas),
                Rule(c, SwissHolidayId.GenevaRestauration),
            };
        }

        // ---- Neuchâtel (spec 4 NE, 5.8): 2 Jan / 26 Dec substitutes when 1 Jan / 25 Dec is Sunday. ----
        private static IReadOnlyList<SwissHolidayRule> Neuchatel()
        {
            const SwissCanton c = SwissCanton.NE;
            return new List<SwissHolidayRule>
            {
                Rule(c, SwissHolidayId.NewYear,
                    y => WithSundaySubstitute(new DateTime(y, 1, 1), new DateTime(y, 1, 2))),
                Rule(c, SwissHolidayId.NeuchatelRepublicDay),
                Rule(c, SwissHolidayId.GoodFriday),
                Rule(c, SwissHolidayId.LabourDay),
                Rule(c, SwissHolidayId.Ascension),
                Rule(c, SwissHolidayId.NationalDay),
                Rule(c, SwissHolidayId.Christmas,
                    y => WithSundaySubstitute(new DateTime(y, 12, 25), new DateTime(y, 12, 26))),
            };
        }

        // Solothurn: 1 May from 12:00; three additional statutory holidays outside Bucheggberg.
        private static (IReadOnlyList<SwissHolidayRule> Rules, HolidayResultCompleteness Completeness) Solothurn(string? region)
        {
            const SwissCanton c = SwissCanton.SO;
            var rules = new List<SwissHolidayRule>
            {
                Rule(c, SwissHolidayId.NewYear),
                Rule(c, SwissHolidayId.GoodFriday),
                Rule(c, SwissHolidayId.EasterSunday),
                Rule(c, SwissHolidayId.LabourDay, startTime: new TimeSpan(12, 0, 0)),
                Rule(c, SwissHolidayId.Ascension),
                Rule(c, SwissHolidayId.PentecostSunday),
                Rule(c, SwissHolidayId.NationalDay),
                Rule(c, SwissHolidayId.FederalFast),
                Rule(c, SwissHolidayId.Christmas),
            };
            if (region == "so-other")
                rules.AddRange(Simple(c, SwissHolidayId.CorpusChristi, SwissHolidayId.Assumption, SwissHolidayId.AllSaints));
            return (rules, region is "so-bucheggberg" or "so-other"
                ? HolidayResultCompleteness.Complete : HolidayResultCompleteness.CantonWideIntersectionOnly);
        }

        // ---- Ticino (spec 4 TI): Italian names for St Joseph and Sts Peter & Paul. ----
        private static IReadOnlyList<SwissHolidayRule> Ticino()
        {
            const SwissCanton c = SwissCanton.TI;
            var src = Src(c);
            DateTime Single(SwissHolidayId id, int year) => Formula[id](year);
            SwissHolidayRule R(SwissHolidayId id, string? name = null) =>
                new SwissHolidayRule(id, y => new[] { Single(id, y) }, src, name ?? Names[id], Category: CategoryFor(c, id));

            return new List<SwissHolidayRule>
            {
                R(SwissHolidayId.NewYear),
                R(SwissHolidayId.Epiphany),
                R(SwissHolidayId.StJoseph, "San Giuseppe"),
                R(SwissHolidayId.EasterMonday),
                R(SwissHolidayId.LabourDay),
                R(SwissHolidayId.Ascension),
                R(SwissHolidayId.WhitMonday),
                R(SwissHolidayId.CorpusChristi),
                R(SwissHolidayId.StPeterAndPaul, "San Pietro e Paolo"),
                R(SwissHolidayId.NationalDay),
                R(SwissHolidayId.Assumption),
                R(SwissHolidayId.AllSaints),
                R(SwissHolidayId.ImmaculateConception),
                R(SwissHolidayId.Christmas),
                R(SwissHolidayId.StStephensDay),
            };
        }

        // ---- Aargau (spec 4 AG): 1 Aug always; no silent default; region flag. ----
        private static (IReadOnlyList<SwissHolidayRule> Rules, HolidayResultCompleteness Completeness)
            Aargau(string? district)
        {
            // Canton-wide intersection of the six district lists (verified against the current
            // provider): Neujahr, Karfreitag, Auffahrt, Bundesfeier, Weihnachtstag.
            IReadOnlyList<SwissHolidayRule> Intersection() => new List<SwissHolidayRule>
            {
                Rule(SwissCanton.AG, SwissHolidayId.NewYear),
                Rule(SwissCanton.AG, SwissHolidayId.GoodFriday),
                Rule(SwissCanton.AG, SwissHolidayId.Ascension),
                Rule(SwissCanton.AG, SwissHolidayId.NationalDay),
                Rule(SwissCanton.AG, SwissHolidayId.Christmas),
            };

            if (string.IsNullOrEmpty(district))
                return (Intersection(), HolidayResultCompleteness.CantonWideIntersectionOnly);

            // Extract the main district name from the display string ("Baden (ohne ...)" -> "baden").
            var main = district.Split('(')[0].Trim().ToLowerInvariant();

            SwissHolidayId[]? ids = main switch
            {
                "aarau" => new[]
                {
                    SwissHolidayId.NewYear, SwissHolidayId.BerchtoldsDay, SwissHolidayId.GoodFriday,
                    SwissHolidayId.EasterMonday, SwissHolidayId.Ascension, SwissHolidayId.WhitMonday,
                    SwissHolidayId.NationalDay, SwissHolidayId.Christmas, SwissHolidayId.StStephensDay,
                },
                "baden" => new[]
                {
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.EasterMonday,
                    SwissHolidayId.Ascension, SwissHolidayId.WhitMonday, SwissHolidayId.CorpusChristi,
                    SwissHolidayId.NationalDay, SwissHolidayId.Christmas, SwissHolidayId.StStephensDay,
                },
                "bremgarten" => new[]
                {
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.Ascension,
                    SwissHolidayId.CorpusChristi, SwissHolidayId.NationalDay, SwissHolidayId.Assumption,
                    SwissHolidayId.AllSaints, SwissHolidayId.Christmas, SwissHolidayId.StStephensDay,
                },
                "muri" => new[]
                {
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.Ascension,
                    SwissHolidayId.CorpusChristi, SwissHolidayId.NationalDay, SwissHolidayId.Assumption,
                    SwissHolidayId.AllSaints, SwissHolidayId.ImmaculateConception, SwissHolidayId.Christmas,
                },
                "rheinfelden" => new[]
                {
                    SwissHolidayId.NewYear, SwissHolidayId.GoodFriday, SwissHolidayId.EasterMonday,
                    SwissHolidayId.Ascension, SwissHolidayId.WhitMonday, SwissHolidayId.NationalDay,
                    SwissHolidayId.AllSaints, SwissHolidayId.Christmas, SwissHolidayId.StStephensDay,
                },
                "zurzach" => new[]
                {
                    SwissHolidayId.NewYear, SwissHolidayId.BerchtoldsDay, SwissHolidayId.GoodFriday,
                    SwissHolidayId.Ascension, SwissHolidayId.CorpusChristi, SwissHolidayId.NationalDay,
                    SwissHolidayId.AllSaints, SwissHolidayId.Christmas, SwissHolidayId.StStephensDay,
                },
                _ => null,
            };

            if (ids is null)
                // Unknown district: intersection + partial, never a silent default (spec 4 AG).
                return (Intersection(), HolidayResultCompleteness.CantonWideIntersectionOnly);

            // Known district: full district list, always including 1 August, flagged regional.
            var rules = ids.Select(id => Rule(SwissCanton.AG, id, regional: true)).ToList();
            if (ids.All(id => id != SwissHolidayId.NationalDay))
                rules.Add(Rule(SwissCanton.AG, SwissHolidayId.NationalDay, regional: true));
            return (rules, HolidayResultCompleteness.Complete);
        }
    }
}
