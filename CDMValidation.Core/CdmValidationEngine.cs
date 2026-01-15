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
    public ValidationResult ValidateFile(string filePath)
    {
        var result = new ValidationResult();

        // Reset parser state
        _parser.Reset();

        // Parse the file
        _parser.ParseFile(filePath);

        // Add any parse errors
        result.Errors.AddRange(_parser.ParseErrors);

        // Validate file structure
        var structureErrors = _fileStructureValidator.Validate(_parser);
        result.Errors.AddRange(structureErrors);

        // Validate header record
        if (_parser.HeaderRecord != null)
        {
            var headerErrors = _cdmhValidator.Validate(_parser.HeaderRecord);
            result.Errors.AddRange(headerErrors);
        }

        // Validate footer record
        if (_parser.FooterRecord != null)
        {
            var footerErrors = _srfoValidator.Validate(_parser.FooterRecord);
            result.Errors.AddRange(footerErrors);
        }

        // Validate summary records
        foreach (var summary in _parser.SummaryRecords)
        {
            var summaryErrors = _cs01Validator.Validate(summary);
            result.Errors.AddRange(summaryErrors);
        }

        // Validate detail records
        foreach (var detail in _parser.DetailRecords)
        {
            var detailErrors = _cd01Validator.Validate(detail);
            result.Errors.AddRange(detailErrors);
        }

        // Validate business rules (cross-record validation)
        var businessErrors = _businessRuleValidator.Validate(_parser);
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

        return result;
    }

    /// <summary>
    /// Gets the parsed CDM file data (for inspection/debugging).
    /// </summary>
    public CdmFileParser GetParser() => _parser;
}
