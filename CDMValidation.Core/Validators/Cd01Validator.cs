using CDMValidation.Core.Constants;
using CDMValidation.Core.Models;

namespace CDMValidation.Core.Validators;

/// <summary>
/// Validates CD01.01 Detail Records.
/// </summary>
public class Cd01Validator : IRecordValidator<Cd01Record>
{
    public List<ValidationError> Validate(Cd01Record record)
    {
        var errors = new List<ValidationError>();

        // Field 1: RecordType must be "CD01.01" or "CD01"
        if (record.RecordType != "CD01.01" && record.RecordType != "CD01")
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01",
                FieldName = "RecordType",
                ErrorMessage = $"RecordType must be 'CD01.01' or 'CD01', found '{record.RecordType}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 2: ClaimId is mandatory
        if (string.IsNullOrWhiteSpace(record.ClaimId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "ClaimId",
                ErrorMessage = "ClaimId is mandatory",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 3: SummaryRecordId is mandatory
        if (string.IsNullOrWhiteSpace(record.SummaryRecordId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "SummaryRecordId",
                ErrorMessage = "SummaryRecordId is mandatory",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 4: DspResourceId is mandatory
        if (string.IsNullOrWhiteSpace(record.DspResourceId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "DspResourceId",
                ErrorMessage = "DspResourceId is mandatory",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 5: ResourceISRC format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.ResourceISRC) &&
            !ValidationHelpers.IsValidIsrc(record.ResourceISRC))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "ResourceISRC",
                ErrorMessage = $"ResourceISRC must be in valid ISRC format, found '{record.ResourceISRC}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 8: ResourceDisplayArtistPartyId format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.ResourceDisplayArtistPartyId) &&
            !ValidationHelpers.IsValidPartyId(record.ResourceDisplayArtistPartyId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "ResourceDisplayArtistPartyId",
                ErrorMessage = $"ResourceDisplayArtistPartyId must be in format NAMESPACE::VALUE, found '{record.ResourceDisplayArtistPartyId}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 10: MusicalWorkISWC format validation (if present)
        if (!string.IsNullOrWhiteSpace(record.MusicalWorkISWC) &&
            !ValidationHelpers.IsValidIswc(record.MusicalWorkISWC))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "MusicalWorkISWC",
                ErrorMessage = $"MusicalWorkISWC must be in valid ISWC format (T + 10 digits), found '{record.MusicalWorkISWC}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Field 11: MusicalWorkTitle is mandatory
        if (string.IsNullOrWhiteSpace(record.MusicalWorkTitle))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "MusicalWorkTitle",
                ErrorMessage = "MusicalWorkTitle is mandatory",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: Composer/Author names and IDs should match count
        if (record.MusicalWorkComposerAuthorNames.Count != record.MusicalWorkComposerAuthorPartyIds.Count &&
            record.MusicalWorkComposerAuthorPartyIds.Count > 0)
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "MusicalWorkComposerAuthorName/PartyId",
                ErrorMessage = $"Number of composer/author names ({record.MusicalWorkComposerAuthorNames.Count}) should match number of party IDs ({record.MusicalWorkComposerAuthorPartyIds.Count})",
                Severity = ValidationSeverity.Warning
            });
        }

        // Validate PartyID format for each composer/author
        foreach (var partyId in record.MusicalWorkComposerAuthorPartyIds)
        {
            if (!ValidationHelpers.IsValidPartyId(partyId))
            {
                errors.Add(new ValidationError
                {
                    LineNumber = record.LineNumber,
                    RecordType = "CD01.01",
                    FieldName = "MusicalWorkComposerAuthorPartyId",
                    ErrorMessage = $"MusicalWorkComposerAuthorPartyId must be in format NAMESPACE::VALUE, found '{partyId}'",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        // Field 15: ClaimBasis must be from allowed value set
        if (!AllowedValueSets.IsValidValue(AllowedValueSets.ClaimBasisValues, record.ClaimBasis))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "ClaimBasis",
                ErrorMessage = $"ClaimBasis must be one of: {AllowedValueSets.ErrorMessages.ClaimBasisValuesList}. Found '{record.ClaimBasis}'",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: If ClaimBasis is "Unmatched", LicensorWorkId must be empty
        if (string.Equals(record.ClaimBasis, "Unmatched", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(record.LicensorWorkId))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "LicensorWorkId",
                ErrorMessage = "LicensorWorkId must be empty when ClaimBasis is 'Unmatched'",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: ShareClaimedMechanical must be between 0 and 100
        if (!ValidationHelpers.IsInRange(record.ShareClaimedMechanical, 0, 100))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "ShareClaimedMechanical",
                ErrorMessage = $"ShareClaimedMechanical must be between 0 and 100, found {record.ShareClaimedMechanical}",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: ShareClaimedPerforming must be between 0 and 100
        if (!ValidationHelpers.IsInRange(record.ShareClaimedPerforming, 0, 100))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "ShareClaimedPerforming",
                ErrorMessage = $"ShareClaimedPerforming must be between 0 and 100, found {record.ShareClaimedPerforming}",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: BlendedShareClaimedForMechAndPerf must be between 0 and 100
        if (!ValidationHelpers.IsInRange(record.BlendedShareClaimedForMechAndPerf, 0, 100))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "BlendedShareClaimedForMechAndPerf",
                ErrorMessage = $"BlendedShareClaimedForMechAndPerf must be between 0 and 100, found {record.BlendedShareClaimedForMechAndPerf}",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: PercentageOfResourceInRelease must be between 0 and 100 (if present)
        if (record.PercentageOfResourceInRelease.HasValue &&
            !ValidationHelpers.IsInRange(record.PercentageOfResourceInRelease.Value, 0, 100))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "PercentageOfResourceInRelease",
                ErrorMessage = $"PercentageOfResourceInRelease must be between 0 and 100, found {record.PercentageOfResourceInRelease}",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: ClaimedAmount should equal ClaimedAmountMechanical + ClaimedAmountPerforming
        var calculatedTotal = record.ClaimedAmountMechanical + record.ClaimedAmountPerforming;
        if (!ValidationHelpers.AreApproximatelyEqual(record.ClaimedAmount, calculatedTotal))
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "ClaimedAmount",
                ErrorMessage = $"ClaimedAmount ({record.ClaimedAmount}) should equal ClaimedAmountMechanical ({record.ClaimedAmountMechanical}) + ClaimedAmountPerforming ({record.ClaimedAmountPerforming}) = {calculatedTotal}",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: TariffParameterTypes and TariffParameterValues should match count
        if (record.TariffParameterTypes.Count != record.TariffParameterValues.Count &&
            record.TariffParameterValues.Count > 0)
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "TariffParameterType/Value",
                ErrorMessage = $"Number of TariffParameterTypes ({record.TariffParameterTypes.Count}) should match number of TariffParameterValues ({record.TariffParameterValues.Count})",
                Severity = ValidationSeverity.Error
            });
        }

        // Business Rule: If TariffParameterTypes present, TariffParameterValues required
        if (record.TariffParameterTypes.Count > 0 && record.TariffParameterValues.Count == 0)
        {
            errors.Add(new ValidationError
            {
                LineNumber = record.LineNumber,
                RecordType = "CD01.01",
                FieldName = "TariffParameterValue",
                ErrorMessage = "TariffParameterValue is mandatory when TariffParameterType is present",
                Severity = ValidationSeverity.Error
            });
        }

        // Validate backslash escaping in all string fields
        ValidateBackslashEscaping(record, errors);

        return errors;
    }

    private void ValidateBackslashEscaping(Cd01Record record, List<ValidationError> errors)
    {
        // Check all string fields for proper backslash escaping
        var fieldsToCheck = new Dictionary<string, string?>
        {
            { "ClaimId", record.ClaimId },
            { "SummaryRecordId", record.SummaryRecordId },
            { "DspResourceId", record.DspResourceId },
            { "ResourceISRC", record.ResourceISRC },
            { "ResourceTitle", record.ResourceTitle },
            { "ResourceDisplayArtistName", record.ResourceDisplayArtistName },
            { "ResourceDisplayArtistPartyId", record.ResourceDisplayArtistPartyId },
            { "LicensorWorkId", record.LicensorWorkId },
            { "MusicalWorkISWC", record.MusicalWorkISWC },
            { "MusicalWorkTitle", record.MusicalWorkTitle },
            { "ClaimBasis", record.ClaimBasis },
            { "SalesTransactionId", record.SalesTransactionId }
        };

        foreach (var field in fieldsToCheck)
        {
            if (!string.IsNullOrEmpty(field.Value) && !ValidationHelpers.AreBackslashesProperlyEscaped(field.Value))
            {
                errors.Add(new ValidationError
                {
                    LineNumber = record.LineNumber,
                    RecordType = "CD01.01",
                    FieldName = field.Key,
                    ErrorMessage = $"Backslashes must be escaped as groups of 4 consecutive backslashes. Found improperly escaped backslash in '{field.Key}'",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        // Check list fields
        foreach (var title in record.AlternativeMusicalWorkTitles)
        {
            if (!ValidationHelpers.AreBackslashesProperlyEscaped(title))
            {
                errors.Add(new ValidationError
                {
                    LineNumber = record.LineNumber,
                    RecordType = "CD01.01",
                    FieldName = "AlternativeMusicalWorkTitles",
                    ErrorMessage = $"Backslashes must be escaped as groups of 4 consecutive backslashes. Found improperly escaped backslash in alternative title: '{title}'",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        foreach (var name in record.MusicalWorkComposerAuthorNames)
        {
            if (!ValidationHelpers.AreBackslashesProperlyEscaped(name))
            {
                errors.Add(new ValidationError
                {
                    LineNumber = record.LineNumber,
                    RecordType = "CD01.01",
                    FieldName = "MusicalWorkComposerAuthorNames",
                    ErrorMessage = $"Backslashes must be escaped as groups of 4 consecutive backslashes. Found improperly escaped backslash in composer/author name: '{name}'",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        foreach (var partyId in record.MusicalWorkComposerAuthorPartyIds)
        {
            if (!ValidationHelpers.AreBackslashesProperlyEscaped(partyId))
            {
                errors.Add(new ValidationError
                {
                    LineNumber = record.LineNumber,
                    RecordType = "CD01.01",
                    FieldName = "MusicalWorkComposerAuthorPartyIds",
                    ErrorMessage = $"Backslashes must be escaped as groups of 4 consecutive backslashes. Found improperly escaped backslash in party ID: '{partyId}'",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        foreach (var type in record.TariffParameterTypes)
        {
            if (!ValidationHelpers.AreBackslashesProperlyEscaped(type))
            {
                errors.Add(new ValidationError
                {
                    LineNumber = record.LineNumber,
                    RecordType = "CD01.01",
                    FieldName = "TariffParameterTypes",
                    ErrorMessage = $"Backslashes must be escaped as groups of 4 consecutive backslashes. Found improperly escaped backslash in tariff parameter type: '{type}'",
                    Severity = ValidationSeverity.Error
                });
            }
        }
    }
}
