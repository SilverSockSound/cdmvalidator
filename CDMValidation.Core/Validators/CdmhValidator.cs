using CDMValidation.Core.Constants;
using CDMValidation.Core.Models;

namespace CDMValidation.Core.Validators;

/// <summary>
/// Validates CDMH.01 Header Records.
/// </summary>
public class CdmhValidator : IRecordValidator<CdmhRecord>
{
    public List<ValidationError> Validate(CdmhRecord record)
    {
        var errors = new List<ValidationError>();

        // Field 1: RecordType must be "CDMH.01" or "CDMH"
        if (record.RecordType != "CDMH.01" && record.RecordType != "CDMH")
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH",
                FieldName = "RecordType",
                ErrorMessage = $"RecordType must be 'CDMH.01' or 'CDMH', found '{record.RecordType}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 2: MessageVersion must match CDM/x.x/x.x/x.x format
        if (!ValidationHelpers.IsValidMessageVersion(record.MessageVersion))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "MessageVersion",
                ErrorMessage = $"MessageVersion must be in format 'CDM/x.x/x.x/x.x', found '{record.MessageVersion}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 3: MessageId is mandatory
        if (string.IsNullOrWhiteSpace(record.MessageId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "MessageId",
                ErrorMessage = "MessageId is mandatory",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 4: MessageCreatedDateTime must be RFC 3339 format
        if (!ValidationHelpers.IsValidRfc3339DateTime(record.MessageCreatedDateTime))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "MessageCreatedDateTime",
                ErrorMessage = $"MessageCreatedDateTime must be in RFC 3339 format, found '{record.MessageCreatedDateTime}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 5: Profile must be from allowed value set
        if (!AllowedValueSets.IsValidValue(AllowedValueSets.ProfileIds, record.Profile))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "Profile",
                ErrorMessage = $"Profile must be one of: {AllowedValueSets.ErrorMessages.ProfileIdList}. Found '{record.Profile}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 6: ProfileVersion is mandatory
        if (string.IsNullOrWhiteSpace(record.ProfileVersion))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "ProfileVersion",
                ErrorMessage = "ProfileVersion is mandatory",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 9: UsageStartDate format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.UsageStartDate) &&
            !ValidationHelpers.IsValidIsoDate(record.UsageStartDate))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "UsageStartDate",
                ErrorMessage = $"UsageStartDate must be in ISO 8601 format (YYYY-MM-DD), found '{record.UsageStartDate}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 10: UsageEndDate format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.UsageEndDate) &&
            !ValidationHelpers.IsValidIsoDate(record.UsageEndDate))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "UsageEndDate",
                ErrorMessage = $"UsageEndDate must be in ISO 8601 format (YYYY-MM-DD), found '{record.UsageEndDate}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Validate UsageStartDate <= UsageEndDate
        if (!string.IsNullOrWhiteSpace(record.UsageStartDate) &&
            !string.IsNullOrWhiteSpace(record.UsageEndDate))
        {
            var startDate = ValidationHelpers.ParseIsoDate(record.UsageStartDate);
            var endDate = ValidationHelpers.ParseIsoDate(record.UsageEndDate);

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                errors.Add(new ValidationError
                {
                    LineNumber = record.LineNumber,
                    RecordType = "CDMH.01",
                    FieldName = "UsageStartDate/UsageEndDate",
                    ErrorMessage = "UsageStartDate must be less than or equal to UsageEndDate",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        // Field 11: SenderPartyId is mandatory and must be valid DPID
        if (string.IsNullOrWhiteSpace(record.SenderPartyId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "SenderPartyId",
                ErrorMessage = "SenderPartyId is mandatory",
                Severity = ValidationSeverity.Error
            });
        }
        else if (!ValidationHelpers.IsValidDpid(record.SenderPartyId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "SenderPartyId",
                ErrorMessage = $"SenderPartyId must be a valid DPID format (PADPIDA + 14 chars), found '{record.SenderPartyId}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 12: SenderName is mandatory
        if (string.IsNullOrWhiteSpace(record.SenderName))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "SenderName",
                ErrorMessage = "SenderName is mandatory",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 13: ServiceDescription should not contain spaces or underscores (if present)
        if (!string.IsNullOrWhiteSpace(record.ServiceDescription) &&
            !ValidationHelpers.HasNoSpacesOrUnderscores(record.ServiceDescription))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "ServiceDescription",
                ErrorMessage = "ServiceDescription should not contain space characters or underscores (warning per spec)",
                Severity = ValidationSeverity.Warning
            });
        }

        // Field 14: RecipientPartyId format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.RecipientPartyId) &&
            !ValidationHelpers.IsValidDpid(record.RecipientPartyId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "RecipientPartyId",
                ErrorMessage = $"RecipientPartyId must be a valid DPID format, found '{record.RecipientPartyId}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 17: SentOnBehalfOfPartyId format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.SentOnBehalfOfPartyId) &&
            !ValidationHelpers.IsValidDpid(record.SentOnBehalfOfPartyId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "SentOnBehalfOfPartyId",
                ErrorMessage = $"SentOnBehalfOfPartyId must be a valid DPID format, found '{record.SentOnBehalfOfPartyId}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Conditional: If SentOnBehalfOfPartyId is present, SentOnBehalfOfName must be present
        if (!string.IsNullOrWhiteSpace(record.SentOnBehalfOfPartyId) &&
            string.IsNullOrWhiteSpace(record.SentOnBehalfOfName))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CDMH.01",
                FieldName = "SentOnBehalfOfName",
                ErrorMessage = "SentOnBehalfOfName is mandatory when SentOnBehalfOfPartyId is present",
                Severity = ValidationSeverity.Error
            });
        }

        // Validate backslash escaping in all string fields
        ValidateBackslashEscaping(record, errors);

        return errors;
    }

    private void ValidateBackslashEscaping(CdmhRecord record, List<ValidationError> errors)
    {
        // Check all string fields for proper backslash escaping
        var fieldsToCheck = new Dictionary<string, string?>
        {
            { "MessageVersion", record.MessageVersion },
            { "MessageId", record.MessageId },
            { "Profile", record.Profile },
            { "ProfileVersion", record.ProfileVersion },
            { "RelatedCdmMessageId", record.RelatedCdmMessageId },
            { "SalesReportId", record.SalesReportId },
            { "SenderPartyId", record.SenderPartyId },
            { "SenderName", record.SenderName },
            { "ServiceDescription", record.ServiceDescription },
            { "RecipientPartyId", record.RecipientPartyId },
            { "RecipientName", record.RecipientName },
            { "SentOnBehalfOfPartyId", record.SentOnBehalfOfPartyId },
            { "SentOnBehalfOfName", record.SentOnBehalfOfName }
        };

        foreach (var field in fieldsToCheck)
        {
            if (!string.IsNullOrEmpty(field.Value) && !ValidationHelpers.AreBackslashesProperlyEscaped(field.Value))
            {
                errors.Add(new ValidationError
                {
                    LineNumber = record.LineNumber,
                    RecordType = "CDMH.01",
                    FieldName = field.Key,
                    ErrorMessage = $"Backslashes must be escaped as groups of 4 consecutive backslashes. Found improperly escaped backslash in '{field.Key}'",
                    Severity = ValidationSeverity.Error
                });
            }
        }
    }
}
