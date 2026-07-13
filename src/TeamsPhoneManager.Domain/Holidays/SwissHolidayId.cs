namespace teams_phonemanager.Holidays
{
    /// <summary>
    /// Stable technical identifiers for individual holidays (spec 5.2). Decouples the
    /// engine keys from the German/canton display names, so a name can change without
    /// breaking rule identity or de-duplication.
    /// </summary>
    /// <remarks>
    /// <see cref="GenevaRestauration"/> is an addition to the spec-5.2 list: the Geneva
    /// canton audit (spec section 4, GE) requires "Restauration de la République", which
    /// the enum listing in 5.2 omitted. Added here so GE can be implemented exactly.
    /// </remarks>
    public enum SwissHolidayId
    {
        NewYear,
        BerchtoldsDay,
        Epiphany,
        StJoseph,
        GoodFriday,
        EasterSunday,
        EasterMonday,
        LabourDay,
        Ascension,
        PentecostSunday,
        WhitMonday,
        CorpusChristi,
        StPeterAndPaul,
        NationalDay,
        Assumption,
        FederalFast,
        AllSaints,
        ImmaculateConception,
        Christmas,
        StStephensDay,
        NaefelserFahrt,
        JeuneGenevois,
        LundiDuJeune,
        NeuchatelRepublicDay,
        JuraIndependenceDay,
        MauritiusDay,
        BruderKlaus,
        GenevaRestauration
    }
}
