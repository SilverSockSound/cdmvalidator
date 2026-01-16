using CDMValidation.Core;
using CDMValidation.CLI.OutputFormatters;

namespace CDMValidation.CLI;

class Program
{
    static int Main(string[] args)
    {
        var options = ParseArguments(args);

        if (options == null || options.ShowHelp)
        {
            ShowHelp();
            return options?.ShowHelp == true ? 0 : 2;
        }

        try
        {
            // Create validation engine
            var engine = new CdmValidationEngine();

            // Create progress reporter
            var progressReporter = new ProgressReporter(isJsonOutput: options.OutputFormat == OutputFormat.Json);

            // Validate the file with progress reporting
            var result = engine.ValidateFile(options.FilePath, progressReporter);

            // Complete progress reporting (clears progress bar)
            progressReporter.Complete();

            // Output results
            if (options.OutputFormat == OutputFormat.Json)
            {
                var jsonFormatter = new JsonFormatter();
                jsonFormatter.PrintResult(result);
            }
            else
            {
                var consoleFormatter = new ConsoleFormatter();
                consoleFormatter.PrintResult(result, options.Verbose);
            }

            // Return appropriate exit code
            return result.IsValid ? 0 : 1;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();

            if (options.Verbose)
            {
                Console.Error.WriteLine();
                Console.Error.WriteLine("Stack trace:");
                Console.Error.WriteLine(ex.StackTrace);
            }

            return 2;
        }
    }

    static CommandLineOptions? ParseArguments(string[] args)
    {
        if (args.Length == 0)
            return null;

        var options = new CommandLineOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg.ToLower())
            {
                case "-h":
                case "--help":
                    options.ShowHelp = true;
                    return options;

                case "-v":
                case "--verbose":
                    options.Verbose = true;
                    break;

                case "-j":
                case "--json":
                    options.OutputFormat = OutputFormat.Json;
                    break;

                case "-f":
                case "--format":
                    if (i + 1 < args.Length)
                    {
                        i++;
                        options.OutputFormat = args[i].ToLower() switch
                        {
                            "json" => OutputFormat.Json,
                            "console" => OutputFormat.Console,
                            _ => OutputFormat.Console
                        };
                    }
                    break;

                default:
                    if (!arg.StartsWith("-") && string.IsNullOrEmpty(options.FilePath))
                    {
                        options.FilePath = arg;
                    }
                    break;
            }
        }

        if (string.IsNullOrEmpty(options.FilePath))
        {
            Console.Error.WriteLine("Error: File path is required.");
            return null;
        }

        return options;
    }

    static void ShowHelp()
    {
        Console.WriteLine();
        Console.WriteLine("CDM Validation Tool");
        Console.WriteLine("Validates DDEX Claim Detail Message (CDM) files");
        Console.WriteLine();
        Console.WriteLine("Usage:");
        Console.WriteLine("  CDMValidation.CLI <file-path> [options]");
        Console.WriteLine();
        Console.WriteLine("Arguments:");
        Console.WriteLine("  <file-path>           Path to the CDM file to validate");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -h, --help           Show this help message");
        Console.WriteLine("  -v, --verbose        Show detailed output including warnings");
        Console.WriteLine("  -j, --json           Output results in JSON format");
        Console.WriteLine("  -f, --format <fmt>   Output format: 'console' or 'json' (default: console)");
        Console.WriteLine();
        Console.WriteLine("Exit Codes:");
        Console.WriteLine("  0 - Validation passed (no errors)");
        Console.WriteLine("  1 - Validation failed (errors found)");
        Console.WriteLine("  2 - Error occurred (file not found, parse error, etc.)");
        Console.WriteLine();
        Console.WriteLine("Supported Record Types:");
        Console.WriteLine("  - CDMH.01   Header Record");
        Console.WriteLine("  - SRFO      Footer Record");
        Console.WriteLine("  - CS01.01   Summary Record for Claims with Financial Data");
        Console.WriteLine("  - CD01.01   Detail Record for Claims with Financial Data");
        Console.WriteLine();
        Console.WriteLine("Note: CDDM messages are ignored as per specification.");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  CDMValidation.CLI sample.cdm");
        Console.WriteLine("  CDMValidation.CLI sample.cdm --verbose");
        Console.WriteLine("  CDMValidation.CLI sample.cdm --json > result.json");
        Console.WriteLine();
    }
}

class CommandLineOptions
{
    public string FilePath { get; set; } = string.Empty;
    public bool Verbose { get; set; }
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Console;
    public bool ShowHelp { get; set; }
}

enum OutputFormat
{
    Console,
    Json
}
