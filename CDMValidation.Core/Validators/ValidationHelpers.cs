using System.Globalization;
using System.Text.RegularExpressions;

namespace CDMValidation.Core.Validators;

/// <summary>
/// Provides helper methods for validating CDM field formats.
/// </summary>
public static partial class ValidationHelpers
{
    // Regex patterns for format validation
    [GeneratedRegex(@"^[a-zA-Z]{2}[a-zA-Z0-9]{3}\d{7}$")]
    private static partial Regex IsrcPattern();

    [GeneratedRegex(@"^T\d{10}$")]
    private static partial Regex IswcPattern();

    [GeneratedRegex(@"^PADPIDA\w+$")]
    private static partial Regex DpidPattern();

    [GeneratedRegex(@"^[^:]+::[^:]+$")]
    private static partial Regex PartyIdPattern();

    [GeneratedRegex(@"^CDM/[\d.]+/[\d.]+/[\d.]+$")]
    private static partial Regex MessageVersionPattern();

    /// <summary>
    /// Validates an ISRC format (2 chars + 3 alphanumeric + 2 digits + 5 digits).
    /// </summary>
    public static bool IsValidIsrc(string? isrc)
    {
        return !string.IsNullOrWhiteSpace(isrc) && IsrcPattern().IsMatch(isrc);
    }

    /// <summary>
    /// Validates an ISWC format (T + 10 digits, no dashes).
    /// </summary>
    public static bool IsValidIswc(string? iswc)
    {
        return !string.IsNullOrWhiteSpace(iswc) && IswcPattern().IsMatch(iswc);
    }

    /// <summary>
    /// Validates a DDEX Party ID (DPID) format.
    /// </summary>
    public static bool IsValidDpid(string? dpid)
    {
        return !string.IsNullOrWhiteSpace(dpid) && DpidPattern().IsMatch(dpid);
    }

    /// <summary>
    /// Validates a PartyID format (NAMESPACE::VALUE).
    /// </summary>
    public static bool IsValidPartyId(string? partyId)
    {
        return !string.IsNullOrWhiteSpace(partyId) && PartyIdPattern().IsMatch(partyId);
    }

    /// <summary>
    /// Validates CDM Message Version format (CDM/x.x/x.x/x.x).
    /// </summary>
    public static bool IsValidMessageVersion(string? version)
    {
        return !string.IsNullOrWhiteSpace(version) && MessageVersionPattern().IsMatch(version);
    }

    /// <summary>
    /// Validates ISO 8601 date format (YYYY-MM-DD or YYYY-MM or YYYY) or DD/MM/YYYY format.
    /// </summary>
    public static bool IsValidIsoDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return false;

        // Try YYYY-MM-DD
        if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out _))
            return true;

        // Try DD/MM/YYYY
        if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out _))
            return true;

        // Try YYYY-MM
        if (date.Length == 7 && DateTime.TryParseExact(date, "yyyy-MM", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out _))
            return true;

        // Try YYYY
        if (date.Length == 4 && int.TryParse(date, out int year) && year >= 1900 && year <= 2100)
            return true;

        return false;
    }

    /// <summary>
    /// Validates RFC 3339 datetime format (YYYY-MM-DDThh:mm:ssTZD).
    /// </summary>
    public static bool IsValidRfc3339DateTime(string? dateTime)
    {
        if (string.IsNullOrWhiteSpace(dateTime))
            return false;

        return DateTime.TryParse(dateTime, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out _);
    }

    /// <summary>
    /// Validates that a decimal is within the specified range.
    /// </summary>
    public static bool IsInRange(decimal value, decimal min, decimal max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates that a string does not contain spaces or underscores.
    /// </summary>
    public static bool HasNoSpacesOrUnderscores(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               !value.Contains(' ') &&
               !value.Contains('_');
    }

    /// <summary>
    /// Parses an ISO date string to DateTime for comparison.
    /// </summary>
    public static DateTime? ParseIsoDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return null;

        if (DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out var result))
            return result;

        if (DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out result))
            return result;

        if (DateTime.TryParseExact(date, "yyyy-MM", CultureInfo.InvariantCulture,
            DateTimeStyles.None, out result))
            return result;

        if (date.Length == 4 && int.TryParse(date, out int year))
            return new DateTime(year, 1, 1);

        return null;
    }

    /// <summary>
    /// Validates that two decimals are approximately equal (within tolerance).
    /// </summary>
    public static bool AreApproximatelyEqual(decimal value1, decimal value2, decimal tolerance = 0.01m)
    {
        return Math.Abs(value1 - value2) <= tolerance;
    }
}
