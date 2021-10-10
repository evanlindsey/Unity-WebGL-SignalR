$srVersion = "3.1.19"
$txtVersion = "4.7.2"
$target = "netstandard2.0"

$outDir = ".\temp"
$pluginDir = ".\"

nuget install Microsoft.AspNetCore.SignalR.Client -Version $srVersion -OutputDirectory $outDir
nuget install System.Text.Json -Version $txtVersion -OutputDirectory $outDir

$packages = Get-ChildItem -Path $outDir
foreach ($p in $packages) {
	$dll = Get-ChildItem -Path "$($p.FullName)\lib\$($target)\*.dll"
	if (!($null -eq $dll)) {
		$d = $dll[0]
		if (!(Test-Path "$($pluginDir)\$($d.Name)")) {
			Move-Item -Path $d.FullName -Destination $pluginDir
		}
	}
}

Remove-Item $outDir -Recurse
