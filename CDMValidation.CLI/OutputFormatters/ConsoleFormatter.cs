using CDMValidation.Core.Models;

namespace CDMValidation.CLI.OutputFormatters;

/// <summary>
/// Formats validation results for console output.
/// </summary>
public class ConsoleFormatter
{
    public void PrintResult(ValidationResult result, bool verbose = false)
    {
        Console.WriteLine();
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("CDM VALIDATION RESULT");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();

        // Print overall status
        if (result.IsValid)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ VALIDATION PASSED");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ VALIDATION FAILED");
            Console.ResetColor();
        }

        Console.WriteLine();

        // Print statistics
        Console.WriteLine("Statistics:");
        Console.WriteLine($"  Total Lines:       {result.Statistics.TotalLines}");
        Console.WriteLine($"  Total Records:     {result.Statistics.TotalRecords}");
        Console.WriteLine($"  Header Records:    {result.Statistics.HeaderRecords}");
        Console.WriteLine($"  Summary Records:   {result.Statistics.SummaryRecords}");
        Console.WriteLine($"  Detail Records:    {result.Statistics.DetailRecords}");
        Console.WriteLine($"  Footer Records:    {result.Statistics.FooterRecords}");
        Console.WriteLine($"  Ignored Records:   {result.Statistics.IgnoredRecords}");
        Console.WriteLine();

        // Print error summary
        var errorCount = result.Errors.Count(e => e.Severity == ValidationSeverity.Error);
        var warningCount = result.Errors.Count(e => e.Severity == ValidationSeverity.Warning);

        Console.WriteLine($"Issues Found:");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"  Errors:   {errorCount}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  Warnings: {warningCount}");
        Console.ResetColor();
        Console.WriteLine();

        // Print errors
        if (result.Errors.Any())
        {
            Console.WriteLine("-".PadRight(80, '-'));
            Console.WriteLine("VALIDATION ISSUES:");
            Console.WriteLine("-".PadRight(80, '-'));
            Console.WriteLine();

            // Group errors by record type
            var errorsByType = result.Errors
                .GroupBy(e => e.RecordType)
                .OrderBy(g => g.Key);

            foreach (var group in errorsByType)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[{group.Key}]");
                Console.ResetColor();

                var sortedErrors = group.OrderBy(e => e.LineNumber).ThenBy(e => e.Severity);

                foreach (var error in sortedErrors)
                {
                    // Skip warnings if not in verbose mode
                    if (!verbose && error.Severity == ValidationSeverity.Warning)
                        continue;

                    PrintError(error);
                }

                Console.WriteLine();
            }
        }

        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();
    }

    private void PrintError(ValidationError error)
    {
        string severitySymbol = error.Severity switch
        {
            ValidationSeverity.Error => "✗",
            ValidationSeverity.Warning => "⚠",
            ValidationSeverity.Info => "ℹ",
            _ => "•"
        };

        ConsoleColor severityColor = error.Severity switch
        {
            ValidationSeverity.Error => ConsoleColor.Red,
            ValidationSeverity.Warning => ConsoleColor.Yellow,
            ValidationSeverity.Info => ConsoleColor.Cyan,
            _ => ConsoleColor.White
        };

        Console.ForegroundColor = severityColor;
        Console.Write($"  {severitySymbol} ");
        Console.ResetColor();

        Console.Write($"Line {error.LineNumber}: ");

        if (!string.IsNullOrWhiteSpace(error.FieldName))
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{error.FieldName}] ");
            Console.ResetColor();
        }

        Console.WriteLine(error.ErrorMessage);
    }
}
