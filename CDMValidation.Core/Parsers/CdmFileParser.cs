using CDMValidation.Core.Models;
using CDMValidation.Core.Validators;

namespace CDMValidation.Core.Parsers;

/// <summary>
/// Parses tab-delimited CDM files into structured record objects.
/// </summary>
public class CdmFileParser
{
    public CdmhRecord? HeaderRecord { get; private set; }
    public SrfoRecord? FooterRecord { get; private set; }
    public List<Cs01Record> SummaryRecords { get; private set; } = new();
    public List<Cd01Record> DetailRecords { get; private set; } = new();
    public List<string> IgnoredRecords { get; private set; } = new();
    public List<ValidationError> ParseErrors { get; private set; } = new();
    public int TotalLines { get; private set; }

    /// <summary>
    /// Lightweight index for streaming validation (used instead of full record lists for memory efficiency).
    /// </summary>
    public ValidationIndex ValidationIndex { get; private set; } = new();

    /// <summary>
    /// Parses a CDM file from the given file path.
    /// </summary>
    public void ParseFile(string filePath, IProgress<ValidationProgress>? progress = null)
    {
        if (!File.Exists(filePath))
        {
            ParseErrors.Add(new ValidationError
            {
                LineNumber = 0,
                RecordType = "FILE",
                FieldName = "FilePath",
                ErrorMessage = $"File not found: {filePath}",
                Severity = ValidationSeverity.Error
            });
            return;
        }

        try
        {
            // Use streaming approach for better memory efficiency on large files
            ParseLinesStreaming(filePath, progress);
        }
        catch (Exception ex)
        {
            ParseErrors.Add(new ValidationError
            {
                LineNumber = 0,
                RecordType = "FILE",
                FieldName = "ParseError",
                ErrorMessage = $"Error reading file: {ex.Message}",
                Severity = ValidationSeverity.Error
            });
        }
    }

