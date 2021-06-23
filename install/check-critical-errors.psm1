Import-Module .\beer-intro.psm1

function Write-CriticalErrorIntro {
    Write-Host @"
    _________                                                     
    /   _____/ ______________  __ ___________                      
    \_____  \_/ __ \_  __ \  \/ // __ \_  __ \                     
    /        \  ___/|  | \/\   /\  ___/|  | \/                     
   /_______  /\___  >__|    \_/  \___  >__|                        
           \/     \/                 \/                            
   __________                 __                 __                
   \______   \ ____   _______/  |______ ________/  |_  ___________ 
    |       _// __ \ /  ___/\   __\__  \\_  __ \   __\/ __ \_  __ \
    |    |   \  ___/ \___ \  |  |  / __ \|  | \/|  | \  ___/|  | \/
    |____|_  /\___  >____  > |__| (____  /__|   |__|  \___  >__|   
           \/     \/     \/            \/                 \/          

   #############################################
   #############################################
   #############################################
"@   
}


function Read-CriticalApplicationLogs {
    param (
        [Parameter(Mandatory = $false)][String]$BeerAppsConfig = ".\beer-apps.json"
    )

    Write-BeerIntro
    Write-CriticalErrorIntro

    $errorLogPaths = @();
    $apps = Get-Content -Raw -Path $beerAppsConfig | ConvertFrom-Json 
    Write-Host "Looking for appliaciton with error logs"
    foreach ($app in $apps) {
        if($app.Name -eq "DaAPI-API")
        {
            $pathToError = (Join-Path -Path $app.OutputDir -ChildPath 'critical-errors.json')
            Write-Host "Found DaAPI API. Path to error log: $pathToError"

            $errorLogPaths += ,$pathToError
        }
    }

    Write-Host "Reading application logs";

    foreach ($errorLogPath in $errorLogPaths) {
        Write-Host "Reading log: $errorLogPath"

        $content = Get-Content -Raw -Path $errorLogPath | ConvertFrom-Json 

        $threshold =  [System.DateTimeOffset]::UtcNow.AddMinutes(-5)
        Write-Host "Looking for events older than: $threshold"
        $counter = 0;

        foreach ($item in $content) {
            $timestamp = [System.DateTimeOffset]::Parse($item.Timestamp)

            if($timestamp -gt $threshold)
            {
                $counter++;
            }
        }

        Write-Host "Found: $counter critical erros in file $errorLogPath" 
        if($counter -ge 5)
        {
            Write-Host "Threshold meet. Initilizing restart" -ForegroundColor DarkRed
            Restart-Computer -Force
            return
        }

        Write-Host "Not critical proceed with next file"
    }

    Write-Host "End of log files. Nothing found"

    return 0;
    
}

