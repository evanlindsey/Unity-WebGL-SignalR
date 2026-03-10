# SignalR Plugin Setup

This guide explains how to set up the SignalR plugin dependencies on Windows, macOS, and Linux.

## Prerequisites

### 1. Install PowerShell

**Windows:**
PowerShell 5.1 is built-in. Alternatively, install PowerShell Core from:
https://github.com/PowerShell/PowerShell/releases

**macOS (Homebrew):**
```bash
brew install powershell/tap/powershell
```

**Linux (Ubuntu/Debian):**
```bash
sudo apt-get install -y powershell
```

**Verify installation:**
```bash
pwsh --version
```

### 2. Install .NET SDK

The setup script uses `dotnet restore` to fetch NuGet packages. The server project targets .NET 10. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download).

**Windows:**
Download from: https://dotnet.microsoft.com/download

**macOS (Homebrew):**
```bash
brew install dotnet
```

**Linux:**
Follow instructions at: https://learn.microsoft.com/en-us/dotnet/core/install/linux

**Note:** Unity 6 generates `.slnx` solution files which require .NET SDK 9.0.200+. You can also use the traditional `.sln` file with older SDK versions.

## Installing SignalR Dependencies

### Windows

1. Open PowerShell and navigate to the lib folder:
   ```powershell
   cd path\to\Unity\Assets\Plugins\SignalR\lib
   ```

2. Run the setup script:
   ```powershell
   .\signalr.ps1
   ```

### macOS / Linux

1. Open a terminal and navigate to the lib folder:
   ```bash
   cd /path/to/Unity/Assets/Plugins/SignalR/lib
   ```

2. Run the setup script:
   ```bash
   pwsh ./signalr.ps1
   ```

3. The script will:
   - Restore `Microsoft.AspNetCore.SignalR.Client` and all dependencies via `dotnet restore`
   - Extract the `netstandard2.0` compatible DLLs to the `dll/` folder
   - Clean up temporary files

4. Refresh Unity (or restart it) to load the new assemblies.

## Troubleshooting

### "HubConnection could not be found" in Unity

1. Ensure the DLLs exist in `Assets/Plugins/SignalR/lib/dll/`
2. Refresh the Unity project (Assets > Refresh or Ctrl/Cmd+R)
3. Check the Unity Console for any import errors

### Script creates weird folder names like `.\temp`

This was a bug in older versions of the script. Update to the latest `signalr.ps1` which uses `Join-Path` for cross-platform compatibility.

## Updating SignalR Version

To update to a newer version of SignalR:

1. Edit `signalr.ps1` and change the version number:
   ```powershell
   $signalRVersion = "10.0.1"  # Change to desired version
   ```

2. Delete the existing `dll/` folder:
   ```bash
   rm -rf dll/
   ```

3. Re-run the script:
   ```bash
   pwsh ./signalr.ps1
   ```

## Platform Support

- **macOS**: Fully supported with PowerShell Core
- **Windows**: Fully supported with PowerShell 5.1+ or PowerShell Core
- **Linux**: Supported with PowerShell Core

## Files

- `signalr.ps1` - Setup script that downloads and extracts SignalR DLLs
- `dll/` - Output directory containing the SignalR assemblies (gitignored)
