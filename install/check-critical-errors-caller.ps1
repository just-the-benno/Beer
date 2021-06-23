param (
    [Parameter(Mandatory = $true)][String]$ExecutionPath,
    [Parameter(Mandatory = $false)][String]$BeerAppsConfig = ".\beer-apps.json"
)

Set-Location -Path $ExecutionPath
Import-Module .\check-critical-errors.psm1

Read-CriticalApplicationLogs  -BeerAppsConfig $BeerAppsConfig
