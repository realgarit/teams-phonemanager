using System;

namespace PhoneDesk.Holidays
{
    /// <summary>
    /// Provenance of a holiday rule (spec section 9). Every rule must carry the official
    /// source URL and the date it was verified against that source, so the data set can be
    /// versioned and re-checked at least annually.
    /// </summary>
    public sealed record LegalSource(
        string Url,
        string Citation,
        DateOnly VerifiedAt,
        DateOnly? ValidFrom = null,
        DateOnly? ValidTo = null);
}
