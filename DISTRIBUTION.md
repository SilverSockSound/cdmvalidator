# CDM Validation Tool - Distribution Guide

## Standalone Windows Executable

### Location
```
/home/matthew/Projects/CDMValidation/publish/CDMValidation-Windows-x64.zip
```

### Contents
- `CDMValidation.CLI.exe` (71 MB) - Standalone executable with embedded .NET runtime
- `README.txt` - User documentation and usage instructions

### File Size
- Uncompressed: ~71 MB
- Compressed (zip): ~31 MB

### System Requirements
- Windows 10 or later (64-bit)
- No additional software or .NET installation required

### Distribution
Simply provide users with the `CDMValidation-Windows-x64.zip` file. They can:
1. Extract the zip file anywhere on their computer
2. Run `CDMValidation.CLI.exe` from Command Prompt or PowerShell
3. No installation or setup needed

## Usage Examples

### Windows Command Prompt
```cmd
C:\Tools> CDMValidation.CLI.exe "C:\Data\claims.cdm"
C:\Tools> CDMValidation.CLI.exe "C:\Data\claims.cdm" --verbose
C:\Tools> CDMValidation.CLI.exe "C:\Data\claims.cdm" --json > results.json
```

### PowerShell
```powershell
PS C:\Tools> .\CDMValidation.CLI.exe "C:\Data\claims.cdm"
PS C:\Tools> .\CDMValidation.CLI.exe "C:\Data\claims.cdm" --verbose
PS C:\Tools> .\CDMValidation.CLI.exe "C:\Data\claims.cdm" --json | Out-File results.json
```

## Publishing for Other Platforms

### Windows 32-bit
```bash
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r win-x86 --self-contained -p:PublishSingleFile=true -o publish/windows-x86
```

### Linux (Ubuntu/Debian)
```bash
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o publish/linux-x64
```

### macOS (Intel)
```bash
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o publish/osx-x64
```

### macOS (Apple Silicon)
```bash
dotnet publish CDMValidation.CLI/CDMValidation.CLI.csproj -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true -o publish/osx-arm64
```

## Security Note

The executable is not digitally signed. When users first run it on Windows, they may see:
- "Windows protected your PC" message
- Users should click "More info" then "Run anyway"

To avoid this warning in production:
1. Obtain a code signing certificate
2. Sign the executable with `signtool.exe`

## Version Information

- **Version:** 1.0.0
- **Build Date:** 2026-01-14
- **.NET Version:** 10.0
- **DDEX Standard:** CDM 2.0
- **Target Platforms:** Windows 10+ (x64)

## File Structure

```
publish/
├── CDMValidation-Windows-x64.zip     (Distribution package - 31 MB)
└── windows-x64/
    ├── CDMValidation.CLI.exe         (Executable - 71 MB)
    └── README.txt                     (User documentation)
```

## Notes

- The large file size (~71 MB) is because the .NET 10 runtime is embedded in the executable
- This is a one-time cost - users don't need to install any frameworks
- Compression reduces the distribution size to ~31 MB
- The executable is fully self-contained and portable
