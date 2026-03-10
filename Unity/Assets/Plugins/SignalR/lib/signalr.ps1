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

# Create a temporary project to restore the NuGet package (uses dotnet CLI, no nuget binary needed)
$projFile = Join-Path $tempDir "restore.csproj"
if (!(Test-Path $tempDir)) {
    New-Item -ItemType "directory" -Path $tempDir | Out-Null
}

$projContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup><TargetFramework>$netTarget</TargetFramework></PropertyGroup>
  <ItemGroup><PackageReference Include="Microsoft.AspNetCore.SignalR.Client" Version="$signalRVersion" /></ItemGroup>
</Project>
"@
Set-Content -Path $projFile -Value $projContent

$packagesDir = Join-Path $tempDir "packages"
dotnet restore $projFile --packages $packagesDir

if (!(Test-Path $packagesDir)) {
    Write-Error "Failed to restore packages. Ensure 'dotnet' CLI is installed and in your PATH."
    exit 1
}

# Create dll directory if it doesn't exist
if (!(Test-Path $dllDir)) {
    New-Item -ItemType "directory" -Path $dllDir | Out-Null
}

# Extract DLLs from restored packages
# dotnet restore layout: {packagesDir}/{name}/{version}/lib/{target}/*.dll
$dlls = Get-ChildItem -Path $packagesDir -Recurse -Filter "*.dll" |
Where-Object { $_.Directory.Name -eq $netTarget -and $_.Directory.Parent.Name -eq "lib" }

foreach ($dll in $dlls) {
    $outPath = Join-Path $dllDir $dll.Name
    if (!(Test-Path $outPath)) {
        Write-Host "  Copying $($dll.Name)"
        Copy-Item -Path $dll.FullName -Destination $outPath
    }
}

# Cleanup
Remove-Item $tempDir -Recurse -Force

Write-Host ""
Write-Host "Done! DLLs installed to: $dllDir"
Write-Host "Refresh Unity to load the new assemblies."
