using System.Text.Json;
using System.Text.Json.Serialization;
using CDMValidation.Core.Models;

namespace CDMValidation.CLI.OutputFormatters;

/// <summary>
/// Formats validation results as JSON.
/// </summary>
public class JsonFormatter
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public string FormatResult(ValidationResult result)
    {
        return JsonSerializer.Serialize(result, _options);
    }

    public void PrintResult(ValidationResult result)
    {
        var json = FormatResult(result);
        Console.WriteLine(json);
    }
}
