namespace PhoneDesk.Holidays
{
    /// <summary>
    /// Describes how complete a canton result is (spec 5.6). Cantons that legally differ by
    /// region (AG, FR, SO) must not claim canton-wide completeness without a region: they
    /// return only the canton-wide intersection and flag it here.
    /// </summary>
    public enum HolidayResultCompleteness
    {
        /// <summary>The list is the full, legally binding canton-wide set.</summary>
        Complete,

        /// <summary>Only the days that hold across the whole canton; region needed for the full list.</summary>
        CantonWideIntersectionOnly,

        /// <summary>A region/district must be supplied to produce any meaningful list.</summary>
        RegionRequired
    }
}
