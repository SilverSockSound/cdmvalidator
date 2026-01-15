using CDMValidation.Core.Models;
using CDMValidation.Core.Parsers;

namespace CDMValidation.Core.Validators;

/// <summary>
/// Validates business rules that span multiple records.
/// </summary>
public class BusinessRuleValidator
{
    public List<ValidationError> Validate(CdmFileParser parser)
    {
        var errors = new List<ValidationError>();

        // Build a set of valid SummaryRecordIds
        var validSummaryIds = new HashSet<string>(
            parser.SummaryRecords.Select(s => s.SummaryRecordId),
            StringComparer.OrdinalIgnoreCase
        );

        // Validate that each detail record references a valid summary record
        foreach (var detail in parser.DetailRecords)
        {
            if (!validSummaryIds.Contains(detail.SummaryRecordId))
            {
                errors.Add(new ValidationError
                {
                    LineNumber = detail.LineNumber,
                    RecordType = "CD01.01",
                    FieldName = "SummaryRecordId",
                    ErrorMessage = $"SummaryRecordId '{detail.SummaryRecordId}' does not reference any CS01.01 record",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        // Validate TotalClaimedAmount for each summary record
        foreach (var summary in parser.SummaryRecords)
        {
            var relatedDetails = parser.DetailRecords
                .Where(d => string.Equals(d.SummaryRecordId, summary.SummaryRecordId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (relatedDetails.Any())
            {
                decimal calculatedTotal = relatedDetails.Sum(d => d.ClaimedAmount);

                if (!ValidationHelpers.AreApproximatelyEqual(summary.TotalClaimedAmount, calculatedTotal, 0.02m))
                {
                    errors.Add(new ValidationError
                    {
                        LineNumber = summary.LineNumber,
                        RecordType = "CS01.01",
                        FieldName = "TotalClaimedAmount",
                        ErrorMessage = $"TotalClaimedAmount ({summary.TotalClaimedAmount}) does not match sum of detail ClaimedAmounts ({calculatedTotal:F2})",
                        Severity = ValidationSeverity.Error
                    });
                }
            }
        }

        // Validate BlendedShareClaimedForMechAndPerf calculation
        foreach (var detail in parser.DetailRecords)
        {
            // Find the corresponding summary record
            var summary = parser.SummaryRecords.FirstOrDefault(s =>
                string.Equals(s.SummaryRecordId, detail.SummaryRecordId, StringComparison.OrdinalIgnoreCase));

            if (summary != null)
            {
                // Calculate expected blended share
                // BlendedShare = (ShareMech * SplitMech/100) + (SharePerf * SplitPerf/100)
                decimal expectedBlendedShare =
                    (detail.ShareClaimedMechanical * summary.RightsTypeSplitMechanical / 100m) +
                    (detail.ShareClaimedPerforming * summary.RightsTypeSplitPerforming / 100m);

                if (!ValidationHelpers.AreApproximatelyEqual(detail.BlendedShareClaimedForMechAndPerf, expectedBlendedShare, 0.01m))
                {
                    errors.Add(new ValidationError
                    {
                        LineNumber = detail.LineNumber,
                        RecordType = "CD01.01",
                        FieldName = "BlendedShareClaimedForMechAndPerf",
                        ErrorMessage = $"BlendedShareClaimedForMechAndPerf ({detail.BlendedShareClaimedForMechAndPerf}) does not match calculated value ({expectedBlendedShare:F2}). " +
                                     $"Expected: ({detail.ShareClaimedMechanical} * {summary.RightsTypeSplitMechanical}/100) + ({detail.ShareClaimedPerforming} * {summary.RightsTypeSplitPerforming}/100)",
                        Severity = ValidationSeverity.Error
                    });
                }
            }
        }

        // Check for duplicate ClaimIds
        var claimIdGroups = parser.DetailRecords
            .GroupBy(d => d.ClaimId, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);

        foreach (var group in claimIdGroups)
        {
            var duplicateLines = group.Select(d => d.LineNumber);
            errors.Add(new ValidationError
            {
                LineNumber = group.First().LineNumber,
                RecordType = "CD01.01",
                FieldName = "ClaimId",
                ErrorMessage = $"Duplicate ClaimId '{group.Key}' found at lines: {string.Join(", ", duplicateLines)}",
                Severity = ValidationSeverity.Error
            });
        }

        // Check for duplicate SummaryRecordIds
        var summaryIdGroups = parser.SummaryRecords
            .GroupBy(s => s.SummaryRecordId, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1);

        foreach (var group in summaryIdGroups)
        {
            var duplicateLines = group.Select(s => s.LineNumber);
            errors.Add(new ValidationError
            {
                LineNumber = group.First().LineNumber,
                RecordType = "CS01.01",
                FieldName = "SummaryRecordId",
                ErrorMessage = $"Duplicate SummaryRecordId '{group.Key}' found at lines: {string.Join(", ", duplicateLines)}",
                Severity = ValidationSeverity.Error
            });
        }

        return errors;
    }
}
