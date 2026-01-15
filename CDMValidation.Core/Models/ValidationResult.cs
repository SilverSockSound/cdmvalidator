namespace CDMValidation.Core.Models;

/// <summary>
/// Represents the result of validating a CDM file.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
    public ValidationStatistics Statistics { get; set; } = new();
}

/// <summary>
/// Represents a validation error or issue found in the CDM file.
/// </summary>
public class ValidationError
{
    public int LineNumber { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public ValidationSeverity Severity { get; set; }
}

/// <summary>
/// Represents a validation warning (non-critical issue).
/// </summary>
public class ValidationWarning
{
    public int LineNumber { get; set; }
    public string RecordType { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents statistics about the validated CDM file.
/// </summary>
public class ValidationStatistics
{
    public int TotalLines { get; set; }
    public int TotalRecords { get; set; }
    public int HeaderRecords { get; set; }
    public int FooterRecords { get; set; }
    public int SummaryRecords { get; set; }
    public int DetailRecords { get; set; }
    public int IgnoredRecords { get; set; }
}

/// <summary>
/// Defines the severity level of a validation issue.
/// </summary>
public enum ValidationSeverity
{
    Error,
    Warning,
    Info
}
