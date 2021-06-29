param (
    [Parameter(Mandatory = $true)][String]$ExecutionPath,
    [Parameter(Mandatory = $false)][String]$TsharkPath = "C:\Program Files\Wireshark\tshark.exe",
    [Parameter(Mandatory = $false)][String]$CapturePath = "C:\BeerTemp\DHCPPacketOutput\",
    [Parameter(Mandatory = $false)][String]$InterfaceId = "5",
    [Parameter(Mandatory = $false)][int]$Duration = 600
)

Set-Location -Path $ExecutionPath
Import-Module .\check-outgoing-dhcp-packets.psm1

Read-OutgoingDHCPPackets -TsharkPath $TsharkPath -CapturePath $CapturePath -InterfaceId $InterfaceId -Duration $Duration  
