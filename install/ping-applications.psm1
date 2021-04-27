Import-Module .\beer-intro.psm1

function Write-AppPingerIntro {
    Write-Host @"
    _____                                   
    /  _  \ ______ ______                    
   /  /_\  \\____ \\____ \                   
  /    |    \  |_> >  |_> >                  
  \____|__  /   __/|   __/                   
          \/|__|   |__|                      
  __________.__                              
  \______   \__| ____    ____   ___________  
   |     ___/  |/    \  / ___\_/ __ \_  __ \ 
   |    |   |  |   |  \/ /_/  >  ___/|  | \/ 
   |____|   |__|___|  /\___  / \___  >__|    
                    \//_____/      \/        

   #############################################
   #############################################
   #############################################
"@   
}


function Ping-Applications {
    param (
        [Parameter(Mandatory = $false)][String]$BeerAppsConfig = ".\beer-apps.json"
    )

    Write-BeerIntro
    Write-AppPingerIntro 

    $errorHappend = $false;

    $apps = Get-Content -Raw -Path $beerAppsConfig | ConvertFrom-Json 
    foreach ($app in $apps) {
        Write-Host "Testing Urls of app: ${app.Name}"
        foreach ($url in $app.Urls) {
            Write-Host "Sending dummy request to: $url"
            $response = try { 
                (Invoke-WebRequest -Uri $url -Method 'Get' -ErrorAction Stop).BaseResponse
            }
            catch { 
                $_.Exception.Response 
            }
			
            $responseStatusCode = [int]$response.StatusCode;
        
            Write-Host "Response code $responseStatusCode"
		
            if ($responseStatusCode -ge 500) {
                $errorHappend = $true
                Write-Host "Unexpected error: $responseStatusCode" -ForegroundColor DarkRed
            }
            else {
                Write-Host "Url: $url is running" -ForegroundColor DarkGreen
            }
        }
    }
	
    if ($errorHappend -eq $true) {
        Write-Host "Error while activating apps" -ForegroundColor DarkRed
        return 1;
    }
    else {
        return 0;
    }
}

