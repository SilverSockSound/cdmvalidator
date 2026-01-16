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
    private readonly BusinessRuleValidator _businessRuleValidator;

    public CdmValidationEngine()
    {
        _parser = new CdmFileParser();
        _cdmhValidator = new CdmhValidator();
        _srfoValidator = new SrfoValidator();
        _cs01Validator = new Cs01Validator();
        _cd01Validator = new Cd01Validator();
        _fileStructureValidator = new FileStructureValidator();
        _businessRuleValidator = new BusinessRuleValidator();
    }

    /// <summary>
    /// Validates a CDM file and returns the validation result.
    /// </summary>
    public ValidationResult ValidateFile(string filePath, IProgress<ValidationProgress>? progress = null)
    {
        var result = new ValidationResult();

        // Reset parser state
        _parser.Reset();

        // Parse the file
        _parser.ParseFile(filePath, progress);

        // Add any parse errors
        result.Errors.AddRange(_parser.ParseErrors);

        // Validate file structure
        progress?.Report(new ValidationProgress { Phase = "File Structure", Current = 0, Total = 1 });
        var structureErrors = _fileStructureValidator.Validate(_parser);
        result.Errors.AddRange(structureErrors);
        progress?.Report(new ValidationProgress { Phase = "File Structure", Current = 1, Total = 1 });

        // Validate header record
        if (_parser.HeaderRecord != null)
        {
            progress?.Report(new ValidationProgress { Phase = "Header", Current = 0, Total = 1 });
            var headerErrors = _cdmhValidator.Validate(_parser.HeaderRecord);
            result.Errors.AddRange(headerErrors);
            progress?.Report(new ValidationProgress { Phase = "Header", Current = 1, Total = 1 });
        }

        // Validate footer record
        if (_parser.FooterRecord != null)
        {
            progress?.Report(new ValidationProgress { Phase = "Footer", Current = 0, Total = 1 });
            var footerErrors = _srfoValidator.Validate(_parser.FooterRecord);
            result.Errors.AddRange(footerErrors);
            progress?.Report(new ValidationProgress { Phase = "Footer", Current = 1, Total = 1 });
        }

        // Validate summary records
        int summaryCount = _parser.SummaryRecords.Count;
        int summaryIndex = 0;
        foreach (var summary in _parser.SummaryRecords)
        {
            summaryIndex++;
            if (summaryIndex % 100 == 0 || summaryIndex == summaryCount)
            {
                progress?.Report(new ValidationProgress
                {
                    Phase = "Summary Records",
                    Current = summaryIndex,
                    Total = summaryCount
                });
            }

            var summaryErrors = _cs01Validator.Validate(summary);
            result.Errors.AddRange(summaryErrors);
        }

        // Validate detail records
        int detailCount = _parser.DetailRecords.Count;
        int detailIndex = 0;
        foreach (var detail in _parser.DetailRecords)
        {
            detailIndex++;
            if (detailIndex % 100 == 0 || detailIndex == detailCount)
            {
                progress?.Report(new ValidationProgress
                {
                    Phase = "Detail Records",
                    Current = detailIndex,
                    Total = detailCount
                });
            }

            var detailErrors = _cd01Validator.Validate(detail);
            result.Errors.AddRange(detailErrors);
        }

        // Validate business rules (cross-record validation)
        var businessErrors = _businessRuleValidator.Validate(_parser, progress);
        result.Errors.AddRange(businessErrors);

        // Populate statistics
        result.Statistics = new ValidationStatistics
        {
            TotalLines = _parser.TotalLines,
            TotalRecords = 1 + _parser.SummaryRecords.Count + _parser.DetailRecords.Count + 1, // header + summaries + details + footer
            HeaderRecords = _parser.HeaderRecord != null ? 1 : 0,
            FooterRecords = _parser.FooterRecord != null ? 1 : 0,
            SummaryRecords = _parser.SummaryRecords.Count,
            DetailRecords = _parser.DetailRecords.Count,
            IgnoredRecords = _parser.IgnoredRecords.Count
        };

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
