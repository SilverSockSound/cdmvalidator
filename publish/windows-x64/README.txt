CDM Validation Tool - Windows Standalone Executable
===================================================

ABOUT
-----
This is a standalone Windows executable for validating DDEX Claim Detail Message (CDM) files.
No .NET installation is required - all dependencies are included.

SYSTEM REQUIREMENTS
-------------------
- Windows 10 or later (64-bit)
- No additional software required

USAGE
-----
Open Command Prompt or PowerShell and run:

    CDMValidation.CLI.exe <file-path> [options]

EXAMPLES
--------
Basic validation:
    CDMValidation.CLI.exe "C:\path\to\file.cdm"

Show detailed output with warnings:
    CDMValidation.CLI.exe "C:\path\to\file.cdm" --verbose

Output as JSON:
    CDMValidation.CLI.exe "C:\path\to\file.cdm" --json

Save JSON results to file:
    CDMValidation.CLI.exe "C:\path\to\file.cdm" --json > results.json

Show help:
    CDMValidation.CLI.exe --help

OPTIONS
-------
  -h, --help           Show help message
  -v, --verbose        Show detailed output including warnings
  -j, --json           Output results in JSON format
  -f, --format <fmt>   Output format: 'console' or 'json' (default: console)

EXIT CODES
----------
  0 - Validation passed (no errors)
  1 - Validation failed (errors found)
  2 - Error occurred (file not found, parse error, etc.)

SUPPORTED RECORD TYPES
----------------------
  - CDMH / CDMH.01   Header Record
  - SRFO             Footer Record
  - CS01 / CS01.01   Summary Record for Claims with Financial Data
  - CD01 / CD01.01   Detail Record for Claims with Financial Data

Note: CDDM messages are ignored as per specification.

VALIDATION FEATURES
-------------------
✓ Format validation (ISRC, ISWC, DPID, PartyID)
✓ Date format support (ISO 8601 and DD/MM/YYYY)
✓ Business rules (amounts, calculations, splits)
✓ Cross-record integrity (references, totals)
✓ File structure validation
✓ Comment line support (lines starting with #)

FILE SIZE
---------
The executable is approximately 71MB because it includes the complete .NET 10 runtime.
This ensures it works on any Windows system without requiring .NET installation.

TROUBLESHOOTING
---------------
If you see "Windows protected your PC" message:
1. Click "More info"
2. Click "Run anyway"

This message appears because the executable is not digitally signed.

For any issues or questions, refer to the main project documentation.

VERSION
-------
1.0.0 (2026-01-14)
DDEX CDM Standard: 2.0
