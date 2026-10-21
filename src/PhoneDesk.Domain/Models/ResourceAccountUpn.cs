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
        /// True when the fallback domain, with or without a leading "@", is a valid DNS domain: at most
        /// 253 characters, at least two labels of 1–63 ASCII letters, digits or hyphens without a leading
        /// or trailing hyphen, and a top-level domain of letters only (or an "xn--" IDN label).
        /// </summary>
        public static bool IsValidFallbackDomain(string? fallbackDomain)
        {
            var domain = BareDomain(fallbackDomain);
            if (domain.Length == 0 || domain.Length > 253)
            {
                return false;
            }

            var labels = domain.Split('.');
            if (labels.Length < 2 || !labels.All(IsValidLabel))
            {
                return false;
            }

            var topLevel = labels[^1];
            return topLevel.All(char.IsAsciiLetter) || topLevel.StartsWith("xn--", System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsValidLabel(string label)
            => label.Length is >= 1 and <= 63
                && label[0] != '-'
                && label[^1] != '-'
                && label.All(c => char.IsAsciiLetterOrDigit(c) || c == '-');

        private static string BareDomain(string? fallbackDomain)
        {
            var domain = (fallbackDomain ?? string.Empty).Trim();
            return domain.StartsWith('@') ? domain.Substring(1).Trim() : domain;
        }
    }
}
