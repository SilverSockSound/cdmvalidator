using CDMValidation.Core.Constants;
using CDMValidation.Core.Models;

namespace CDMValidation.Core.Validators;

/// <summary>
/// Validates CS01.01 Summary Records.
/// </summary>
public class Cs01Validator : IRecordValidator<Cs01Record>
{
    public List<ValidationError> Validate(Cs01Record record)
    {
        var errors = new List<ValidationError>();

        // Field 1: RecordType must be "CS01.01" or "CS01"
        if (record.RecordType != "CS01.01" && record.RecordType != "CS01")
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01",
                FieldName = "RecordType",
                ErrorMessage = $"RecordType must be 'CS01.01' or 'CS01', found '{record.RecordType}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 2: SummaryRecordId is mandatory
        if (string.IsNullOrWhiteSpace(record.SummaryRecordId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "SummaryRecordId",
                ErrorMessage = "SummaryRecordId is mandatory",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 5: RightsControllerPartyId format validation (if present) - can be DPID or PartyID format
        if (!string.IsNullOrWhiteSpace(record.RightsControllerPartyId) &&
            !ValidationHelpers.IsValidPartyId(record.RightsControllerPartyId) &&
            !ValidationHelpers.IsValidDpid(record.RightsControllerPartyId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "RightsControllerPartyId",
                ErrorMessage = $"RightsControllerPartyId must be in format NAMESPACE::VALUE or DPID format, found '{record.RightsControllerPartyId}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Conditional: If RightsControllerName is present, RightsControllerPartyId should be present
        if (!string.IsNullOrWhiteSpace(record.RightsControllerName) &&
            string.IsNullOrWhiteSpace(record.RightsControllerPartyId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "RightsControllerPartyId",
                ErrorMessage = "RightsControllerPartyId is mandatory when RightsControllerName is present",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 7: DistributionChannelDPID format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.DistributionChannelDPID) &&
            !ValidationHelpers.IsValidDpid(record.DistributionChannelDPID))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "DistributionChannelDPID",
                ErrorMessage = $"DistributionChannelDPID must be a valid DPID format, found '{record.DistributionChannelDPID}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 8: StartOfClaimPeriod format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.StartOfClaimPeriod) &&
            !ValidationHelpers.IsValidIsoDate(record.StartOfClaimPeriod))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "StartOfClaimPeriod",
                ErrorMessage = $"StartOfClaimPeriod must be in ISO 8601 format, found '{record.StartOfClaimPeriod}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 9: EndOfClaimPeriod format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.EndOfClaimPeriod) &&
            !ValidationHelpers.IsValidIsoDate(record.EndOfClaimPeriod))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "EndOfClaimPeriod",
                ErrorMessage = $"EndOfClaimPeriod must be in ISO 8601 format, found '{record.EndOfClaimPeriod}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: StartOfClaimPeriod <= EndOfClaimPeriod
        if (!string.IsNullOrWhiteSpace(record.StartOfClaimPeriod) &&
            !string.IsNullOrWhiteSpace(record.EndOfClaimPeriod))
        {
            var startDate = ValidationHelpers.ParseIsoDate(record.StartOfClaimPeriod);
            var endDate = ValidationHelpers.ParseIsoDate(record.EndOfClaimPeriod);

            if (startDate.HasValue && endDate.HasValue && startDate > endDate)
            {
                errors.Add(new ValidationError
                {
                    LineNumber = record.LineNumber,
                    RecordType = "CS01.01",
                    FieldName = "StartOfClaimPeriod/EndOfClaimPeriod",
                    ErrorMessage = "StartOfClaimPeriod must be less than or equal to EndOfClaimPeriod",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        // Field 10: CommercialModel must be from allowed value set
        if (!AllowedValueSets.IsValidValue(AllowedValueSets.CommercialModelTypes, record.CommercialModel))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "CommercialModel",
                ErrorMessage = $"CommercialModel must be one of: {string.Join(", ", AllowedValueSets.CommercialModelTypes)}. Found '{record.CommercialModel}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 11: UseType must be from allowed value set
        if (!AllowedValueSets.IsValidValue(AllowedValueSets.UseTypes, record.UseType))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "UseType",
                ErrorMessage = $"UseType must be one of: {string.Join(", ", AllowedValueSets.UseTypes)}. Found '{record.UseType}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 12: Territory must be from allowed value set
        if (!AllowedValueSets.IsValidValue(AllowedValueSets.TerritoryCodes, record.Territory))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "Territory",
                ErrorMessage = $"Territory must be a valid ISO 3166-1 alpha-2 code. Found '{record.Territory}'",
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
                RecordType = "CS01.01",
                FieldName = "ServiceDescription",
                ErrorMessage = "ServiceDescription should not contain space characters or underscores (warning per spec)",
                Severity = ValidationSeverity.Warning
            });
        }

        // Field 14: CurrencyOfReporting must be valid currency code (if present)
        if (!string.IsNullOrWhiteSpace(record.CurrencyOfReporting) &&
            !AllowedValueSets.IsValidValue(AllowedValueSets.CurrencyCodes, record.CurrencyOfReporting))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "CurrencyOfReporting",
                ErrorMessage = $"CurrencyOfReporting must be a valid ISO 4217 currency code. Found '{record.CurrencyOfReporting}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 15: CurrencyOfInvoicing must be valid currency code (if present)
        if (!string.IsNullOrWhiteSpace(record.CurrencyOfInvoicing) &&
            !AllowedValueSets.IsValidValue(AllowedValueSets.CurrencyCodes, record.CurrencyOfInvoicing))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "CurrencyOfInvoicing",
                ErrorMessage = $"CurrencyOfInvoicing must be a valid ISO 4217 currency code. Found '{record.CurrencyOfInvoicing}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: If CurrencyOfReporting != CurrencyOfInvoicing, ExchangeRate is required
        if (!string.IsNullOrWhiteSpace(record.CurrencyOfReporting) &&
            !string.IsNullOrWhiteSpace(record.CurrencyOfInvoicing) &&
            !string.Equals(record.CurrencyOfReporting, record.CurrencyOfInvoicing, StringComparison.OrdinalIgnoreCase) &&
            !record.ExchangeRate.HasValue)
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "ExchangeRate",
                ErrorMessage = "ExchangeRate is mandatory when CurrencyOfReporting differs from CurrencyOfInvoicing",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: If ExchangeRate is present, ExchangeRateSource is required
        if (record.ExchangeRate.HasValue && string.IsNullOrWhiteSpace(record.ExchangeRateSource))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "ExchangeRateSource",
                ErrorMessage = "ExchangeRateSource is mandatory when ExchangeRate is present",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: If ExchangeRate is present, StartDateOfCurrencyExchange is required
        if (record.ExchangeRate.HasValue && string.IsNullOrWhiteSpace(record.StartDateOfCurrencyExchange))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "StartDateOfCurrencyExchange",
                ErrorMessage = "StartDateOfCurrencyExchange is mandatory when ExchangeRate is present",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 18: StartDateOfCurrencyExchange format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.StartDateOfCurrencyExchange) &&
            !ValidationHelpers.IsValidRfc3339DateTime(record.StartDateOfCurrencyExchange) &&
            !ValidationHelpers.IsValidIsoDate(record.StartDateOfCurrencyExchange))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "StartDateOfCurrencyExchange",
                ErrorMessage = $"StartDateOfCurrencyExchange must be in ISO 8601 datetime or date format, found '{record.StartDateOfCurrencyExchange}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: If StartDateOfCurrencyExchange is present, EndDateOfCurrencyExchange is required
        if (!string.IsNullOrWhiteSpace(record.StartDateOfCurrencyExchange) &&
            string.IsNullOrWhiteSpace(record.EndDateOfCurrencyExchange))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "EndDateOfCurrencyExchange",
                ErrorMessage = "EndDateOfCurrencyExchange is mandatory when StartDateOfCurrencyExchange is present",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 19: EndDateOfCurrencyExchange format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.EndDateOfCurrencyExchange) &&
            !ValidationHelpers.IsValidRfc3339DateTime(record.EndDateOfCurrencyExchange) &&
            !ValidationHelpers.IsValidIsoDate(record.EndDateOfCurrencyExchange))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "EndDateOfCurrencyExchange",
                ErrorMessage = $"EndDateOfCurrencyExchange must be in ISO 8601 datetime or date format, found '{record.EndDateOfCurrencyExchange}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: RightsTypeSplitMechanical must be between 0 and 100
        if (!ValidationHelpers.IsInRange(record.RightsTypeSplitMechanical, 0, 100))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "RightsTypeSplitMechanical",
                ErrorMessage = $"RightsTypeSplitMechanical must be between 0 and 100, found {record.RightsTypeSplitMechanical}",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: RightsTypeSplitPerforming must be between 0 and 100
        if (!ValidationHelpers.IsInRange(record.RightsTypeSplitPerforming, 0, 100))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "RightsTypeSplitPerforming",
                ErrorMessage = $"RightsTypeSplitPerforming must be between 0 and 100, found {record.RightsTypeSplitPerforming}",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: RightsTypeSplitMechanical + RightsTypeSplitPerforming must equal 100
        var splitSum = record.RightsTypeSplitMechanical + record.RightsTypeSplitPerforming;
        if (!ValidationHelpers.AreApproximatelyEqual(splitSum, 100m))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "RightsTypeSplitMechanical/RightsTypeSplitPerforming",
                ErrorMessage = $"RightsTypeSplitMechanical + RightsTypeSplitPerforming must equal 100, found {splitSum}",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: TotalMarketShare must be between 0 and 100 (if present)
        if (record.TotalMarketShare.HasValue &&
            !ValidationHelpers.IsInRange(record.TotalMarketShare.Value, 0, 100))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CS01.01",
                FieldName = "TotalMarketShare",
                ErrorMessage = $"TotalMarketShare must be between 0 and 100, found {record.TotalMarketShare}",
                Severity = ValidationSeverity.Error
            });
        }

        // Validate backslash escaping in all string fields
        ValidateBackslashEscaping(record, errors);

        return errors;
    }

    private void ValidateBackslashEscaping(Cs01Record record, List<ValidationError> errors)
    {
        // Check all string fields for proper backslash escaping
        var fieldsToCheck = new Dictionary<string, string?>
        {
            { "SummaryRecordId", record.SummaryRecordId },
            { "InvoiceReference", record.InvoiceReference },
            { "RightsControllerName", record.RightsControllerName },
            { "RightsControllerPartyId", record.RightsControllerPartyId },
            { "DistributionChannel", record.DistributionChannel },
            { "DistributionChannelDPID", record.DistributionChannelDPID },
            { "CommercialModel", record.CommercialModel },
            { "UseType", record.UseType },
            { "Territory", record.Territory },
            { "ServiceDescription", record.ServiceDescription },
            { "CurrencyOfReporting", record.CurrencyOfReporting },
            { "CurrencyOfInvoicing", record.CurrencyOfInvoicing },
            { "ExchangeRateSource", record.ExchangeRateSource }
        };

        foreach (var field in fieldsToCheck)
        {
            if (!string.IsNullOrEmpty(field.Value) && !ValidationHelpers.AreBackslashesProperlyEscaped(field.Value))
            {
                errors.Add(new ValidationError
                {
                    LineNumber = record.LineNumber,
                    RecordType = "CS01.01",
                    FieldName = field.Key,
                    ErrorMessage = $"Backslashes must be escaped as groups of 4 consecutive backslashes. Found improperly escaped backslash in '{field.Key}'",
                    Severity = ValidationSeverity.Error
                });
            }
        }
    }
}
