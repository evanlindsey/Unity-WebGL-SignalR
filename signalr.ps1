$srVersion = "3.1.10"
$txtVersion = "4.7.2"
$target = "netstandard2.0"

$outDir = ".\Packages"
$pluginDir = ".\Unity\Assets\Plugins\SignalR\Packages"

nuget install Microsoft.AspNetCore.SignalR.Client -Version $srVersion -OutputDirectory $outDir
nuget install System.Text.Json -Version $txtVersion -OutputDirectory $outDir

$packages = Get-ChildItem -Path $outDir
foreach ($p in $packages) {
	$dll = Get-ChildItem -Path "$($p.FullName)\lib\$($target)\*.dll"
	if (!($dll -eq $null)) {
		$d = $dll[0]
		if (!(Test-Path "$($pluginDir)\$($d.Name)")) {
			Move-Item -Path $d.FullName -Destination $pluginDir
		}
	}
}

Remove-Item $outDir -Recurse
