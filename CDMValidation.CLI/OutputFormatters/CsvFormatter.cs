using CDMValidation.Core.Models;
using System.Text;

namespace CDMValidation.CLI.OutputFormatters;

/// <summary>
/// Formats validation results as CSV for easy import into Excel or data analysis tools.
/// </summary>
public class CsvFormatter
{
    public string FormatResult(ValidationResult result)
    {
        var csv = new StringBuilder();

        // Add statistics section as comments (prefixed with #)
        csv.AppendLine("# CDM Validation Results");
        csv.AppendLine($"# Validation Status: {(result.IsValid ? "PASSED" : "FAILED")}");
        csv.AppendLine("#");
        csv.AppendLine("# Statistics:");
        csv.AppendLine($"# Total Lines: {result.Statistics.TotalLines}");
        csv.AppendLine($"# Total Records: {result.Statistics.TotalRecords}");
        csv.AppendLine($"# Header Records: {result.Statistics.HeaderRecords}");
        csv.AppendLine($"# Summary Records: {result.Statistics.SummaryRecords}");
        csv.AppendLine($"# Detail Records: {result.Statistics.DetailRecords}");
        csv.AppendLine($"# Footer Records: {result.Statistics.FooterRecords}");
        csv.AppendLine($"# Ignored Records: {result.Statistics.IgnoredRecords}");
        csv.AppendLine("#");
        csv.AppendLine("# Financial Totals:");
        csv.AppendLine($"# File Total: ${result.Statistics.TotalClaimedAmount:N2}");

        if (result.Statistics.SummaryTotals.Any())
        {
            csv.AppendLine("#");
            csv.AppendLine("# Summary Breakdown:");
            var sortedSummaries = result.Statistics.SummaryTotals.Values
                .OrderBy(s => s.SummaryRecordId);

            foreach (var summary in sortedSummaries)
            {
                var serviceDesc = !string.IsNullOrWhiteSpace(summary.ServiceDescription)
                    ? $" ({summary.ServiceDescription})"
                    : "";
                csv.AppendLine($"#   {summary.SummaryRecordId}{serviceDesc}: ${summary.TotalClaimedAmount:N2} ({summary.DetailRecordCount} details)");
            }
        }

        csv.AppendLine("#");
        csv.AppendLine($"# Total Errors: {result.Errors.Count(e => e.Severity == ValidationSeverity.Error)}");
        csv.AppendLine($"# Total Warnings: {result.Errors.Count(e => e.Severity == ValidationSeverity.Warning)}");
        csv.AppendLine("#");

        // Add validation errors/warnings section
        csv.AppendLine("RecordType,LineNumber,Severity,FieldName,ErrorMessage");

        // Sort errors by record type, then line number, then severity
        var sortedErrors = result.Errors
            .OrderBy(e => GetRecordTypeOrder(e.RecordType))
            .ThenBy(e => e.LineNumber)
            .ThenBy(e => e.Severity);

        foreach (var error in sortedErrors)
        {
            csv.AppendLine(FormatErrorAsCsvRow(error));
        }

        return csv.ToString();
    }

    public void PrintResult(ValidationResult result)
    {
        var csv = FormatResult(result);
        Console.WriteLine(csv);
    }

    private static string FormatErrorAsCsvRow(ValidationError error)
    {
        // Escape CSV values (handle commas and quotes)
        var recordType = EscapeCsvValue(error.RecordType);
        var lineNumber = error.LineNumber.ToString();
        var severity = error.Severity.ToString();
        var fieldName = EscapeCsvValue(error.FieldName);
        var errorMessage = EscapeCsvValue(error.ErrorMessage);

        return $"{recordType},{lineNumber},{severity},{fieldName},{errorMessage}";
    }

    private static string EscapeCsvValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        // If value contains comma, quote, or newline, wrap in quotes and escape quotes
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }

    private static int GetRecordTypeOrder(string recordType)
    {
        return recordType switch
        {
            "FILE" => 0,
            "CDMH.01" => 1,
            "CDMH" => 1,
            "CS01.01" => 2,
            "CS01" => 2,
            "CD01.01" => 3,
            "CD01" => 3,
            "SRFO" => 4,
            _ => 99
        };
    }
}
