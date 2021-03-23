param(
    [Parameter(Mandatory = $true)][string]$NewCertThumbprint = "3EE8E3EF760F4AB4A6068A235E876642223E4532",
    [Parameter(Mandatory = $false)][string]$OldCertThumbprint = "",
    [Parameter(Mandatory = $false)][string]$RootPath = "C:\ESDB\"
)

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

Write-Host $NewCertThumbprint -ForegroundCol Green
Write-Host $OldCertThumbprint -ForegroundColor Yellow 

#get certificate
$certificate = Get-Item -Path (Join-Path -Path "Cert:\LocalMachine\WebHosting\" -ChildPath $NewCertThumbprint)

#get certifcate chain to export root certificate

$chain = New-Object -TypeName System.Security.Cryptography.X509Certificates.X509Chain 
$chain.Build($certificate)

#get the root certifcate

$rootCertifcate = $chain.ChainElements[$chain.ChainElements.Count - 1].Certificate

#export the root certificate
$rootCertifcateFilePath = Join-Path -Path $RootPath -ChildPath "TrustedRootCertificates" 
if ( (Test-Path $rootCertifcateFilePath) -eq $false) {
    New-Item $rootCertifcateFilePath -ItemType Directory
}

Export-Certificate -Cert $rootCertifcate -FilePath (Join-Path -Path $rootCertifcateFilePath -ChildPath "root.crt")
$certificate | Move-Item -Destination "Cert:\LocalMachine\My"

$pathToConfigFile = Join-Path -Path $RootPath -ChildPath "eventstore.conf"
$existingConfigrationContent = Get-Content $pathToConfigFile

$replaceString = '$1' + "`"$NewCertThumbprint`""
$existingConfigrationContent = $existingConfigrationContent -replace '(CertificateThumbPrint: )(")(.*?)(")', $replaceString
Set-Content -Path $pathToConfigFile -Value $existingConfigrationContent

if ([String]::IsNullOrEmpty($OldCertThumbprint) -eq $false) {
    try {
        Stop-Service -Name "ESDB"
        Start-Service -Name "ESDB"
    }
    catch {
        
    }

}