param (
    [Parameter(Mandatory = $true)][String]$ExecutionPath,
    [Parameter(Mandatory = $false)][String]$BeerAppsConfig = ".\beer-apps.json"
)

Set-Location -Path $ExecutionPath
Import-Module .\ping-applications.psm1

Ping-Applications  -BeerAppsConfig $BeerAppsConfig
