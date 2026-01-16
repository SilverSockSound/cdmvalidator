namespace CDMValidation.Core.Constants;

/// <summary>
/// Contains allowed value sets (AVS) for CDM validation as defined in the DDEX standard.
/// </summary>
public static class AllowedValueSets
{
    /// <summary>
    /// Allowed Profile IDs for CDM messages.
    /// </summary>
    public static readonly HashSet<string> ProfileIds = new(StringComparer.OrdinalIgnoreCase)
    {
        "Claims",
        "ClaimsCorrections",
        "DataDiscrepancies",
        "OverclaimDiscrepancies",
        "BasicCDMPostUsage"
    };

    /// <summary>
    /// Allowed Commercial Model types.
    /// </summary>
    public static readonly HashSet<string> CommercialModelTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "AdvertisementSupportedModel",
        "PayAsYouGoModel",
        "SubscriptionModel",
        "FreeOfChargeModel",
        "Subscription"
    };

    /// <summary>
    /// Allowed Use Types for DSR.
    /// </summary>
    public static readonly HashSet<string> UseTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Stream",
        "PermanentDownload",
        "OnDemandStream",
        "NonInteractiveStream",
        "ConditionalDownload",
        "LimitedDownload",
        "ODStream"
    };

    /// <summary>
    /// Allowed Claim Basis values.
    /// </summary>
    public static readonly HashSet<string> ClaimBasisValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "Direct",
        "Indirect",
        "Unmatched",
        "Matched"
    };

    /// <summary>
    /// ISO 4217 Currency Codes (common subset).
    /// </summary>
    public static readonly HashSet<string> CurrencyCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "USD", "EUR", "GBP", "JPY", "AUD", "CAD", "CHF", "CNY", "SEK", "NZD",
        "MXN", "SGD", "HKD", "NOK", "KRW", "TRY", "INR", "RUB", "BRL", "ZAR",
        "DKK", "PLN", "THB", "MYR", "IDR", "HUF", "CZK", "ILS", "CLP", "PHP",
        "AED", "SAR", "TWD", "ARS", "COP", "RON", "VND", "EGP", "NGN", "PKR"
    };

    /// <summary>
    /// ISO 3166-1 alpha-2 Territory Codes (common subset).
    /// </summary>
    public static readonly HashSet<string> TerritoryCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "US", "GB", "CA", "AU", "DE", "FR", "IT", "ES", "JP", "CN", "IN", "BR",
        "MX", "KR", "RU", "NL", "SE", "NO", "DK", "FI", "BE", "AT", "CH", "IE",
        "NZ", "SG", "HK", "TH", "MY", "ID", "PH", "VN", "PL", "CZ", "HU", "RO",
        "TR", "IL", "ZA", "EG", "NG", "SA", "AE", "AR", "CL", "CO", "PE", "PT",
        "GR", "SK", "HR", "SI", "BG", "LT", "LV", "EE", "IS", "LU", "MT", "CY"
    };

    /// <summary>
    /// Pre-generated error message suffixes for better performance.
    /// </summary>
    public static class ErrorMessages
    {
        public static readonly string ProfileIdList = string.Join(", ", ProfileIds);
        public static readonly string CommercialModelTypesList = string.Join(", ", CommercialModelTypes);
        public static readonly string UseTypesList = string.Join(", ", UseTypes);
        public static readonly string ClaimBasisValuesList = string.Join(", ", ClaimBasisValues);
        public static readonly string CurrencyCodesList = string.Join(", ", CurrencyCodes);
        public static readonly string TerritoryCodesList = string.Join(", ", TerritoryCodes);
    }

    /// <summary>
    /// Validates if a value is in the allowed set.
    /// </summary>
    public static bool IsValidValue(HashSet<string> allowedSet, string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && allowedSet.Contains(value);
    }
}
