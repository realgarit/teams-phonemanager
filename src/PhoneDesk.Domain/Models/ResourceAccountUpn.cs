using System.Linq;

namespace PhoneDesk.Models
{
    /// <summary>
    /// Builds resource account UPNs from a local part and the Microsoft fallback domain.
    /// Operators enter the domain both as "@contoso.onmicrosoft.com" and as
    /// "contoso.onmicrosoft.com"; both must produce "local@contoso.onmicrosoft.com".
    /// </summary>
    public static class ResourceAccountUpn
    {
        /// <summary>
        /// Returns the "@domain" suffix for a fallback domain, or an empty string when no domain is set.
        /// </summary>
        public static string DomainSuffix(string? fallbackDomain)
        {
            var domain = BareDomain(fallbackDomain);
            return domain.Length == 0 ? string.Empty : "@" + domain;
        }

        public static string Build(string localPart, string? fallbackDomain)
            => localPart + DomainSuffix(fallbackDomain);

        /// <summary>
        /// True when the fallback domain, with or without a leading "@", is a plausible DNS domain.
        /// </summary>
        public static bool IsValidFallbackDomain(string? fallbackDomain)
        {
            var domain = BareDomain(fallbackDomain);
            if (domain.Length == 0 || domain.Contains('@') || domain.Any(char.IsWhiteSpace))
            {
                return false;
            }

            var labels = domain.Split('.');
            return labels.Length >= 2 && labels.All(label => label.Length > 0);
        }

        private static string BareDomain(string? fallbackDomain)
        {
            var domain = (fallbackDomain ?? string.Empty).Trim();
            return domain.StartsWith('@') ? domain.Substring(1).Trim() : domain;
        }
    }
}
