namespace CDMValidation.Core.Models;

/// <summary>
/// Represents an SRFO Footer Record for CDM messages.
/// </summary>
public class SrfoRecord
{
    public int LineNumber { get; set; }

    // Field 1
    public string RecordType { get; set; } = string.Empty;

    // Field 2
    public int NumberOfLinesInReport { get; set; }

    // Field 3
    public int NumberOfSummaryRecords { get; set; }

    /// <summary>
    /// Parses a tab-delimited line into an SrfoRecord.
    /// </summary>
    public static SrfoRecord Parse(string line, int lineNumber)
    {
        var fields = line.Split('\t');
        var record = new SrfoRecord { LineNumber = lineNumber };

        if (fields.Length > 0) record.RecordType = fields[0];
        if (fields.Length > 1 && int.TryParse(fields[1], out int linesCount))
            record.NumberOfLinesInReport = linesCount;
        if (fields.Length > 2 && int.TryParse(fields[2], out int summaryCount))
            record.NumberOfSummaryRecords = summaryCount;

        return record;
    }
}