    /// <summary>
    /// Parses an array of lines into record objects.
    /// </summary>
    public void ParseLines(string[] lines)
    {
        TotalLines = lines.Length;

        for (int i = 0; i < lines.Length; i++)
        {
            int lineNumber = i + 1;
            var line = lines[i];

            // Skip empty lines and comment lines (starting with #)
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;

            try
            {
                var recordType = GetRecordType(line);

                switch (recordType)
                {
                    case "CDMH.01":
                    case "CDMH":
                        HeaderRecord = CdmhRecord.Parse(line, lineNumber);
                        break;

                    case "SRFO":
                        FooterRecord = SrfoRecord.Parse(line, lineNumber);
                        break;

                    case "CS01.01":
                    case "CS01":
                        SummaryRecords.Add(Cs01Record.Parse(line, lineNumber));
                        break;

                    case "CD01.01":
                    case "CD01":
                        DetailRecords.Add(Cd01Record.Parse(line, lineNumber));
                        break;

                    case "CDDM":
                        // Ignore CDDM messages as per requirements
                        IgnoredRecords.Add(recordType);
                        break;

                    case var rt when rt.StartsWith("CDD"):
                        // Ignore all CDDM-related records (CDD1, CDD2.01, CDD3.01)
                        IgnoredRecords.Add(recordType);
                        break;

                    default:
                        ParseErrors.Add(new ValidationError
                        {
                            LineNumber = lineNumber,
                            RecordType = recordType,
                            FieldName = "RecordType",
                            ErrorMessage = $"Unknown or unsupported record type: {recordType}",
                            Severity = ValidationSeverity.Warning
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                ParseErrors.Add(new ValidationError
                {
                    LineNumber = lineNumber,
                    RecordType = "UNKNOWN",
                    FieldName = "Parse",
                    ErrorMessage = $"Error parsing line: {ex.Message}",
                    Severity = ValidationSeverity.Error
                });
            }
        }
    }

    /// <summary>
    /// Parses lines from a file using streaming for memory efficiency.
    /// </summary>
    private void ParseLinesStreaming(string filePath, IProgress<ValidationProgress>? progress)
    {
        int lineNumber = 0;
        int estimatedTotalLines = 0;

        // Estimate total lines based on file size and average line length (avoids reading file twice)
        if (progress != null)
        {
            var fileInfo = new FileInfo(filePath);
            // Sample first 10000 lines to estimate average line length
            int sampleSize = 0;
            long sampleBytes = 0;
            foreach (var line in File.ReadLines(filePath).Take(10000))
            {
                sampleSize++;
                sampleBytes += System.Text.Encoding.UTF8.GetByteCount(line) + 1; // +1 for newline
            }

            if (sampleSize > 0)
            {
                double avgLineLength = (double)sampleBytes / sampleSize;
                estimatedTotalLines = (int)(fileInfo.Length / avgLineLength);
            }

            progress.Report(new ValidationProgress
            {
                Phase = "Parsing",
                Current = 0,
                Total = estimatedTotalLines
            });
        }

        foreach (var line in File.ReadLines(filePath))
        {
            lineNumber++;

            // Report progress every 1000 lines
            if (progress != null && lineNumber % 1000 == 0)
            {
                progress.Report(new ValidationProgress
                {
                    Phase = "Parsing",
                    Current = lineNumber,
                    Total = estimatedTotalLines > 0 ? estimatedTotalLines : lineNumber
                });
            }

            // Skip empty lines and comment lines (starting with #)
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;

            try
            {
                var recordType = GetRecordType(line);

                switch (recordType)
                {
                    case "CDMH.01":
                    case "CDMH":
                        HeaderRecord = CdmhRecord.Parse(line, lineNumber);
                        break;

                    case "SRFO":
                        FooterRecord = SrfoRecord.Parse(line, lineNumber);
                        break;

                    case "CS01.01":
                    case "CS01":
                        SummaryRecords.Add(Cs01Record.Parse(line, lineNumber));
                        break;

                    case "CD01.01":
                    case "CD01":
                        DetailRecords.Add(Cd01Record.Parse(line, lineNumber));
                        break;

                    case "CDDM":
                        // Ignore CDDM messages as per requirements
                        IgnoredRecords.Add(recordType);
                        break;

                    case var rt when rt.StartsWith("CDD"):
                        // Ignore all CDDM-related records (CDD1, CDD2.01, CDD3.01)
                        IgnoredRecords.Add(recordType);
                        break;

                    default:
                        ParseErrors.Add(new ValidationError
                        {
                            LineNumber = lineNumber,
                            RecordType = recordType,
                            FieldName = "RecordType",
                            ErrorMessage = $"Unknown or unsupported record type: {recordType}",
                            Severity = ValidationSeverity.Warning
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                ParseErrors.Add(new ValidationError
                {
                    LineNumber = lineNumber,
                    RecordType = "UNKNOWN",
                    FieldName = "Parse",
                    ErrorMessage = $"Error parsing line: {ex.Message}",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        // Set actual total lines after parsing
        TotalLines = lineNumber;

        // Report completion with actual count
        if (progress != null)
        {
            progress.Report(new ValidationProgress
            {
                Phase = "Parsing",
                Current = TotalLines,
                Total = TotalLines
            });
        }
    }

    /// <summary>
    /// Extracts the record type from a tab-delimited line.
    /// </summary>
    private string GetRecordType(string line)
    {
        var fields = line.Split('\t');
        return fields.Length > 0 ? fields[0] : string.Empty;
    }

    /// <summary>
    /// Resets the parser state for a new file.
    /// </summary>
    public void Reset()
    {
        HeaderRecord = null;
        FooterRecord = null;
        SummaryRecords.Clear();
        DetailRecords.Clear();
        IgnoredRecords.Clear();
        ParseErrors.Clear();
        TotalLines = 0;
        ValidationIndex = new ValidationIndex();
    }

    /// <summary>
    /// PASS 1: Streams file line-by-line, validates individual records, builds minimal index.
    /// Discards full record objects immediately after extracting index data.
    /// </summary>
    public List<ValidationError> ParseAndIndexFile(
        string filePath,
        IProgress<ValidationProgress>? progress,
        CdmhValidator cdmhValidator,
        SrfoValidator srfoValidator,
        Cs01Validator cs01Validator,
        Cd01Validator cd01Validator)
    {
        var errors = new List<ValidationError>();

        if (!File.Exists(filePath))
        {
            errors.Add(new ValidationError
            {
                LineNumber = 0,
                RecordType = "FILE",
                FieldName = "FilePath",
                ErrorMessage = $"File not found: {filePath}",
                Severity = ValidationSeverity.Error
            });
            return errors;
        }

        int lineNumber = 0;
        int estimatedTotalLines = 0;

        // Estimate total lines for progress reporting
        if (progress != null)
        {
            var fileInfo = new FileInfo(filePath);
            int sampleSize = 0;
            long sampleBytes = 0;
            foreach (var line in File.ReadLines(filePath).Take(10000))
            {
                sampleSize++;
                sampleBytes += System.Text.Encoding.UTF8.GetByteCount(line) + 1;
            }

            if (sampleSize > 0)
            {
                double avgLineLength = (double)sampleBytes / sampleSize;
                estimatedTotalLines = (int)(fileInfo.Length / avgLineLength);
            }
        }

        foreach (var line in File.ReadLines(filePath))
        {
            lineNumber++;

            // Report progress every 1000 lines
            if (progress != null && lineNumber % 1000 == 0)
            {
                progress.Report(new ValidationProgress
                {
                    Phase = "Pass 1/2: Indexing and validating records",
                    Current = lineNumber,
                    Total = estimatedTotalLines > 0 ? estimatedTotalLines : lineNumber
                });
            }

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;

            try
            {
                var recordType = GetRecordType(line);

                switch (recordType)
                {
                    case "CDMH.01":
                    case "CDMH":
                        {
                            var header = CdmhRecord.Parse(line, lineNumber);
                            ValidationIndex.HeaderRecord = header;
                            errors.AddRange(cdmhValidator.Validate(header));
                            break;
                        }

                    case "SRFO":
                        {
                            var footer = SrfoRecord.Parse(line, lineNumber);
                            ValidationIndex.FooterRecord = footer;
                            errors.AddRange(srfoValidator.Validate(footer));
                            break;
                        }

                    case "CS01.01":
                    case "CS01":
                        {
                            var summary = Cs01Record.Parse(line, lineNumber);
                            errors.AddRange(cs01Validator.Validate(summary));

                            // Check for duplicate SummaryRecordId
                            if (!ValidationIndex.SeenSummaryRecordIds.Add(summary.SummaryRecordId))
                            {
                                errors.Add(new ValidationError
                                {
                                    LineNumber = lineNumber,
                                    RecordType = "CS01.01",
                                    FieldName = "SummaryRecordId",
                                    ErrorMessage = $"Duplicate SummaryRecordId '{summary.SummaryRecordId}'",
                                    Severity = ValidationSeverity.Error
                                });
                            }

                            // Store minimal index data (only 6 fields vs 23)
                            ValidationIndex.Summaries[summary.SummaryRecordId] = new SummaryIndex
                            {
                                SummaryRecordId = summary.SummaryRecordId,
                                LineNumber = summary.LineNumber,
                                ServiceDescription = summary.ServiceDescription,
                                TotalClaimedAmount = summary.TotalClaimedAmount,
                                RightsTypeSplitMechanical = summary.RightsTypeSplitMechanical,
                                RightsTypeSplitPerforming = summary.RightsTypeSplitPerforming,
                                AccumulatedDetailAmount = 0,
                                DetailRecordCount = 0
                            };

                            ValidationIndex.SummaryRecordCount++;

                            // Full record object is now garbage collected (KEY MEMORY SAVINGS)
                            break;
                        }

                    case "CD01.01":
                    case "CD01":
                        {
                            var detail = Cd01Record.Parse(line, lineNumber);
                            errors.AddRange(cd01Validator.Validate(detail));

                            // Check for duplicate ClaimId
                            if (!ValidationIndex.SeenClaimIds.Add(detail.ClaimId))
                            {
                                errors.Add(new ValidationError
                                {
                                    LineNumber = lineNumber,
                                    RecordType = "CD01.01",
                                    FieldName = "ClaimId",
                                    ErrorMessage = $"Duplicate ClaimId '{detail.ClaimId}'",
                                    Severity = ValidationSeverity.Error
                                });
                            }

                            ValidationIndex.DetailRecordCount++;

                            // Full record object is now garbage collected (KEY MEMORY SAVINGS)
                            break;
                        }

                    case "CDDM":
                        ValidationIndex.IgnoredRecordCount++;
                        IgnoredRecords.Add(recordType);
                        break;

                    case var rt when rt.StartsWith("CDD"):
                        ValidationIndex.IgnoredRecordCount++;
                        IgnoredRecords.Add(recordType);
                        break;

                    default:
                        errors.Add(new ValidationError
                        {
                            LineNumber = lineNumber,
                            RecordType = recordType,
                            FieldName = "RecordType",
                            ErrorMessage = $"Unknown or unsupported record type: {recordType}",
                            Severity = ValidationSeverity.Warning
                        });
                        break;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ValidationError
                {
                    LineNumber = lineNumber,
                    RecordType = "UNKNOWN",
                    FieldName = "Parse",
                    ErrorMessage = $"Error parsing line: {ex.Message}",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        ValidationIndex.TotalLines = lineNumber;
        TotalLines = lineNumber;

        // Report completion
        if (progress != null)
        {
            progress.Report(new ValidationProgress
            {
                Phase = "Pass 1/2: Indexing and validating records",
                Current = lineNumber,
                Total = lineNumber
            });
        }

        return errors;
    }

    /// <summary>
    /// PASS 2: Re-reads file streaming, validates cross-record business rules using the lightweight index.
    /// </summary>
    public List<ValidationError> ValidateCrossRecordRules(string filePath, IProgress<ValidationProgress>? progress)
    {
        var errors = new List<ValidationError>();

        if (!File.Exists(filePath))
        {
            errors.Add(new ValidationError
            {
                LineNumber = 0,
                RecordType = "FILE",
                FieldName = "FilePath",
                ErrorMessage = $"File not found: {filePath}",
                Severity = ValidationSeverity.Error
            });
            return errors;
        }

        int lineNumber = 0;
        int detailsProcessed = 0;
        int totalDetails = ValidationIndex.DetailRecordCount;

        foreach (var line in File.ReadLines(filePath))
        {
            lineNumber++;

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("#"))
                continue;

            var recordType = GetRecordType(line);

            // Only process CD01 detail records in this pass
            if (recordType == "CD01.01" || recordType == "CD01")
            {
                try
                {
                    var detail = Cd01Record.Parse(line, lineNumber);

                    // Validate CD01 → CS01 reference exists
                    if (!ValidationIndex.Summaries.TryGetValue(detail.SummaryRecordId, out var summaryIndex))
                    {
                        errors.Add(new ValidationError
                        {
                            LineNumber = lineNumber,
                            RecordType = "CD01.01",
                            FieldName = "SummaryRecordId",
                            ErrorMessage = $"SummaryRecordId '{detail.SummaryRecordId}' does not reference any CS01.01 record",
                            Severity = ValidationSeverity.Error
                        });
                    }
                    else
                    {
                        // Accumulate ClaimedAmount for summary total validation
                        summaryIndex.AccumulatedDetailAmount += detail.ClaimedAmount;
                        summaryIndex.DetailRecordCount++;

                        // Validate BlendedShareClaimedForMechAndPerf calculation
                        decimal expectedBlendedShare =
                            (detail.ShareClaimedMechanical * summaryIndex.RightsTypeSplitMechanical / 100m) +
                            (detail.ShareClaimedPerforming * summaryIndex.RightsTypeSplitPerforming / 100m);

                        if (!ValidationHelpers.AreApproximatelyEqual(
                            detail.BlendedShareClaimedForMechAndPerf, expectedBlendedShare, 0.01m))
                        {
                            errors.Add(new ValidationError
                            {
                                LineNumber = lineNumber,
                                RecordType = "CD01.01",
                                FieldName = "BlendedShareClaimedForMechAndPerf",
                                ErrorMessage = $"BlendedShareClaimedForMechAndPerf ({detail.BlendedShareClaimedForMechAndPerf}) does not match calculated value ({expectedBlendedShare:F2}). " +
                                             $"Expected: ({detail.ShareClaimedMechanical} * {summaryIndex.RightsTypeSplitMechanical}/100) + ({detail.ShareClaimedPerforming} * {summaryIndex.RightsTypeSplitPerforming}/100)",
                                Severity = ValidationSeverity.Error
                            });
                        }
                    }

                    detailsProcessed++;

                    // Report progress every 1000 details
                    if (progress != null && detailsProcessed % 1000 == 0)
                    {
                        progress.Report(new ValidationProgress
                        {
                            Phase = "Pass 2/2: Validating cross-record business rules",
                            Current = detailsProcessed,
                            Total = totalDetails
                        });
                    }

                    // Discard detail record immediately (KEY MEMORY SAVINGS)
                }
                catch (Exception ex)
                {
                    errors.Add(new ValidationError
                    {
                        LineNumber = lineNumber,
                        RecordType = "CD01.01",
                        FieldName = "Parse",
                        ErrorMessage = $"Error parsing detail record: {ex.Message}",
                        Severity = ValidationSeverity.Error
                    });
                }
            }
        }

        // Report completion
        if (progress != null)
        {
            progress.Report(new ValidationProgress
            {
                Phase = "Pass 2/2: Validating cross-record business rules",
                Current = detailsProcessed,
                Total = totalDetails
            });
        }

        return errors;
    }

    /// <summary>
    /// POST-PASS: Validates that accumulated detail amounts match summary TotalClaimedAmount.
    /// </summary>
    public List<ValidationError> ValidateSummaryTotals()
    {
        var errors = new List<ValidationError>();

        foreach (var summaryIndex in ValidationIndex.Summaries.Values)
        {
            if (!ValidationHelpers.AreApproximatelyEqual(
                summaryIndex.TotalClaimedAmount, summaryIndex.AccumulatedDetailAmount, 0.02m))
            {
                errors.Add(new ValidationError
                {
                    LineNumber = summaryIndex.LineNumber,
                    RecordType = "CS01.01",
                    FieldName = "TotalClaimedAmount",
                    ErrorMessage = $"TotalClaimedAmount ({summaryIndex.TotalClaimedAmount}) does not match sum of detail ClaimedAmounts ({summaryIndex.AccumulatedDetailAmount:F2})",
                    Severity = ValidationSeverity.Error
                });
            }
        }

        return errors;
    }
}
