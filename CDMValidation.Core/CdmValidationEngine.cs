using CDMValidation.Core.Models;
using CDMValidation.Core.Parsers;
using CDMValidation.Core.Validators;

namespace CDMValidation.Core;

/// <summary>
/// Main engine for validating CDM files.
/// </summary>
public class CdmValidationEngine
{
    private readonly CdmFileParser _parser;
    private readonly CdmhValidator _cdmhValidator;
    private readonly SrfoValidator _srfoValidator;
    private readonly Cs01Validator _cs01Validator;
    private readonly Cd01Validator _cd01Validator;
    private readonly FileStructureValidator _fileStructureValidator;

    public CdmValidationEngine()
    {
        _parser = new CdmFileParser();
        _cdmhValidator = new CdmhValidator();
        _srfoValidator = new SrfoValidator();
        _cs01Validator = new Cs01Validator();
        _cd01Validator = new Cd01Validator();
        _fileStructureValidator = new FileStructureValidator();
    }

    /// <summary>
    /// Validates a CDM file and returns the validation result using streaming two-pass approach.
    /// PASS 1: Parse, validate individual records, build lightweight index.
    /// PASS 2: Validate cross-record business rules using index.
    /// POST-PASS: Validate summary totals and file structure.
    /// </summary>
    public ValidationResult ValidateFile(string filePath, IProgress<ValidationProgress>? progress = null)
    {
        var result = new ValidationResult();

        // Reset parser state
        _parser.Reset();

        // PASS 1: Build index & validate individual records (streaming)
        var pass1Errors = _parser.ParseAndIndexFile(
            filePath, progress,
            _cdmhValidator, _srfoValidator, _cs01Validator, _cd01Validator);
        result.Errors.AddRange(pass1Errors);
        result.Errors.AddRange(_parser.ParseErrors);

        // PASS 2: Validate cross-record rules (streaming)
        var pass2Errors = _parser.ValidateCrossRecordRules(filePath, progress);
        result.Errors.AddRange(pass2Errors);

        // POST-PASS: Validate summary totals
        var summaryErrors = _parser.ValidateSummaryTotals();
        result.Errors.AddRange(summaryErrors);

        // Validate file structure using the lightweight index
        progress?.Report(new ValidationProgress { Phase = "Validating file structure", Current = 0, Total = 1 });
        var structureErrors = _fileStructureValidator.ValidateStreaming(_parser.ValidationIndex);
        result.Errors.AddRange(structureErrors);
        progress?.Report(new ValidationProgress { Phase = "Validating file structure", Current = 1, Total = 1 });

        // Populate statistics from index
        result.Statistics = new ValidationStatistics
        {
            TotalLines = _parser.ValidationIndex.TotalLines,
            TotalRecords = 1 + _parser.ValidationIndex.SummaryRecordCount + _parser.ValidationIndex.DetailRecordCount + 1,
            HeaderRecords = _parser.ValidationIndex.HeaderRecord != null ? 1 : 0,
            FooterRecords = _parser.ValidationIndex.FooterRecord != null ? 1 : 0,
            SummaryRecords = _parser.ValidationIndex.SummaryRecordCount,
            DetailRecords = _parser.ValidationIndex.DetailRecordCount,
            IgnoredRecords = _parser.ValidationIndex.IgnoredRecordCount
        };

        // Populate summary totals and file total
        decimal fileTotalClaimedAmount = 0;
        foreach (var summaryIndex in _parser.ValidationIndex.Summaries.Values)
        {
            result.Statistics.SummaryTotals[summaryIndex.SummaryRecordId] = new SummaryTotal
            {
                SummaryRecordId = summaryIndex.SummaryRecordId,
                ServiceDescription = summaryIndex.ServiceDescription,
                TotalClaimedAmount = summaryIndex.TotalClaimedAmount,
                DetailRecordCount = summaryIndex.DetailRecordCount
            };
            fileTotalClaimedAmount += summaryIndex.TotalClaimedAmount;
        }
        result.Statistics.TotalClaimedAmount = fileTotalClaimedAmount;

        // Set overall validation status
        result.IsValid = !result.Errors.Any(e => e.Severity == ValidationSeverity.Error);

        progress?.Report(new ValidationProgress { Phase = "Complete", Current = 1, Total = 1 });

        return result;
    }

    /// <summary>
    /// Gets the parsed CDM file data (for inspection/debugging).
    /// </summary>
    public CdmFileParser GetParser() => _parser;
}
