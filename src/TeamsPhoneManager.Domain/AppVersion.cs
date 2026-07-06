namespace teams_phonemanager.Models;

/// <summary>
/// A three-part semantic version with tolerant parsing for release tags
/// ("v3.17.0", "3.17.0") and the display constant ("Version 3.17.0").
/// </summary>
public readonly record struct AppVersion(int Major, int Minor, int Patch) : IComparable<AppVersion>
{
    public static bool TryParse(string? text, out AppVersion version)
    {
        version = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        var trimmed = text.Trim();
        // Strip any leading non-digit prefix ("v", "Version ", ...).
        var start = 0;
        while (start < trimmed.Length && !char.IsAsciiDigit(trimmed[start]))
        {
            start++;
        }

        var parts = trimmed[start..].Split('.');
        if (parts.Length is < 2 or > 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var major) ||
            !int.TryParse(parts[1], out var minor))
        {
            return false;
        }

        var patch = 0;
        if (parts.Length == 3 && !int.TryParse(parts[2], out patch))
        {
            return false;
        }

        version = new AppVersion(major, minor, patch);
        return true;
    }

    public int CompareTo(AppVersion other)
    {
        var major = Major.CompareTo(other.Major);
        if (major != 0)
        {
            return major;
        }

        var minor = Minor.CompareTo(other.Minor);
        return minor != 0 ? minor : Patch.CompareTo(other.Patch);
    }

    public bool IsNewerThan(AppVersion other) => CompareTo(other) > 0;

    public override string ToString() => $"{Major}.{Minor}.{Patch}";
}
