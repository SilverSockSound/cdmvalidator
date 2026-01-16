using CDMValidation.Core.Models;
using CDMValidation.Core.Parsers;

namespace CDMValidation.Core.Validators;

/// <summary>
/// Validates business rules that span multiple records.
/// NOTE: This class is obsolete and kept only for backward compatibility.
/// New code should use the streaming validation methods in CdmFileParser instead:
/// - ParseAndIndexFile() for duplicate detection
/// - ValidateCrossRecordRules() for CD01→CS01 references and BlendedShare calculation
/// - ValidateSummaryTotals() for TotalClaimedAmount validation
/// </summary>
[Obsolete("This class is obsolete. Use streaming validation methods in CdmFileParser for memory efficiency.", false)]
public class BusinessRuleValidator
{
    public List<ValidationError> Validate(CdmFileParser parser, IProgress<ValidationProgress>? progress = null)
    {
        var errors = new List<ValidationError>();

        // Build lookup structures for O(1) access instead of O(N) linear searches
        var summaryLookup = parser.SummaryRecords
            .ToDictionary(s => s.SummaryRecordId, s => s, StringComparer.OrdinalIgnoreCase);

        var detailsBySummaryId = parser.DetailRecords
            .GroupBy(d => d.SummaryRecordId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // Calculate total validations: details validation + summary validation + blended share validation + 2 duplicate checks
        int totalValidations = (parser.DetailRecords.Count * 2) + parser.SummaryRecords.Count + 2;
        int currentValidation = 0;

        // Validate that each detail record references a valid summary record
        foreach (var detail in parser.DetailRecords)
        {
            if (!summaryLookup.ContainsKey(detail.SummaryRecordId))
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

            currentValidation++;
            if (progress != null && currentValidation % 100 == 0)
            {
                progress.Report(new ValidationProgress
                {
                    Phase = "Business Rules",
                    Current = currentValidation,
                    Total = totalValidations
                });
            }
        }

        // Validate TotalClaimedAmount for each summary record using pre-built lookup
        foreach (var summary in parser.SummaryRecords)
        {
            if (detailsBySummaryId.TryGetValue(summary.SummaryRecordId, out var relatedDetails))
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

        // Validate BlendedShareClaimedForMechAndPerf calculation using dictionary lookup
        foreach (var detail in parser.DetailRecords)
        {
            // Use dictionary lookup instead of FirstOrDefault (O(1) vs O(N))
            if (summaryLookup.TryGetValue(detail.SummaryRecordId, out var summary))
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

            currentValidation++;
            if (progress != null && currentValidation % 100 == 0)
            {
                progress.Report(new ValidationProgress
                {
                    Phase = "Business Rules",
                    Current = currentValidation,
                    Total = totalValidations
                });
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

        currentValidation++;
        progress?.Report(new ValidationProgress
        {
            Phase = "Business Rules",
            Current = currentValidation,
            Total = totalValidations
        });

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

        currentValidation++;
        progress?.Report(new ValidationProgress
        {
            Phase = "Business Rules",
            Current = currentValidation,
            Total = totalValidations
        });

        return errors;
    }
}
