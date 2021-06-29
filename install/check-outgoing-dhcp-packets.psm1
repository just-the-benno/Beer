Import-Module .\beer-intro.psm1

function Write-DHCPPacketObersverIntro {
    Write-Host @"
    ________    ___ ___  ___________________                
    \______ \  /   |   \ \_   ___ \______   \               
     |    |  \/    ~    \/    \  \/|     ___/               
     |    `   \    Y    /\     \___|    |                   
    /_______  /\___|_  /  \______  /____|                   
            \/       \/          \/                         
    __________                __           __               
    \______   \_____    ____ |  | __ _____/  |_             
     |     ___/\__  \ _/ ___\|  |/ // __ \   __\            
     |    |     / __ \\  \___|    <\  ___/|  |              
     |____|    (____  /\___  >__|_ \\___  >__|              
                    \/     \/     \/    \/                  
    ________ ___.                                           
    \_____  \\_ |__   ______ ______________  __ ___________ 
     /   |   \| __ \ /  ___// __ \_  __ \  \/ // __ \_  __ \
    /    |    \ \_\ \\___ \\  ___/|  | \/\   /\  ___/|  | \/
    \_______  /___  /____  >\___  >__|    \_/  \___  >__|   
            \/    \/     \/     \/                 \/        

   #############################################
   #############################################
   #############################################
"@   
}

function Read-OutgoingDHCPPackets {
    param (
        [Parameter(Mandatory = $false)][String]$TsharkPath = "C:\Program Files\Wireshark\tshark.exe",
        [Parameter(Mandatory = $false)][String]$CapturePath = "C:\BeerTemp\DHCPPacketOutput\",
        [Parameter(Mandatory = $false)][String]$InterfaceId = "5",
        [Parameter(Mandatory = $false)][int]$Duration = 600
    )

    Write-BeerIntro
    Write-DHCPPacketObersverIntro
    
    Write-Host "Get a random number for this capture. Creating directories..."

    $randomNumer = Get-Random
    $captureFinalDirectory = (Join-Path -Path $CapturePath -ChildPath $randomNumer ) 

    New-Item -Path $captureFinalDirectory -ItemType Directory *>$null

    $captureFinalPath = Join-Path -Path $captureFinalDirectory -ChildPath "dhcpv4capture.pcap"

    Write-Host "Starting the capturing process"
    $argList = "-i $InterfaceId -f `"udp and port 67`" -w `"$captureFinalPath`" --autostop duration:$Duration"
	$argList
    $process = Start-Process -FilePath $TsharkPath -ArgumentList  $argList -NoNewWindow
    Write-Host "Capturing process started. Process Id: $($process.Id)"
    Write-Host "Wating for $($Duration + 20) seconds"

    for ($i = 0; $i -lt $Duration + 20; $i++) {
        Start-Sleep -Seconds 1
        Write-Host "Wating for another second. Remaining: $($Duration - $i +20) seconds"
    }

    if ($process.HasExited -eq $false) {
        $process.Kill();
    }

    Write-Host "Capturing process finished. Starting anlyzing..."

    foreach ($item in Get-ChildItem -Path $captureFinalDirectory) {
        $content = & "$TsharkPath" -r `"$($item.FullName)`" -q -z dhcp,stat -2
        $ackLine = $content[10]
        $numberValue = $ackLine.Substring($ackLine.IndexOf('|') + 1, $ackLine.Length - $ackLine.LastIndexOf('|') + 1).Trim()
        Write-Host "Found $numberValue ACK in sample"
        if ($numberValue -eq "0") {
            Write-Host "No ACK found. Requesting restart" -ForegroundColor DarkRed
            Restart-Computer -Force
            return;
        }
        else {
            Write-Host "Well. That is more than zero. Nothing to worry about."

        }
    }

    Write-Host "End of analyzing. Nothing found"

    return 0;
}
