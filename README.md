# CDM Validation Tool

A high-performance C# application for validating DDEX Claim Detail Message (CDM) files according to the DDEX CDM 2.0 standard specification.

## Overview

This tool validates tab-delimited CDM files containing claim and financial data for musical works. It uses a streaming two-pass validation approach for memory efficiency and comprehensive validation coverage.

### Supported Record Types

- **CDMH.01** - Header Record for CDM messages
- **CS01.01** - Summary Record for claims with financial data
- **CD01.01** - Detail Record for claims with financial data
- **SRFO** - Footer Record for CDM messages

## Validation Layers

The validation engine performs comprehensive checks across multiple layers:

### 1. Individual Record Validation (Pass 1)

Each record type is validated according to its specific requirements:

#### CDMH.01 (Header) Validation
- MessageVersion format must be `CDM/x.x/x.x/x.x`
- DateTime format must conform to RFC 3339
- ServiceDescription: no spaces or underscores allowed
- Conditional field dependencies
- All mandatory fields must be present
- Data type validation (string, integer, decimal, datetime, ISO date)

#### CS01.01 (Summary) Validation
- `RightsTypeSplitMechanical + RightsTypeSplitPerforming` must equal 100
- Currency exchange rules (rate required when currencies differ)
- Date range validation (`StartOfClaimPeriod ≤ EndOfClaimPeriod`)
- Format validation for DPID, PartyID
- Mandatory field presence checks
- Allowed Value Set (AVS) validation

#### CD01.01 (Detail) Validation
- Share values must be in 0-100 range
- `ClaimedAmount = ClaimedAmountMechanical + ClaimedAmountPerforming`
- BlendedShare calculation validation
- If `ClaimBasis = "Unmatched"`, LicensorWorkId must be empty
- Format validation for ISRC, ISWC identifiers
- Reference validation for summary records

#### SRFO (Footer) Validation
- NumberOfLinesInReport must match actual file line count
- NumberOfSummaryRecords must match actual CS01.01 record count

### 2. Cross-Record Validation (Pass 2)

- Detail records must reference valid summary records
- SummaryRecordId references are validated
- Aggregate calculations across related records
- Rights type splits consistency

### 3. Summary Totals Validation (Post-Pass)

- TotalClaimedAmount in summary records must match sum of detail record amounts
- Rights splits validated across all detail records for each summary

### 4. File Structure Validation

- File must start with CDMH.01 header
- File must end with SRFO footer
- No duplicate ClaimIds
- No duplicate SummaryRecordIds
- Line and record counts must match footer values

### Format Validation

The validators check the following formats and standards:
- **ISRC** (ISO 3901) - International Standard Recording Code
- **ISWC** (ISO 15707) - International Standard Musical Work Code
- **DPID** - DDEX Party Identifier format
- **PartyID** - Party identifier format
- **ISO Date** (ISO 8601:2004) - Date format
- **DateTime** (RFC 3339) - Date and time format
- **Territory Codes** (ISO 3166-1 alpha-2)
- **Currency Codes** (ISO 4217)
- **Allowed Value Sets** - DDEX-defined value constraints

## Command Line Usage

### Basic Syntax

```bash
CDMValidation.CLI <file-path> [options]
```

### Options

| Option | Shorthand | Description |
|--------|-----------|-------------|
| `--help` | `-h` | Display help message and exit |
| `--verbose` | `-v` | Show detailed output including warnings |
| `--json` | `-j` | Output results in JSON format |
| `--csv` | `-c` | Output results in CSV format |
| `--format <fmt>` | `-f` | Specify output format: `console`, `json`, or `csv` (default: `console`) |

### Examples

```bash
# Basic validation with console output
CDMValidation.CLI sample.cdm

# Show detailed validation information
CDMValidation.CLI sample.cdm --verbose

# Output results in JSON format
CDMValidation.CLI sample.cdm --json

# Output results in CSV format
CDMValidation.CLI sample.cdm --csv

# Save JSON output to file
CDMValidation.CLI sample.cdm --json > validation-results.json

# Save CSV output to file
CDMValidation.CLI sample.cdm --csv > validation-results.csv

# Alternative format specification
CDMValidation.CLI sample.cdm --format json

# Show help
CDMValidation.CLI --help
```

### Using with .NET SDK

If running from source with the .NET SDK:

```bash
# Basic validation
dotnet run --project CDMValidation.CLI -- sample.cdm

# With verbose output
dotnet run --project CDMValidation.CLI -- sample.cdm --verbose

# JSON output
dotnet run --project CDMValidation.CLI -- sample.cdm --json

# CSV output
dotnet run --project CDMValidation.CLI -- sample.cdm --csv
```

