namespace CDMValidation.Core.Models;

/// <summary>
/// Minimal CS01 data stored in index for cross-record validation.
/// Only stores the essential fields needed, reducing memory from ~400 bytes to ~150 bytes per record.
/// </summary>
public class SummaryIndex
{
    public string SummaryRecordId { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string? ServiceDescription { get; set; }
    public decimal TotalClaimedAmount { get; set; }
    public decimal RightsTypeSplitMechanical { get; set; }
    public decimal RightsTypeSplitPerforming { get; set; }

    /// <summary>
    /// Accumulated ClaimedAmount from all CD01 detail records referencing this summary.
    /// Used to validate TotalClaimedAmount during post-pass validation.
    /// </summary>
    public decimal AccumulatedDetailAmount { get; set; }

    /// <summary>
    /// Count of CD01 detail records referencing this summary.
    /// </summary>
    public int DetailRecordCount { get; set; }
}

/// <summary>
/// Lightweight index structure for streaming validation.
/// Holds minimal data needed for cross-record validation without keeping full record objects in memory.
/// </summary>
public class ValidationIndex
{
    /// <summary>
    /// Minimal CS01 summary data indexed by SummaryRecordId (case-insensitive).
    /// </summary>
    public Dictionary<string, SummaryIndex> Summaries { get; set; } =
        new Dictionary<string, SummaryIndex>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// HashSet for tracking seen ClaimIds to detect duplicates.
    /// Uses case-insensitive comparison.
    /// </summary>
    public HashSet<string> SeenClaimIds { get; set; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// HashSet for tracking seen SummaryRecordIds to detect duplicates.
    /// Uses case-insensitive comparison.
    /// </summary>
    public HashSet<string> SeenSummaryRecordIds { get; set; } =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Header record (kept in memory, negligible size).
    /// </summary>
    public CdmhRecord? HeaderRecord { get; set; }

    /// <summary>
    /// Footer record (kept in memory, negligible size).
    /// </summary>
    public SrfoRecord? FooterRecord { get; set; }

    /// <summary>
    /// Total lines in the file.
    /// </summary>
    public int TotalLines { get; set; }

    /// <summary>
    /// Count of summary records parsed.
    /// </summary>
    public int SummaryRecordCount { get; set; }

    /// <summary>
    /// Count of detail records parsed.
    /// </summary>
    public int DetailRecordCount { get; set; }

    /// <summary>
    /// Count of ignored records (CDDM, etc.).
    /// </summary>
    public int IgnoredRecordCount { get; set; }
}
