using System.Text.Json;
using System.Text.Json.Serialization;
using CDMValidation.Core.Models;

namespace CDMValidation.CLI.OutputFormatters;

/// <summary>
/// JSON source generation context for AOT and trimming support.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ValidationResult))]
[JsonSerializable(typeof(ValidationError))]
[JsonSerializable(typeof(ValidationWarning))]
[JsonSerializable(typeof(ValidationStatistics))]
[JsonSerializable(typeof(SummaryTotal))]
[JsonSerializable(typeof(ValidationSeverity))]
[JsonSerializable(typeof(Dictionary<string, SummaryTotal>))]
internal partial class ValidationJsonContext : JsonSerializerContext
{
}

/// <summary>
/// Formats validation results as JSON.
/// </summary>
public class JsonFormatter
{
    public string FormatResult(ValidationResult result)
    {
        return JsonSerializer.Serialize(result, ValidationJsonContext.Default.ValidationResult);
    }

    public void PrintResult(ValidationResult result)
    {
        var json = FormatResult(result);
        Console.WriteLine(json);
    }
}
