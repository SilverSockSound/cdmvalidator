using CDMValidation.Core.Models;

namespace CDMValidation.Core.Parsers;

/// <summary>
/// Parses tab-delimited CDM files into structured record objects.
/// </summary>
public class CdmFileParser
{
    public CdmhRecord? HeaderRecord { get; private set; }
    public SrfoRecord? FooterRecord { get; private set; }
    public List<Cs01Record> SummaryRecords { get; private set; } = new();
    public List<Cd01Record> DetailRecords { get; private set; } = new();
    public List<string> IgnoredRecords { get; private set; } = new();
    public List<ValidationError> ParseErrors { get; private set; } = new();
    public int TotalLines { get; private set; }

    /// <summary>
    /// Parses a CDM file from the given file path.
    /// </summary>
    public void ParseFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            ParseErrors.Add(new ValidationError
            {
                LineNumber = 0,
                RecordType = "FILE",
                FieldName = "FilePath",
                ErrorMessage = $"File not found: {filePath}",
                Severity = ValidationSeverity.Error
            });
            return;
        }

        try
        {
            var lines = File.ReadAllLines(filePath);
            ParseLines(lines);
        }
        catch (Exception ex)
        {
            ParseErrors.Add(new ValidationError
            {
                LineNumber = 0,
                RecordType = "FILE",
                FieldName = "ParseError",
                ErrorMessage = $"Error reading file: {ex.Message}",
                Severity = ValidationSeverity.Error
            });
        }
    }

    /// <summary>
    /// Parses an array of lines into record objects.
    /// </summary>
    public void ParseLines(string[] lines)
    {
        TotalLines = lines.Length;

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            var line = lines[i];

            // Skip empty lines and comment lines (starting with #)
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;

            try
            {
                var recordType = GetRecordType(line);

                switch (recordType)
                {
                    case "CDMH.01":
                    case "CDMH":
                        HeaderRecord = CdmhRecord.Parse(line, lineNumber);
                        break;

                    case "SRFO":
                        FooterRecord = SrfoRecord.Parse(line, lineNumber);
                        break;

                    case "CS01.01":
                    case "CS01":
                        SummaryRecords.Add(Cs01Record.Parse(line, lineNumber));
                        break;

                    case "CD01.01":
                    case "CD01":
                        DetailRecords.Add(Cd01Record.Parse(line, lineNumber));
                        break;

                    case "CDDM":
                        // Ignore CDDM messages as per requirements
                        IgnoredRecords.Add(recordType);
                        break;

                    case var rt when rt.StartsWith("CDD"):
                        // Ignore all CDDM-related records (CDD1, CDD2.01, CDD3.01)
                        IgnoredRecords.Add(recordType);
                        break;

                    default:
                        ParseErrors.Add(new ValidationError
                        {
                            LineNumber = lineNumber,
                            RecordType = recordType,
                            FieldName = "RecordType",
                            ErrorMessage = $"Unknown or unsupported record type: {recordType}",
                            Severity = ValidationSeverity.Warning
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                ParseErrors.Add(new ValidationError
                {
                    LineNumber = lineNumber,
                    RecordType = "UNKNOWN",
                    FieldName = "Parse",
                    ErrorMessage = $"Error parsing line: {ex.Message}",
                    Severity = ValidationSeverity.Error
                });
            }
        }
    }

    /// <summary>
    /// Extracts the record type from a tab-delimited line.
    /// </summary>
    private string GetRecordType(string line)
    {
        var fields = line.Split('\t');
        return fields.Length > 0 ? fields[0] : string.Empty;
    }

    /// <summary>
    /// Resets the parser state for a new file.
    /// </summary>
    public void Reset()
    {
        HeaderRecord = null;
        FooterRecord = null;
        SummaryRecords.Clear();
        DetailRecords.Clear();
        IgnoredRecords.Clear();
        ParseErrors.Clear();
        TotalLines = 0;
    }
}
