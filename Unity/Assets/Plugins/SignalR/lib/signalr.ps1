$signalRVersion = "6.0.10"
$netTarget = "netstandard2.0"

$tempDir = ".\temp"
$dllDir = ".\dll"

nuget install Microsoft.AspNetCore.SignalR.Client -Version $signalRVersion -OutputDirectory $tempDir

if (!(Test-Path $dllDir)) {
	New-Item -ItemType "directory" -Path $dllDir
}

$packages = Get-ChildItem -Path $tempDir
foreach ($p in $packages) {
	$path = "$($p.FullName)\lib\$($netTarget)"
	if (Test-Path $path) {
		$dll = Get-ChildItem -Path "$($path)\*.dll"
		if (!($null -eq $dll)) {
			$d = $dll[0]
			$out = "$($dllDir)\$($d.Name)"
			if (!(Test-Path $out)) {
				Move-Item -Path $d.FullName -Destination $out
			}
		}
	}
}

Remove-Item $tempDir -Recurse
