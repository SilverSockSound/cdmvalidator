namespace CDMValidation.Core.Models;

/// <summary>
/// Represents a CDMH.01 Header Record for CDM messages.
/// </summary>
public class CdmhRecord
{
    public int LineNumber { get; set; }

    // Field 1
    public string RecordType { get; set; } = string.Empty;

    // Field 2
    public string MessageVersion { get; set; } = string.Empty;

    // Field 3
    public string MessageId { get; set; } = string.Empty;

    // Field 4
    public string MessageCreatedDateTime { get; set; } = string.Empty;

    // Field 5
    public string Profile { get; set; } = string.Empty;

    // Field 6
    public string ProfileVersion { get; set; } = string.Empty;

    // Field 7 - Conditional
    public string? RelatedCdmMessageId { get; set; }

    // Field 8 - Conditional
    public string? SalesReportId { get; set; }

    // Field 9 - Conditional
    public string? UsageStartDate { get; set; }

    // Field 10 - Conditional
    public string? UsageEndDate { get; set; }

    // Field 11
    public string SenderPartyId { get; set; } = string.Empty;

    // Field 12
    public string SenderName { get; set; } = string.Empty;

    // Field 13 - Conditional
    public string? ServiceDescription { get; set; }

    // Field 14 - Optional
    public string? RecipientPartyId { get; set; }

    // Field 15 - Optional
    public string? RecipientName { get; set; }

    // Field 16 - Conditional
    public int? ClaimingRound { get; set; }

    // Field 17 - Conditional
    public string? SentOnBehalfOfPartyId { get; set; }

    // Field 18 - Conditional
    public string? SentOnBehalfOfName { get; set; }

    /// <summary>
    /// Parses a tab-delimited line into a CdmhRecord.
    /// </summary>
    public static CdmhRecord Parse(string line, int lineNumber)
    {
        var fields = line.Split('\t');
        var record = new CdmhRecord { LineNumber = lineNumber };

        if (fields.Length > 0) record.RecordType = fields[0];
        if (fields.Length > 1) record.MessageVersion = fields[1];
        if (fields.Length > 2) record.MessageId = fields[2];
        if (fields.Length > 3) record.MessageCreatedDateTime = fields[3];
        if (fields.Length > 4) record.Profile = fields[4];
        if (fields.Length > 5) record.ProfileVersion = fields[5];
        if (fields.Length > 6 && !string.IsNullOrWhiteSpace(fields[6])) record.RelatedCdmMessageId = fields[6];
        if (fields.Length > 7 && !string.IsNullOrWhiteSpace(fields[7])) record.SalesReportId = fields[7];
        if (fields.Length > 8 && !string.IsNullOrWhiteSpace(fields[8])) record.UsageStartDate = fields[8];
        if (fields.Length > 9 && !string.IsNullOrWhiteSpace(fields[9])) record.UsageEndDate = fields[9];
        if (fields.Length > 10) record.SenderPartyId = fields[10];
        if (fields.Length > 11) record.SenderName = fields[11];
        if (fields.Length > 12 && !string.IsNullOrWhiteSpace(fields[12])) record.ServiceDescription = fields[12];
        if (fields.Length > 13 && !string.IsNullOrWhiteSpace(fields[13])) record.RecipientPartyId = fields[13];
        if (fields.Length > 14 && !string.IsNullOrWhiteSpace(fields[14])) record.RecipientName = fields[14];
        if (fields.Length > 15 && !string.IsNullOrWhiteSpace(fields[15]) && int.TryParse(fields[15], out int claimingRound))
            record.ClaimingRound = claimingRound;
        if (fields.Length > 16 && !string.IsNullOrWhiteSpace(fields[16])) record.SentOnBehalfOfPartyId = fields[16];
        if (fields.Length > 17 && !string.IsNullOrWhiteSpace(fields[17])) record.SentOnBehalfOfName = fields[17];

        return record;
    }
}