## Exit Codes

The CLI returns standard exit codes for scripting and automation:

| Exit Code | Meaning | Description |
|-----------|---------|-------------|
| **0** | Success | Validation passed with no errors |
| **1** | Validation Failed | Errors found in the CDM file |
| **2** | Runtime Error | File not found, parse error, or system error |

## Installation

### Option 1: Standalone Executable (Recommended)

**No .NET installation required!**

1. Download the appropriate package for your platform from the `publish/` folder
2. Extract the archive
3. Run the executable directly from terminal/command prompt

The standalone executable includes the .NET runtime and works on:
- Windows 10+ (64-bit)
- Linux (64-bit)
- macOS (64-bit)

### Option 2: Build from Source

**Prerequisites:**
- .NET 10.0 SDK or later

**Build:**
```bash
cd CDMValidation
dotnet build
```

**Create Standalone Executable:**
```bash
# Windows 64-bit
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/windows-x64

# Windows 32-bit
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r win-x86 --self-contained -p:PublishSingleFile=true -o publish/windows-x86

# Linux 64-bit
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o publish/linux-x64

# macOS 64-bit (Intel)
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o publish/osx-x64

# macOS ARM64 (Apple Silicon)
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true -o publish/osx-arm64
```

## File Format

CDM files must be tab-delimited text files with the following structure:

```
CDMH.01	<field2>	<field3>	...
CS01.01	<field2>	<field3>	...
CD01.01	<field2>	<field3>	...
CD01.01	<field2>	<field3>	...
SRFO	<field2>	<field3>
```

### Multiple Value Fields

Some fields support multiple values separated by pipe (`|`) character:
- AlternativeMusicalWorkTitle
- MusicalWorkComposerAuthorName
- MusicalWorkComposerAuthorPartyId
- TariffParameterType
- TariffParameterValue

### Example Valid File

```
CDMH.01	CDM/2.2/5.0/4.2	MSG123	2024-01-01T10:00:00Z	Claims	1.0	...
CS01.01	SUM1		RightsOwner	DPID123::123	...	75	25	100.00
CD01.01	CLM1	SUM1	RES123	USRC12345678	...	50	25	43.75	...	100.00
SRFO	3	1
```

## Output Formats

### Console Output (Default)

Formatted, color-coded output designed for human readability:
- Summary statistics
- Validation status
- Error and warning details with line numbers
- Progress indicators during validation

### JSON Output

Structured JSON format for programmatic integration:
```json
{
  "isValid": false,
  "errors": [
    {
      "lineNumber": 3,
      "recordType": "CD01.01",
      "errorMessage": "ClaimedAmount calculation error",
      "severity": "Error"
    }
  ],
  "statistics": {
    "totalLines": 4,
    "totalRecords": 4,
    "summaryRecords": 1,
    "detailRecords": 1
  }
}
```

### CSV Output

Comma-separated values format for spreadsheet import:
```csv
LineNumber,RecordType,Severity,ErrorMessage
3,CD01.01,Error,"ClaimedAmount calculation error"
```

## Common Validation Errors

### Missing Header
```
Error: File must contain a CDMH.01 header record
```

### Invalid Rights Split
```
Line 2: RightsTypeSplitMechanical + RightsTypeSplitPerforming must equal 100, found 95
```

### Invalid Reference
```
Line 3: SummaryRecordId 'SUM2' does not reference any CS01.01 record
```

### Calculation Mismatch
```
Line 3: ClaimedAmount (150.00) should equal ClaimedAmountMechanical (75.00) + ClaimedAmountPerforming (50.00) = 125.00
```

### Format Error
```
Line 2: Invalid ISRC format. Expected format: CC-XXX-YY-NNNNN
```

## Architecture

```
CDMValidation/
├── CDMValidation.Core/              # Core validation library
│   ├── Models/                      # Record type models
│   │   ├── CdmhRecord.cs
│   │   ├── Cs01Record.cs
│   │   ├── Cd01Record.cs
│   │   ├── SrfoRecord.cs
│   │   ├── ValidationResult.cs
│   │   └── StreamingValidationIndex.cs
│   ├── Validators/                  # Validation logic
│   │   ├── CdmhValidator.cs
│   │   ├── Cs01Validator.cs
│   │   ├── Cd01Validator.cs
│   │   ├── SrfoValidator.cs
│   │   ├── FileStructureValidator.cs
│   │   ├── BusinessRuleValidator.cs
│   │   └── ValidationHelpers.cs
│   ├── Parsers/                     # File parsing
│   │   └── CdmFileParser.cs
│   ├── Constants/                   # Allowed value sets
│   │   └── AllowedValueSets.cs
│   └── CdmValidationEngine.cs      # Main orchestrator
├── CDMValidation.CLI/               # Console application
│   ├── OutputFormatters/            # Output formatters
│   │   ├── ConsoleFormatter.cs
│   │   ├── JsonFormatter.cs
│   │   └── CsvFormatter.cs
│   └── Program.cs                   # CLI entry point
└── CDMValidation.Tests/             # Unit tests
```

