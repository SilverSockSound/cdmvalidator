using CDMValidation.Core.Models;

namespace CDMValidation.Core.Validators;

/// <summary>
/// Validates SRFO Footer Records.
/// </summary>
public class SrfoValidator : IRecordValidator<SrfoRecord>
{
    public List<ValidationError> Validate(SrfoRecord record)
    {
        var errors = new List<ValidationError>();

        // Field 1: RecordType must be "SRFO"
        if (record.RecordType != "SRFO")
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "SRFO",
                FieldName = "RecordType",
                ErrorMessage = $"RecordType must be 'SRFO', found '{record.RecordType}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 2: NumberOfLinesInReport must be positive
        if (record.NumberOfLinesInReport <= 0)
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "SRFO",
                FieldName = "NumberOfLinesInReport",
                ErrorMessage = $"NumberOfLinesInReport must be positive, found {record.NumberOfLinesInReport}",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 3: NumberOfSummaryRecords must be non-negative
        if (record.NumberOfSummaryRecords < 0)
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "SRFO",
                FieldName = "NumberOfSummaryRecords",
                ErrorMessage = $"NumberOfSummaryRecords must be non-negative, found {record.NumberOfSummaryRecords}",
                Severity = ValidationSeverity.Error
            });
        }

        return errors;
    }
}
