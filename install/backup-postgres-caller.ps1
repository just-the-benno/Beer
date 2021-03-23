param (
    [Parameter(Mandatory = $true)][String]$ExecutionPath,
    [Parameter(Mandatory = $false)][String]$ConfigFilePath = ".\postgres-dbs.json",
    [Parameter(Mandatory = $false)][String]$User = "postgres",
    [Parameter(Mandatory = $true)][String]$Password,
    [Parameter(Mandatory = $false)][String]$Server = "localhost",
    [Parameter(Mandatory = $false)][String]$Port = 5432,
    [Parameter(Mandatory = $false)][String]$PostgresPath = "C:\Program Files\PostgreSQL\13\bin",
    [Parameter(Mandatory = $false)][String]$OutputPath = ".\postgres-exports"
)

Set-Location -Path $ExecutionPath
Import-Module .\backup-postgres.psm1

Save-PostgresDatabases -ConfigFilePath $ConfigFilePath -User $User -Password $Password -Server $Server -Port $Port -PostgresPath $PostgresPath -OutputPath $OutputPath


