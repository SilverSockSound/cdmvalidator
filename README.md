# CDM Validation Tool

A C# application for validating DDEX Claim Detail Message (CDM) files according to the DDEX CDM 2.0 standard specification.

## Overview

This tool validates tab-delimited CDM files containing claim and financial data for musical works. It validates the following record types:

- **CDMH.01** - Header Record for CDM messages
- **SRFO** - Footer Record for CDM messages
- **CS01.01** - Summary Record for claims with financial data
- **CD01.01** - Detail Record for claims with financial data

CDDM (Claim Detail Data Message) records are ignored as per requirements.

## Features

### Validation Layers

1. **Field-Level Validation**
   - Data type validation (string, integer, decimal, datetime, ISO date)
   - Format validation (ISRC, ISWC, DPID, PartyID formats)
   - Fixed string values (e.g., RecordType)
   - Allowed Value Set (AVS) validation

2. **Record-Level Validation**
   - Mandatory field presence
   - Conditional field logic
   - Field value constraints (e.g., decimals between 0-100)

3. **Cross-Record Validation**
   - Summary record reference validation
   - Aggregate calculations (TotalClaimedAmount must match sum of details)
   - Rights type splits must sum to 100%
   - Blended share calculations

4. **File-Level Validation**
   - File must start with CDMH.01
   - File must end with SRFO
   - Line and record counts must match footer values
   - No duplicate ClaimIds or SummaryRecordIds

### Business Rules

#### CDMH.01 Rules
- MessageVersion format: `CDM/x.x/x.x/x.x`
- DateTime format: RFC 3339
- ServiceDescription: no spaces or underscores
- Conditional field dependencies

#### CS01.01 Rules
- `RightsTypeSplitMechanical + RightsTypeSplitPerforming = 100`
- Currency exchange rules (rate required when currencies differ)
- Date range validation (StartOfClaimPeriod ≤ EndOfClaimPeriod)

#### CD01.01 Rules
- Share values: 0-100 range
- `ClaimedAmount = ClaimedAmountMechanical + ClaimedAmountPerforming`
- BlendedShare calculation validation
- If ClaimBasis = "Unmatched", LicensorWorkId must be empty

#### SRFO Rules
- `NumberOfLinesInReport` must match actual file line count
- `NumberOfSummaryRecords` must match actual CS01.01 record count

## Architecture

```
CDMValidation/
├── CDMValidation.Core/           # Core validation library
│   ├── Models/                   # Record type models
│   ├── Validators/               # Validation logic
│   ├── Parsers/                  # File parsing
│   ├── Constants/                # Allowed value sets
│   └── CdmValidationEngine.cs   # Main orchestrator
├── CDMValidation.CLI/            # Console application
│   ├── OutputFormatters/         # Console & JSON output
│   └── Program.cs                # CLI entry point
└── CDMValidation.Tests/          # Unit tests
```

## Installation

### Option 1: Standalone Windows Executable (Recommended for End Users)

**No .NET installation required!**

1. Download `CDMValidation-Windows-x64.zip` from the `publish/` folder
2. Extract the zip file
3. Run `CDMValidation.CLI.exe` from Command Prompt or PowerShell

The standalone executable includes the .NET runtime and works on Windows 10+ (64-bit).

### Option 2: Build from Source

**Prerequisites:**
- .NET 10.0 SDK or later

**Build:**
```bash
cd CDMValidation
dotnet build
```

**Publish your own standalone executable:**
```bash
# Windows 64-bit
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o publish/windows-x64

# Windows 32-bit
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r win-x86 --self-contained -p:PublishSingleFile=true -o publish/windows-x86

# Linux 64-bit
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o publish/linux-x64

# macOS 64-bit
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o publish/osx-x64
```

## Usage

### Command Line (Standalone Executable)

```bash
# Basic validation
CDMValidation.CLI.exe sample.cdm

# Verbose output (includes warnings)
CDMValidation.CLI.exe sample.cdm --verbose

# JSON output
CDMValidation.CLI.exe sample.cdm --json

# Save JSON output to file
CDMValidation.CLI.exe sample.cdm --json > result.json

# Show help
CDMValidation.CLI.exe --help
```

### Command Line (Using .NET SDK)

```bash
# Basic validation
dotnet run --project CDMValidation.CLI -- sample.cdm

# Verbose output (includes warnings)
dotnet run --project CDMValidation.CLI -- sample.cdm --verbose

# JSON output
dotnet run --project CDMValidation.CLI -- sample.cdm --json

# Save JSON output to file
dotnet run --project CDMValidation.CLI -- sample.cdm --json > result.json

# Show help
dotnet run --project CDMValidation.CLI -- --help
```

### As a Library

```csharp
using CDMValidation.Core;

var engine = new CdmValidationEngine();
var result = engine.ValidateFile("path/to/file.cdm");

if (result.IsValid)
{
    Console.WriteLine("Validation passed!");
}
else
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Line {error.LineNumber}: {error.ErrorMessage}");
    }
}
```

## Exit Codes

- **0** - Validation passed (no errors)
- **1** - Validation failed (errors found)
- **2** - Error occurred (file not found, parse error, etc.)

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

## Validation Examples

### Valid File Example

```
CDMH.01	CDM/2.2/5.0/4.2	MSG123	2024-01-01T10:00:00Z	Claims	1.0	...
CS01.01	SUM1		RightsOwner	DPID123::123	...	75	25	100.00
CD01.01	CLM1	SUM1	RES123	USRC12345678	...	50	25	43.75	...	100.00
SRFO	3	1
```

### Common Validation Errors

**Missing Header**
```
Error: File must contain a CDMH.01 header record
```

**Invalid Rights Split**
```
Line 2: RightsTypeSplitMechanical + RightsTypeSplitPerforming must equal 100, found 95
```

**Invalid Reference**
```
Line 3: SummaryRecordId 'SUM2' does not reference any CS01.01 record
```

**Calculation Mismatch**
```
Line 3: ClaimedAmount (150.00) should equal ClaimedAmountMechanical (75.00) + ClaimedAmountPerforming (50.00) = 125.00
```

## Supported Standards

- DDEX Claim Detail Message Suite Standard Version 2.0
- ISO 3901 (ISRC)
- ISO 15707 (ISWC)
- ISO 8601:2004 (Dates)
- RFC 3339 (DateTime)
- ISO 3166-1 alpha-2 (Territory codes)
- ISO 4217 (Currency codes)

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

Edit `CDMValidation.Core/Constants/AllowedValueSets.cs` to add or modify allowed values.

## Known Limitations

- Only validates the four specified record types (CDMH.01, SRFO, CS01.01, CD01.01)
- Allowed Value Sets use a common subset of values (extend as needed for your use case)
- Assumes tab-delimited format (not XML or other formats)
- Currently targets CDM 2.0 specification

## Contributing

To contribute to this project:

1. Add validation rules to the appropriate validator
2. Update tests to cover new validation scenarios
3. Document changes in this README

## License

This is a custom implementation for CDM validation. Refer to your organization's licensing requirements.

## References

- [DDEX Standards](https://ddex.net/standards/claim-detail-message-suite/)
- [DDEX Knowledge Base](https://kb.ddex.net/)
- [CDM Part 2: Record Type Definitions](https://cdm2.ddex.net/claim-detail-message-suite:-part-2-record-type-definition/)

## Support

For issues or questions:
- Check validation error messages for specific guidance
- Review the DDEX CDM specification documentation
- Examine sample CDM files for correct format

---

**Version:** 1.0.0
**Last Updated:** 2026-01-14
**DDEX Standard:** CDM 2.0
