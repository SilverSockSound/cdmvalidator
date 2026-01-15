namespace CDMValidation.Core.Models;

/// <summary>
/// Represents a CS01.01 Summary Record for claims with financial data.
/// </summary>
public class Cs01Record
{
    public int LineNumber { get; set; }

    // Field 1
    public string RecordType { get; set; } = string.Empty;

    // Field 2
    public string SummaryRecordId { get; set; } = string.Empty;

    // Field 3 - Optional
    public string? InvoiceReference { get; set; }

    // Field 4 - Conditional
    public string? RightsControllerName { get; set; }

    // Field 5 - Conditional
    public string? RightsControllerPartyId { get; set; }

    // Field 6 - Conditional
    public string? DistributionChannel { get; set; }

    // Field 7 - Conditional
    public string? DistributionChannelDPID { get; set; }

    // Field 8 - Conditional
    public string? StartOfClaimPeriod { get; set; }

    // Field 9 - Conditional
    public string? EndOfClaimPeriod { get; set; }

    // Field 10
    public string CommercialModel { get; set; } = string.Empty;

    // Field 11
    public string UseType { get; set; } = string.Empty;

    // Field 12
    public string Territory { get; set; } = string.Empty;

    // Field 13 - Conditional
    public string? ServiceDescription { get; set; }

    // Field 14 - Conditional
    public string? CurrencyOfReporting { get; set; }

    // Field 15 - Conditional
    public string? CurrencyOfInvoicing { get; set; }

    // Field 16 - Conditional
    public decimal? ExchangeRate { get; set; }

    // Field 17 - Conditional
    public string? ExchangeRateSource { get; set; }

    // Field 18 - Conditional
    public string? StartDateOfCurrencyExchange { get; set; }

    // Field 19 - Conditional
    public string? EndDateOfCurrencyExchange { get; set; }

    // Field 20
    public decimal RightsTypeSplitMechanical { get; set; }

    // Field 21
    public decimal RightsTypeSplitPerforming { get; set; }

    // Field 22 - Optional
    public decimal? TotalMarketShare { get; set; }

    // Field 23
    public decimal TotalClaimedAmount { get; set; }

    /// <summary>
    /// Parses a tab-delimited line into a Cs01Record.
    /// </summary>
    public static Cs01Record Parse(string line, int lineNumber)
    {
        var fields = line.Split('\t');
        var record = new Cs01Record { LineNumber = lineNumber };

        if (fields.Length > 0) record.RecordType = fields[0];
        if (fields.Length > 1) record.SummaryRecordId = fields[1];
        if (fields.Length > 2 && !string.IsNullOrWhiteSpace(fields[2])) record.InvoiceReference = fields[2];
        if (fields.Length > 3 && !string.IsNullOrWhiteSpace(fields[3])) record.RightsControllerName = fields[3];
        if (fields.Length > 4 && !string.IsNullOrWhiteSpace(fields[4])) record.RightsControllerPartyId = fields[4];
        if (fields.Length > 5 && !string.IsNullOrWhiteSpace(fields[5])) record.DistributionChannel = fields[5];
        if (fields.Length > 6 && !string.IsNullOrWhiteSpace(fields[6])) record.DistributionChannelDPID = fields[6];
        if (fields.Length > 7 && !string.IsNullOrWhiteSpace(fields[7])) record.StartOfClaimPeriod = fields[7];
        if (fields.Length > 8 && !string.IsNullOrWhiteSpace(fields[8])) record.EndOfClaimPeriod = fields[8];
        if (fields.Length > 9) record.CommercialModel = fields[9];
        if (fields.Length > 10) record.UseType = fields[10];
        if (fields.Length > 11) record.Territory = fields[11];
        if (fields.Length > 12 && !string.IsNullOrWhiteSpace(fields[12])) record.ServiceDescription = fields[12];
        if (fields.Length > 13 && !string.IsNullOrWhiteSpace(fields[13])) record.CurrencyOfReporting = fields[13];
        if (fields.Length > 14 && !string.IsNullOrWhiteSpace(fields[14])) record.CurrencyOfInvoicing = fields[14];
        if (fields.Length > 15 && !string.IsNullOrWhiteSpace(fields[15]) && decimal.TryParse(fields[15], out decimal exchangeRate))
            record.ExchangeRate = exchangeRate;
        if (fields.Length > 16 && !string.IsNullOrWhiteSpace(fields[16])) record.ExchangeRateSource = fields[16];
        if (fields.Length > 17 && !string.IsNullOrWhiteSpace(fields[17])) record.StartDateOfCurrencyExchange = fields[17];
        if (fields.Length > 18 && !string.IsNullOrWhiteSpace(fields[18])) record.EndDateOfCurrencyExchange = fields[18];
        if (fields.Length > 19 && decimal.TryParse(fields[19], out decimal mechSplit))
            record.RightsTypeSplitMechanical = mechSplit;
        if (fields.Length > 20 && decimal.TryParse(fields[20], out decimal perfSplit))
            record.RightsTypeSplitPerforming = perfSplit;
        if (fields.Length > 21 && !string.IsNullOrWhiteSpace(fields[21]) && decimal.TryParse(fields[21], out decimal marketShare))
            record.TotalMarketShare = marketShare;
        if (fields.Length > 22 && decimal.TryParse(fields[22], out decimal totalClaimed))
            record.TotalClaimedAmount = totalClaimed;

        return record;
    }
}
