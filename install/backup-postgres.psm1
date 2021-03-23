Import-Module .\beer-intro.psm1

function Write-PostgresBackup {
    Write-Host @"
    __________               __                               
    \______   \____  _______/  |_  ___________   ____   ______
     |     ___/  _ \/  ___/\   __\/ ___\_  __ \_/ __ \ /  ___/
     |    |  (  <_> )___ \  |  | / /_/  >  | \/\  ___/ \___ \ 
     |____|   \____/____  > |__| \___  /|__|    \___  >____  >
                        \/      /_____/             \/     \/   
    
            __________                __                 
            \______   \_____    ____ |  | ____ ________  
            |    |  _/\__  \ _/ ___\|  |/ /  |  \____ \ 
            |    |   \ / __ \\  \___|    <|  |  /  |_> >
            |______  /(____  /\___  >__|_ \____/|   __/ 
                    \/      \/     \/     \/     |__|    

   #############################################################################
   #############################################################################
   #############################################################################
"@   
}


function Save-PostgresDatabases {
    param (
        [Parameter(Mandatory = $false)][String]$ConfigFilePath = ".\postgres-dbs.json",
        [Parameter(Mandatory = $false)][String]$User = "postgres",
        [Parameter(Mandatory = $true)][String]$Password,
        [Parameter(Mandatory = $false)][String]$Server = "localhost",
        [Parameter(Mandatory = $false)][String]$Port = 5432,
        [Parameter(Mandatory = $false)][String]$PostgresPath = "C:\Program Files\PostgreSQL\13\bin",
        [Parameter(Mandatory = $false)][String]$OutputPath = ".\postgres-exports"
    )

    Write-BeerIntro
    Write-PostgresBackup 

    $dbs = Get-Content -Raw -Path $ConfigFilePath | ConvertFrom-Json  

    $pathToPgDump = Join-Path -Path $PostgresPath -ChildPath "pg_dump"

    $Env:PGPASSWORD = $Password

    if ( (Test-Path $OutputPath) -eq $false) {
        New-Item $OutputPath -ItemType Directory
    }

    # check if output directory exists

    $sum = 0

    foreach ($dbName in $dbs) {
        $outputFile = Join-Path -Path $OutputPath -ChildPath "$dbName-$([System.DateTime]::Now.ToString("dd-MM-yyyy",[System.Globalization.CultureInfo]::InvariantCulture).Replace('/','-'))-backup.sql"
        
        Write-Host "Backing up db $dbName to $outputFile"

        $command = "& `'$pathToPgDump`'" + " -d $dbName -U $User -h $Server -p $Port -w -f $outputFile -F p -b -C --inserts"
        Invoke-Expression -Command $command 

        $exit = $LASTEXITCODE
        $sum += $exit

        if($LASTEXITCODE -eq 0)
        {
            Write-Host "Database $dbName successfully exported" -ForegroundColor DarkGreen
        }
        else {
            Write-Host "Database $dbName couldn't be exported" -ForegroundColor DarkRed
        }
    }

    if($sum -eq 0)
    {
        return 0;
    }
    else {
        return 1;
    }
}

