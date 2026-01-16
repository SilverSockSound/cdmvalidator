namespace CDMValidation.Core.Models;

/// <summary>
/// Represents a CD01.01 Detail Record for claims with financial data.
/// </summary>
public class Cd01Record
{
    public int LineNumber { get; set; }

    // Field 1
    public string RecordType { get; set; } = string.Empty;

    // Field 2
    public string ClaimId { get; set; } = string.Empty;

    // Field 3
    public string SummaryRecordId { get; set; } = string.Empty;

    // Field 4
    public string DspResourceId { get; set; } = string.Empty;

    // Field 5 - Conditional
    public string? ResourceISRC { get; set; }

    // Field 6 - Conditional
    public string? ResourceTitle { get; set; }

    // Field 7 - Conditional
    public string? ResourceDisplayArtistName { get; set; }

    // Field 8 - Conditional
    public string? ResourceDisplayArtistPartyId { get; set; }

    // Field 9 - Conditional
    public string? LicensorWorkId { get; set; }

    // Field 10 - Conditional
    public string? MusicalWorkISWC { get; set; }

    // Field 11
    public string MusicalWorkTitle { get; set; } = string.Empty;

    // Field 12 - Optional (Multiple)
    public List<string> AlternativeMusicalWorkTitles { get; set; } = new();

    // Field 13 - Conditional (Multiple)
    public List<string> MusicalWorkComposerAuthorNames { get; set; } = new();

    // Field 14 - Optional (Multiple)
    public List<string> MusicalWorkComposerAuthorPartyIds { get; set; } = new();

    // Field 15
    public string ClaimBasis { get; set; } = string.Empty;

    // Field 16
    public decimal ShareClaimedMechanical { get; set; }

    // Field 17
    public decimal ShareClaimedPerforming { get; set; }

    // Field 18
    public decimal BlendedShareClaimedForMechAndPerf { get; set; }

    // Field 19 - Conditional
    public string? SalesTransactionId { get; set; }

    // Field 20
    public decimal Usages { get; set; }

    // Field 21 - Conditional
    public decimal? PercentageOfResourceInRelease { get; set; }

    // Field 22 - Conditional
    public decimal? GeneratedRevenueExcSalesTaxInCurrencyOfReporting { get; set; }

    // Field 23 - Conditional
    public decimal? GeneratedRevenueExcSalesTaxInCurrencyOfInvoicing { get; set; }

    // Field 24
    public decimal ClaimedAmountMechanical { get; set; }

    // Field 25
    public decimal ClaimedAmountPerforming { get; set; }

    // Field 26 - Optional (Multiple)
    public List<string> TariffParameterTypes { get; set; } = new();

    // Field 27 - Conditional (Multiple)
    public List<decimal> TariffParameterValues { get; set; } = new();

    // Field 28
    public decimal ClaimedAmount { get; set; }

    /// <summary>
    /// Parses a tab-delimited line into a Cd01Record.
    /// </summary>
    public static Cd01Record Parse(string line, int lineNumber)
    {
        var fields = line.Split('\t');
        var record = new Cd01Record { LineNumber = lineNumber };

        if (fields.Length > 0) record.RecordType = fields[0];
        if (fields.Length > 1) record.ClaimId = fields[1];
        if (fields.Length > 2) record.SummaryRecordId = fields[2];
        if (fields.Length > 3) record.DspResourceId = fields[3];
        if (fields.Length > 4 && !string.IsNullOrWhiteSpace(fields[4])) record.ResourceISRC = fields[4];
        if (fields.Length > 5 && !string.IsNullOrWhiteSpace(fields[5])) record.ResourceTitle = fields[5];
        if (fields.Length > 6 && !string.IsNullOrWhiteSpace(fields[6])) record.ResourceDisplayArtistName = fields[6];
        if (fields.Length > 7 && !string.IsNullOrWhiteSpace(fields[7])) record.ResourceDisplayArtistPartyId = fields[7];
        if (fields.Length > 8 && !string.IsNullOrWhiteSpace(fields[8])) record.LicensorWorkId = fields[8];
        if (fields.Length > 9 && !string.IsNullOrWhiteSpace(fields[9])) record.MusicalWorkISWC = fields[9];
        if (fields.Length > 10) record.MusicalWorkTitle = fields[10];

        // Field 12 - Multiple alternative titles
        if (fields.Length > 11 && !string.IsNullOrWhiteSpace(fields[11]))
            record.AlternativeMusicalWorkTitles = fields[11].Split('|').ToList();

        // Field 13 - Multiple composer/author names
        if (fields.Length > 12 && !string.IsNullOrWhiteSpace(fields[12]))
            record.MusicalWorkComposerAuthorNames = fields[12].Split('|').ToList();

        // Field 14 - Multiple composer/author party IDs
        if (fields.Length > 13 && !string.IsNullOrWhiteSpace(fields[13]))
            record.MusicalWorkComposerAuthorPartyIds = fields[13].Split('|').ToList();

        if (fields.Length > 14) record.ClaimBasis = fields[14];
        if (fields.Length > 15 && decimal.TryParse(fields[15], out decimal shareMech))
            record.ShareClaimedMechanical = shareMech;
        if (fields.Length > 16 && decimal.TryParse(fields[16], out decimal sharePerf))
            record.ShareClaimedPerforming = sharePerf;
        if (fields.Length > 17 && decimal.TryParse(fields[17], out decimal blendedShare))
            record.BlendedShareClaimedForMechAndPerf = blendedShare;
        if (fields.Length > 18 && !string.IsNullOrWhiteSpace(fields[18])) record.SalesTransactionId = fields[18];
        if (fields.Length > 19 && decimal.TryParse(fields[19], out decimal usages))
            record.Usages = usages;
        if (fields.Length > 20 && !string.IsNullOrWhiteSpace(fields[20]) && decimal.TryParse(fields[20], out decimal pctResource))
            record.PercentageOfResourceInRelease = pctResource;
        if (fields.Length > 21 && !string.IsNullOrWhiteSpace(fields[21]) && decimal.TryParse(fields[21], out decimal revReporting))
            record.GeneratedRevenueExcSalesTaxInCurrencyOfReporting = revReporting;
        if (fields.Length > 22 && !string.IsNullOrWhiteSpace(fields[22]) && decimal.TryParse(fields[22], out decimal revInvoicing))
            record.GeneratedRevenueExcSalesTaxInCurrencyOfInvoicing = revInvoicing;
        if (fields.Length > 23 && decimal.TryParse(fields[23], out decimal claimedMech))
            record.ClaimedAmountMechanical = claimedMech;
        if (fields.Length > 24 && decimal.TryParse(fields[24], out decimal claimedPerf))
            record.ClaimedAmountPerforming = claimedPerf;

        // Field 23 - Multiple tariff parameter types
        if (fields.Length > 25 && !string.IsNullOrWhiteSpace(fields[25]))
            record.TariffParameterTypes = fields[25].Split('|').ToList();

        // Field 24 - Multiple tariff parameter values
        if (fields.Length > 26 && !string.IsNullOrWhiteSpace(fields[26]))
        {
            var values = fields[26].Split('|');
            foreach (var value in values)
            {
                if (decimal.TryParse(value, out decimal tariffValue))
                    record.TariffParameterValues.Add(tariffValue);
            }
        }

        
        
        if (fields.Length > 27 && decimal.TryParse(fields[27], out decimal claimedAmt))
            record.ClaimedAmount = claimedAmt;

        return record;
    }
}
