param (
    [Parameter(Mandatory = $true)][String]$ExecutionPath,
    [Parameter(Mandatory = $false)][String]$DatabasePath = "C:\ESDB\",
    [Parameter(Mandatory = $false)][String]$OutputPath = "C:\ESDB-Backup\"
)

Set-Location -Path $ExecutionPath
Import-Module .\backup-eventsourcedb.psm1

Save-EventSourceDatabase -DatabasePath $DatabasePath -OutputPath $OutputPath

