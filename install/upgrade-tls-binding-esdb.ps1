param(
    [Parameter(Mandatory = $true)][string]$NewCertThumbprint,
    [Parameter(Mandatory = $false)][string]$OldCertThumbprint = "",
    [Parameter(Mandatory = $false)][string]$RootPath = "C:\ESDB\"
)

Write-Host "Upgrade EventStore TLS Binding Script started..."

$NewCertThumbprint = $NewCertThumbprint.Substring($NewCertThumbprint.IndexOf('=') + 1);
if ([String]::IsNullOrEmpty($OldCertThumbprint) -eq $false) {
    if ($OldCertThumbprint.Contains('=') -eq $true) {
        $OldCertThumbprint = $OldCertThumbprint.Substring($OldCertThumbprint.IndexOf('=') + 1);
    }
}

if($RootPath.Contains('=') -eq $true)
{
    $RootPath = $RootPath.Substring($RootPath.IndexOf('=') + 1);
}

Write-Host "New Certificate Fingerprint:" $NewCertThumbprint -ForegroundCol Green
Write-Host "Old Certificate Fingerprint:" $OldCertThumbprint -ForegroundColor Yellow 

Write-Host "Move the new certificate to the right store and exporting root certificate..."

#get certificate
$certificate = Get-Item -Path (Join-Path -Path "Cert:\LocalMachine\WebHosting\" -ChildPath $NewCertThumbprint)

#get certifcate chain to export root certificate

$chain = New-Object -TypeName System.Security.Cryptography.X509Certificates.X509Chain 
$chain.Build($certificate) *>$null

#get the root certifcate

$rootCertifcate = $chain.ChainElements[$chain.ChainElements.Count - 1].Certificate

#export the root certificate
$rootCertifcateFilePath = Join-Path -Path $RootPath -ChildPath "TrustedRootCertificates" 
if ( (Test-Path $rootCertifcateFilePath) -eq $false) {
    New-Item $rootCertifcateFilePath -ItemType Directory
}

Export-Certificate -Cert $rootCertifcate -FilePath (Join-Path -Path $rootCertifcateFilePath -ChildPath "root.crt") *>$null
Write-Host "Root Certificate exported to file root.crt"
$certificate | Move-Item -Destination "Cert:\LocalMachine\My" 
Write-Host "Certificate moved to new location. Done!"
Write-Host "Updating eventstore.conf file..."
$pathToConfigFile = Join-Path -Path $RootPath -ChildPath "eventstore.conf"
$existingConfigrationContent = Get-Content $pathToConfigFile

$replaceString = '$1' + "`"$NewCertThumbprint`""
$existingConfigrationContent = $existingConfigrationContent -replace '(CertificateThumbPrint: )(")(.*?)(")', $replaceString
Set-Content -Path $pathToConfigFile -Value $existingConfigrationContent
Write-Host "Thumprint in eventstore.conf file replaced. Done!";
Write-Host "Checking if EventStore needs to be restarted"
if ([String]::IsNullOrEmpty($OldCertThumbprint) -eq $false) {
    try {
        Write-Host "Restarting ESDB..."
        Stop-Service -Name "ESDB"
        Start-Service -Name "ESDB"
        Write-Host "Restart of ESDB done!"

        Write-Host "Restarting IIS..."
        Stop-Service -Name "W3SVC"
        Start-Service -Name "W3SVC"
        Write-Host "Restart of IIS done!"

        Write-Host "Starting task to restart appliactions..."

        Start-ScheduledTask -TaskName BeerPing
        Write-Host "Task started. Done!"
    }
    catch {
        Write-Host "Error while restarting service" -ForegroundColor DarkRed
    }

}
else {
    Write-Host "No old certificate found. Assuming service is not running. Nothing to do"
}

Write-Host "Script finished!"