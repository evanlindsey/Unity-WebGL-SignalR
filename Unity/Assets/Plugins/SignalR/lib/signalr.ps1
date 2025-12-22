$signalRVersion = "10.0.1"
$netTarget = "netstandard2.0"

# Use cross-platform paths (Join-Path handles OS-specific separators)
$scriptDir = $PSScriptRoot
if ([string]::IsNullOrEmpty($scriptDir)) {
    $scriptDir = (Get-Location).Path
}

$tempDir = Join-Path $scriptDir "temp"
$dllDir = Join-Path $scriptDir "dll"

Write-Host "Installing SignalR Client v$signalRVersion..."
Write-Host "Temp directory: $tempDir"
Write-Host "DLL directory: $dllDir"

# Install NuGet package
nuget install Microsoft.AspNetCore.SignalR.Client -Version $signalRVersion -OutputDirectory $tempDir

if (!(Test-Path $tempDir)) {
    Write-Error "Failed to download packages. Ensure 'nuget' CLI is installed and in your PATH."
    exit 1
}

# Create dll directory if it doesn't exist
if (!(Test-Path $dllDir)) {
    New-Item -ItemType "directory" -Path $dllDir | Out-Null
}

# Extract DLLs from packages
$packages = Get-ChildItem -Path $tempDir -Directory
foreach ($p in $packages) {
    # Use Join-Path for cross-platform compatibility
    $libPath = Join-Path $p.FullName "lib" $netTarget
    if (Test-Path $libPath) {
        $dlls = Get-ChildItem -Path (Join-Path $libPath "*.dll")
        foreach ($dll in $dlls) {
            $outPath = Join-Path $dllDir $dll.Name
            if (!(Test-Path $outPath)) {
                Write-Host "  Copying $($dll.Name)"
                Copy-Item -Path $dll.FullName -Destination $outPath
            }
        }
    }
}

# Cleanup
Remove-Item $tempDir -Recurse -Force

Write-Host ""
Write-Host "Done! DLLs installed to: $dllDir"
Write-Host "Refresh Unity to load the new assemblies."
