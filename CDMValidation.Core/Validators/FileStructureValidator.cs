using CDMValidation.Core.Models;
using CDMValidation.Core.Parsers;

namespace CDMValidation.Core.Validators;

/// <summary>
/// Validates the overall structure of a CDM file.
/// </summary>
public class FileStructureValidator
{
    public List<ValidationError> Validate(CdmFileParser parser)
    {
        var errors = new List<ValidationError>();

        // File must have a header record
        if (parser.HeaderRecord == null)
        {
            errors.Add(new ValidationError
            {
                LineNumber = 0,
                RecordType = "FILE",
                FieldName = "HeaderRecord",
                ErrorMessage = "File must contain a CDMH.01 header record",
                Severity = ValidationSeverity.Error
            });
        }

        // File must have a footer record
        if (parser.FooterRecord == null)
        {
            errors.Add(new ValidationError
            {
                LineNumber = 0,
                RecordType = "FILE",
                FieldName = "FooterRecord",
                ErrorMessage = "File must contain an SRFO footer record",
                Severity = ValidationSeverity.Error
            });
        }

        // Header must be the first record
        if (parser.HeaderRecord != null && parser.HeaderRecord.LineNumber != 1)
        {
            errors.Add(new ValidationError
            {
                LineNumber = parser.HeaderRecord.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "Position",
                ErrorMessage = $"CDMH.01 header record must be the first line, found at line {parser.HeaderRecord.LineNumber}",
                Severity = ValidationSeverity.Error
            });
        }

        // Footer must be the last non-empty, non-comment record
        // Note: We don't strictly enforce it's the last line since there may be trailing empty lines
        if (parser.FooterRecord != null)
        {
            // Check if there are any data records after the footer
            bool hasRecordsAfterFooter =
                (parser.HeaderRecord != null && parser.HeaderRecord.LineNumber > parser.FooterRecord.LineNumber) ||
                parser.SummaryRecords.Any(s => s.LineNumber > parser.FooterRecord.LineNumber) ||
                parser.DetailRecords.Any(d => d.LineNumber > parser.FooterRecord.LineNumber);

            if (hasRecordsAfterFooter)
            {
                errors.Add(new ValidationError
                {
                    LineNumber = parser.FooterRecord.LineNumber,
                    RecordType = "SRFO",
                    FieldName = "Position",
                    ErrorMessage = "SRFO footer record must be the last record (data records found after footer)",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        // Validate NumberOfLinesInReport matches actual line count
        if (parser.FooterRecord != null)
        {
            int expectedLines = parser.FooterRecord.NumberOfLinesInReport;
            int actualLines = parser.TotalLines;

            if (expectedLines != actualLines)
            {
                errors.Add(new ValidationError
                {
                    LineNumber = parser.FooterRecord.LineNumber,
                    RecordType = "SRFO",
                    FieldName = "NumberOfLinesInReport",
                    ErrorMessage = $"NumberOfLinesInReport ({expectedLines}) does not match actual line count ({actualLines})",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        // Validate NumberOfSummaryRecords matches actual count
        if (parser.FooterRecord != null)
        {
            int expectedSummary = parser.FooterRecord.NumberOfSummaryRecords;
            int actualSummary = parser.SummaryRecords.Count;

            if (expectedSummary != actualSummary)
            {
                errors.Add(new ValidationError
                {
                    LineNumber = parser.FooterRecord.LineNumber,
                    RecordType = "SRFO",
                    FieldName = "NumberOfSummaryRecords",
                    ErrorMessage = $"NumberOfSummaryRecords ({expectedSummary}) does not match actual summary record count ({actualSummary})",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        return errors;
    }
}
