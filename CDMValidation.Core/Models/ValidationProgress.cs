namespace CDMValidation.Core.Models;

/// <summary>
/// Represents progress during validation operations.
/// </summary>
public class ValidationProgress
{
    /// <summary>
    /// Current phase of validation (e.g., "Parsing", "Validating Records", "Business Rules").
    /// </summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>
    /// Current progress count.
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// Total items to process.
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Percentage complete (0-100).
    /// </summary>
    public double PercentComplete => Total > 0 ? (double)Current / Total * 100 : 0;

    /// <summary>
    /// Optional estimated time remaining.
    /// </summary>
    public TimeSpan? EstimatedTimeRemaining { get; set; }
}