## Using as a Library

You can integrate the validation engine into your own applications:

```csharp
using CDMValidation.Core;

// Create validation engine
var engine = new CdmValidationEngine();

// Validate file
var result = engine.ValidateFile("path/to/file.cdm");

// Check results
if (result.IsValid)
{
    Console.WriteLine("Validation passed!");
    Console.WriteLine($"Total records: {result.Statistics.TotalRecords}");
    Console.WriteLine($"Total claimed amount: {result.Statistics.TotalClaimedAmount}");
}
else
{
    Console.WriteLine("Validation failed!");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Line {error.LineNumber}: {error.ErrorMessage}");
    }
}

// Access warnings
foreach (var warning in result.Errors.Where(e => e.Severity == ValidationSeverity.Warning))
{
    Console.WriteLine($"Warning on line {warning.LineNumber}: {warning.ErrorMessage}");
}
```

### With Progress Reporting

```csharp
var engine = new CdmValidationEngine();

var progress = new Progress<ValidationProgress>(p =>
{
    Console.WriteLine($"{p.Phase}: {p.Current}/{p.Total}");
});

var result = engine.ValidateFile("path/to/file.cdm", progress);
```

## Performance

The validation engine uses a streaming two-pass approach for optimal performance:

- **Pass 1**: Parse records, validate individual records, build lightweight index
- **Pass 2**: Validate cross-record business rules using index
- **Post-Pass**: Validate summary totals and file structure

This approach ensures:
- Low memory footprint (suitable for large files)
- Fast validation with progress reporting
- Comprehensive validation coverage

## Supported Standards

- **DDEX** Claim Detail Message Suite Standard Version 2.0
- **ISO 3901** - International Standard Recording Code (ISRC)
- **ISO 15707** - International Standard Musical Work Code (ISWC)
- **ISO 8601:2004** - Date and time format
- **RFC 3339** - Date and time on the Internet
- **ISO 3166-1 alpha-2** - Territory codes
- **ISO 4217** - Currency codes

## Development

### Running Tests

```bash
dotnet test
```

### Adding New Validators

1. Create a validator class implementing `IRecordValidator<T>`
2. Add validation logic in the `Validate` method
3. Register the validator in `CdmValidationEngine`

### Extending Allowed Value Sets

Edit `CDMValidation.Core/Constants/AllowedValueSets.cs` to add or modify allowed values for:
- Territory codes
- Currency codes
- Rights types
- Claim bases
- And other DDEX-defined value sets

## Known Limitations

- Validates only the four specified record types (CDMH.01, CS01.01, CD01.01, SRFO)
- Allowed Value Sets use a common subset (extend as needed for specific use cases)
- Assumes tab-delimited format (XML or other formats not supported)
- Targets DDEX CDM 2.0 specification

## Contributing

To contribute to this project:

1. Add validation rules to the appropriate validator class
2. Write unit tests to cover new validation scenarios
3. Update this README to document changes
4. Ensure all tests pass before submitting

## References

- [DDEX Standards](https://ddex.net/standards/claim-detail-message-suite/)
- [DDEX Knowledge Base](https://kb.ddex.net/)
- [CDM Part 2: Record Type Definitions](https://cdm2.ddex.net/claim-detail-message-suite:-part-2-record-type-definition/)
- [ISO 3901 - ISRC](https://www.ifpi.org/isrc/)
- [ISO 15707 - ISWC](https://www.iswc.org/)

## License

This is a custom implementation for CDM validation. Refer to your organization's licensing requirements.

## Support

For issues or questions:
- Check validation error messages for specific guidance
- Review the DDEX CDM 2.0 specification documentation
- Examine sample CDM files for correct format examples
- Run with `--verbose` flag for detailed validation information

---

**Version:** 1.0.0
**Last Updated:** 2026-01-16
**DDEX Standard:** CDM 2.0
**Platform:** Cross-platform (.NET 10.0)
