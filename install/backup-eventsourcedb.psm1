Import-Module .\beer-intro.psm1

function Write-EventSourceDBBackup {
    Write-Host @"
    ___________                    __   _________                                  
    \_   _____/__  __ ____   _____/  |_/   _____/ ____  __ _________   ____  ____  
     |    __)_\  \/ // __ \ /    \   __\_____  \ /  _ \|  |  \_  __ \_/ ___\/ __ \ 
     |        \\   /\  ___/|   |  \  | /        (  <_> )  |  /|  | \/\  \__\  ___/ 
    /_______  / \_/  \___  >___|  /__|/_______  /\____/|____/ |__|    \___  >___  >
            \/           \/     \/            \/                          \/    \/ 
            ________          __        ___.                                               
            \______ \ _____ _/  |______ \_ |__ _____    ______ ____                        
            |    |  \\__  \\   __\__  \ | __ \\__  \  /  ___// __ \                       
            |    `   \/ __ \|  |  / __ \| \_\ \/ __ \_\___ \\  ___/                       
            /_______  (____  /__| (____  /___  (____  /____  >\___  >                      
                    \/     \/          \/    \/     \/     \/     \/                       
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


function Save-EventSourceDatabase {
    param (
        [Parameter(Mandatory = $false)][String]$DatabasePath = "C:\ESDB\",
        [Parameter(Mandatory = $false)][String]$OutputPath = "C:\ESDB-Backup\"
    )

    Write-BeerIntro
    Write-EventSourceDBBackup 

    if ( (Test-Path $OutputPath) -eq $false) {
        New-Item $OutputPath -ItemType Directory
    }

    $timestamp = "$([System.DateTime]::Now.ToString("dd-MM-yyyy",[System.Globalization.CultureInfo]::InvariantCulture).Replace('/','-'))-backup";
    Write-Host $timestamp;
    $outputDirWithTimestamp = Join-Path -Path $OutputPath -ChildPath $timestamp
    New-Item $outputDirWithTimestamp -ItemType Directory

    Copy-Item -Path (Join-Path -Path $DatabasePath -ChildPath "Index") -Destination (Join-Path -Path $outputDirWithTimestamp -ChildPath "Index") -Recurse
    Copy-Item -Path (Join-Path -Path $DatabasePath -ChildPath "Data") -Destination (Join-Path -Path $outputDirWithTimestamp -ChildPath "Data") -Recurse

    return 0;
   
}

