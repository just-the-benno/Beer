
function Write-BeerIntro {

    Write-Host @" 
__________                          .______.                      .___ 
\______   \_______  _________     __| _/\_ |__ _____    ____    __| _/ 
 |    |  _/\_  __ \/  _ \__  \   / __ |  | __ \\__  \  /    \  / __ |  
 |    |   \ |  | \(  <_> ) __ \_/ /_/ |  | \_\ \/ __ \|   |  \/ /_/ |  
 |______  / |__|   \____(____  /\____ |  |___  (____  /___|  /\____ |  
        \/                   \/      \/      \/     \/     \/      \/  
                      .__                                              
  ____   ____    ____ |__| ____   ____   ___________  ______           
_/ __ \ /    \  / ___\|  |/    \_/ __ \_/ __ \_  __ \/  ___/           
\  ___/|   |  \/ /_/  >  |   |  \  ___/\  ___/|  | \/\___ \            
 \___  >___|  /\___  /|__|___|  /\___  >\___  >__|  /____  >           
     \/     \//_____/         \/     \/     \/           \/            
        _____  _____.__                     .__                        
  _____/ ____\/ ____\__| ____ _____    ____ |__| ____  __ __  ______   
_/ __ \   __\\   __\|  |/ ___\\__  \ _/ ___\|  |/  _ \|  |  \/  ___/   
\  ___/|  |   |  |  |  \  \___ / __ \\  \___|  (  <_> )  |  /\___ \    
 \___  >__|   |__|  |__|\___  >____  /\___  >__|\____/|____//____  >   
     \/                     \/     \/     \/                     \/    
                      __           __                                  
_______  ____   ____ |  | __ _____/  |_                                
\_  __ \/  _ \_/ ___\|  |/ // __ \   __\                               
 |  | \(  <_> )  \___|    <\  ___/|  |                                 
 |__|   \____/ \___  >__|_ \\___  >__|                                 
                   \/     \/    \/                                     	

  .   *   ..  . *  *
*  * @()Ooc()*   o  .
    (Q@*0CG*O()  ___
   |\_________/|/ _ \
   |  |  |  |  | / | |     _                        
   |  |  |  |  | | | |    | |__   ___  ___ _ __  
   |  |  |  |  | | | |    | '_ \ / _ \/ _ \ '__]
   |  |  |  |  | | | |    | |_) |  __/  __/ |  
   |  |  |  |  | | | |    |_.__/ \___|\___|_|  
   |  |  |  |  | \_| |
   |  |  |  |  |\___/
   |\_|__|__|_/|
    \_________/
		
	
#############################################################################
#############################################################################
############################################################################# 
"@
}

function Write-Install {
    Write-Host @"
 .___                 __         .__  .__   
 |   | ____   _______/  |______  |  | |  |  
 |   |/    \ /  ___/\   __\__  \ |  | |  |  
 |   |   |  \\___ \  |  |  / __ \|  |_|  |__
 |___|___|  /____  > |__| (____  /____/____/
          \/     \/            \/           
  
 #############################################################################
 #############################################################################
 #############################################################################
"@
    
}

function Find-InstalledChocoPackage {
    param (
        [Parameter(Mandatory = $true)][String]$Name
    ) 

    $result = Invoke-Expression  "choco list -lo" | Where-object { $_.ToLower().StartsWith($Name.ToLower()) }
    if ($null -ne $result -or [String]::IsNullOrEmpty($result) -eq $false) {
        return $true;
    }

    return $false;  
}

function Install-ChocoPackage {
    param (
        [Parameter(Mandatory = $true)][String]$Name,
        [Parameter(Mandatory = $false)][String]$AddtionalParameter = ""
    )

    Write-Host "checking if programm '$Name' is installed"
    $isInstalled = Find-InstalledChocoPackage -Name $Name
    if ($isInstalled -eq $true) {
        Write-Host "Program $Name is installed";
        Write-Host "Applying upgrades..."
    }
    else {
        
        Write-Host "Installting the programm $Name..."
    }

    $command = "choco upgrade ${Name} -y --source=`"'https://chocolatey.org/api/v2'`"";
    if ([String]::IsNullOrEmpty($AddtionalParameter) -eq $false) {
        $command += " --params `"${AddtionalParameter}`"";
    }

    Invoke-Expression $command 
    $isInstalled = Find-InstalledChocoPackage -Name $Name
    Update-Path
    return $isInstalled;
}

function Install-PreChocoPackage {
    param (
        [Parameter(Mandatory = $true)][String]$Name,
        [Parameter(Mandatory = $false)][String]$AddtionalParameter = ""
    )

    $command = "choco upgrade ${Name} --pre -y --source=`"'https://chocolatey.org/api/v2'`"";
    if ([String]::IsNullOrEmpty($AddtionalParameter) -eq $false) {
        $command += " --params `"${AddtionalParameter}`"";
    }

    Invoke-Expression $command 
    Update-Path
    return $true;
}

function Get-AbsolutFilePath {
    param(
        [string]$path
    )
    return [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot -ChildPath $path))

}

function Update-Path {
    $env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User") 
}

function Find-NPMPackageInstalled {
    param (
        [Parameter(Mandatory = $true)][string]$Name
    )
    
    $installedPackages = (Invoke-Expression "npm list -g");
    foreach ($package in $installedPackages) {
        if ( $package.Contains($Name) -eq $true) {
            return $true;
        }
    }

    return $false;
}

function Install-NPMPackageIfNeeded {
    param (
        [Parameter(Mandatory = $true)][string]$Name
    )

    Write-Host "Check if npm package '$Name' is installed"
    if ( (Find-NPMPackageInstalled -Name $Name) -eq $true) {
        Write-Host "npm package $Name is installed"
        return $true;
    }
    else {
        try {
            Write-Host "installing npm package '$Name'"
            Invoke-Expression "npm install $Name --global" *>$null;
            $installResult = (Find-NPMPackageInstalled -Name $Name)
            if ($installResult -eq $true) {
                Write-Host "installing npm package '$Name' succeeded" -ForegroundColor DarkGreen
            }
            else {
                Write-Host "installing npm package '$Name' failed" -ForegroundColor DarkRed
            }

            return $installResult
        }
        catch {
            Write-Host "installing npm package '$Name' failed" -ForegroundColor DarkRed
            return $false;
        }
    }
}


function Install-IIS {

    Write-Host "Checking if IIS server role is installed..."
    $iisInstallResult = Get-WindowsFeature Web-Server

    if ($iisInstallResult.Installed -eq $false) {
        Write-Host "Installing IIS as a server role..."
        $installResult = Install-WindowsFeature -Name Web-Server -IncludeManagementTools
        if ($installResult.Success -eq $true) {
            Write-Host "IIS role successfully installed " -ForegroundColor DarkGreen
            return $true
        }
        else {
            Write-Host "Failed to Install the IIS server role" -ForegroundColor DarkRed
            return $false
        }
    }
    else {
        Write-Host "IIS is installed"
        return $true
    }
}

function Set-HSTSToWebsite {
    param (
        [Parameter(Mandatory = $true)][string]$Name

    )

    Start-IISCommitDelay

    $sitesCollection = Get-IISConfigSection -SectionPath "system.applicationHost/sites" | Get-IISConfigCollection
    $siteElement = Get-IISConfigCollectionElement -ConfigCollection $sitesCollection -ConfigAttribute @{"name" = $Name }
    $hstsElement = Get-IISConfigElement -ConfigElement $siteElement -ChildElementName "hsts"
    Set-IISConfigAttributeValue -ConfigElement $hstsElement -AttributeName "enabled" -AttributeValue $true
    Set-IISConfigAttributeValue -ConfigElement $hstsElement -AttributeName "max-age" -AttributeValue 31536000
    Set-IISConfigAttributeValue -ConfigElement $hstsElement -AttributeName "includeSubDomains" -AttributeValue $false
    Set-IISConfigAttributeValue -ConfigElement $hstsElement -AttributeName "redirectHttpToHttps" -AttributeValue $true

    Stop-IISCommitDelay
 
    
}

function Update-IISSites {
    param (
        $apps
    )

    foreach ($app in $apps) {
        if ($app.IsBuilt -eq $false) {
            Write-Host "Skipping $($app.Name) because it is not built" -ForegroundColor DarkRed
            continue;
        }

        Write-Host "Checking if website '$($app.Name)' exists..."
        $webSiteresult = Get-WebSite -Name $app.Name
        if ($null -eq $webSiteresult) {
            Write-Host "website '$($app.Name)' not existing. No updated needed. Skipping..." 
            continue
        }

        Write-Host "Website '$($app.Name)' found. Stopping it for this update"

        Stop-Website -Name $app.Name
        Write-Host "Website '$($app.Name)' has stopped to updade it"
        Clear-AppFolder($app) 
        Copy-App -App $app -CheckFolder $false
        Update-WebConfig($app)
        Start-Website -Name $app.Name
        Write-Host "Website '$($app.Name)' has been updated and restarted"
    }
}

function Update-WebConfig {
    param (
        [Parameter(Mandatory = $true)]$app
    )

    Write-Host "Setting variables as enviroment variables for the app '$($app.Name)'..."
    $webConfigFilePath = Join-Path $app.OutputDir -ChildPath "web.config"
    [xml]$xmlDoc = Get-Content $webConfigFilePath
    $aspCoreNode = $xmlDoc.SelectSingleNode("/configuration/location/system.webServer/aspNetCore")

    $parentNode = $xmlDoc.CreateElement("environmentVariables")
    foreach ($item in $app.Variables.keys) {
        $node = $xmlDoc.CreateElement("environmentVariable")

        $node.SetAttribute("name", $item)
        $node.SetAttribute("value", $app.Variables.$item)

        $parentNode.AppendChild($node)

    }

    $aspCoreNode.AppendChild($parentNode)

    $xmlDoc.Save($webConfigFilePath);

    Write-Host "Web.config adjusted" -ForegroundColor DarkGreen
}

function Add-IISSites {
    param (
        [Parameter(Mandatory = $true)]$apps,
        [Parameter(Mandatory = $true)][string]$EmailAddress,
        [Parameter(Mandatory = $true)][string]$AzureTenendId,
        [Parameter(Mandatory = $true)][string]$AzureClientId,
        [Parameter(Mandatory = $true)][string]$AzureClientPassword,
        [Parameter(Mandatory = $true)][string]$AzureSubscrionId,
        [Parameter(Mandatory = $true)][string]$AzureResourceGroupName
    )

    foreach ($app in $apps) {
        if ($app.IsBuilt -eq $false) {
            Write-Host "Skipping $($app.Name) because it is not built" -ForegroundColor DarkRed
            continue;
        }

        Write-Host "Checking if website '$($app.Name)' exists..."
        $webSiteresult = Get-WebSite -Name $app.Name
        if ($null -ne $webSiteresult) {
            Write-Host "website '$($app.Name)' exists. Skipping..." 
            continue
        }
        else {
            Write-Host "website '$($app.Name)' not found. Creating it..." 
        }

        Update-WebConfig($app)

        Write-Host "Creating website..."
        New-WebAppPool -Name $app.Name -Force *>$null
        Set-ItemProperty -Path (Join-Path -Path "IIS:\AppPools\" -ChildPath $app.Name) processModel.loadUserProfile $true
        New-WebSite -Name $app.Name -Port 80  -IPAddress "*" -HostHeader $app.Urls[0] -PhysicalPath $app.OutputDir.Replace("/", "\") -ApplicationPool $app.Name *>$null
        $site = Get-WebSite -Name $app.Name
        Write-Host "App $($app.Name) is created and listen to $($app.Urls[0])" -ForegroundColor DarkGreen
    
        for ($i = 1; $i -lt $app.Urls.Count; $i++) {
            New-WebBinding -Name  $app.Name -IPAddress "*" -Port 80 -HostHeader $app.Urls[$i]
            Write-Host "App $($app.Name) is now listing at $($app.Urls[$i]) too"  -ForegroundColor DarkGreen
        } 

        Write-Host "Requesting a certificate from let's Encrypt..."
        $expression = "wacs --accepttos --emailaddress $EmailAddress --friendlyname `"$($app.Name)`"  --usedefaulttaskuser --target iis --siteid $($site.ID) --installation iis  --validationmode dns-01 --validation azure  --azuretenantid $AzureTenendId  --azureclientid $AzureClientId --azuresecret $AzureClientPassword --azuresubscriptionid $AzureSubscrionId --azureresourcegroupname $AzureResourceGroupName"
        Invoke-Expression   $expression *>$null

        Write-Host "Certificate obtained" -ForegroundColor DarkGreen
        Write-Host "Enabling HSTS..."
        Set-HSTSToWebsite -Name $app.Name
        Write-Host "HSTS is enabled" -ForegroundColor DarkGreen
    }
}

function Clear-AppFolder {
    param (
        [Parameter(Mandatory = $true)]$app
    )

    if ( (Test-Path $app.OutputDir -PathType Container) -eq $false) {
        Write-Host "$($app.OutputDir) doens't exists. Skipping this step"
        continue;
    }

    for ($i = 0; $i -le 2; $i++) {
        try {
            Get-ChildItem -Path $app.OutputDir -Recurse  | Remove-Item -Force -Recurse *>$null
            Start-Sleep -Seconds 1.0
        }
        catch {
        }
    }
}

function Copy-App {
    param (
        [Parameter(Mandatory = $true)]$App,
        [Parameter(Mandatory = $false)][bool]$CheckFolder = $true
    )

    if ($app.IsBuilt -eq $false) {
        Write-Host "Skipping $($app.Name) because it is not built" -ForegroundColor DarkRed
        
        continue;
    }

    if ($checkFolder -eq $true) {
        if ( (Test-Path $app.OutputDir -PathType Container) -eq $true) {
            Write-Host "$($app.OutputDir) already exists. Skipping this step"
            continue;
        }
        else {
            New-Item -Path $app.OutputDir -ItemType Directory
        }
    }

    Write-Host "Coping $($app.Name) to output direcoty $($app.OutputDir)..." 
    Copy-Item -Path "$($app.PublishDir)\*" -Destination $app.OutputDir -Recurse
    Write-Host "App $($app.Name) copy to $($app.OutputDir)" -ForegroundColor DarkGreen

}

function Copy-Files {
    param (
        $apps
    )

    foreach ($app in $apps) {
        Copy-App -App $app
    }
}

function Add-Apps {
    param (
        $apps
    )

    foreach ($app in $apps) {
        $projectFilePath = Join-Path -Path  $repoDestDict -ChildPath $app.ProjectFile
        $projectDir = Split-Path -Path $projectFilePath
        if ([System.IO.File]::Exists($projectFilePath) -eq $false) {
            Write-Host "Project file $projectFilePath not found. Skipping..." -ForegroundColor DarkRed
            continue;
        }
    
        $publishDir = Join-Path -Path  $projectDir -ChildPath "publish"
        Write-Host "Building the app: '$($app.Name)'. This can take a few minutes..."
        Invoke-Expression "dotnet publish -c release -o $publishDir $projectFilePath" *>$null
        $app.IsBuilt = $true;
        $app.PublishDir = $publishDir;
        Write-Host "app '$($app.Name)' sucessfully built" -ForegroundColor DarkGreen
    }
}

function Edit-HostsSingleEntry {
    param (
        [Parameter(Mandatory = $true)][String]$Hostname,
        [Parameter(Mandatory = $true)][String]$Address
    )

    $hostsPath = "$env:windir\System32\drivers\etc\hosts"
    $hosts = [System.Collections.ArrayList](Get-Content $hostsPath)

    $entryToRemove = $null;
    foreach ($hostEntry in $hosts) {
        if ($hostEntry.Contains($Hostname) -eq $true) {
            $entryToRemove = $hostEntry
        }
    }

    if ($null -ne $entryToRemove) {
        $hosts.Remove($entryToRemove);
    }

    $hosts.Add("$Address $Hostname") *>$null
    $hosts | Out-File $hostsPath
}

function Edit-Hosts {
    param (
        $apps
    )

    $hostsPath = "$env:windir\System32\drivers\etc\hosts"
    $hosts = [System.Collections.ArrayList](Get-Content $hostsPath)
    
    foreach ($app in $apps) {
        if ($app.IsBuilt -eq $false) {
            Write-Host "Skipping $($app.Name) because it is not built" -ForegroundColor DarkRed
            continue;
        }
        Write-Host "updating host file..."
        foreach ($url in $app.Urls) {
            $ipv4EntryToRemove = $null;
            $ipv6EntryToRemove = $null;

            foreach ($hostEntry in $hosts) {
                if ($hostEntry.Contains("127.0.0.1 $url") -eq $true) {
                    $ipv4EntryToRemove = $hostEntry
                }
                else {
                    if ($hostEntry.Contains("::1 $url") -eq $true) {
                        $ipv6EntryToRemove = $hostEntry;
                    }
                }
            }

            if ($null -ne $ipv4EntryToRemove) {
                $hosts.Remove($ipv4EntryToRemove);
                Write-Host "host entry $ipv4EntryToRemove removed"
            }

            if ($null -ne $ipv6EntryToRemove) {
                $hosts.Remove($ipv6EntryToRemove);
                Write-Host "host entry $ipv6EntryToRemove removed"
            }

            $hosts.Add("127.0.0.1 $url") *>$null
            $hosts.Add("::1 $url") *>$null

            Write-Host "host entry 127.0.0.1 $url added" -ForegroundColor DarkGreen
            Write-Host "host entry ::1 $url added" -ForegroundColor DarkGreen
        }
    }

    $hosts | Out-File $hostsPath
}

function Get-VersionOfDotNetTool {
    param (
        [Parameter(Mandatory = $true)][String]$Name
    )
    $dotnetTools = Invoke-Expression "dotnet tool list --global"
    foreach ($line in $dotnetTools) {
        if ($line.Contains($Name) -eq $true) {
            $elements = $line.Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries);
            return $elements[1];
        }
    }

    return $null;
}

function Install-AzureDNSPluginForWinACME {
    param (
        [Parameter(Mandatory = $true)][String]$PluginUrl,
        [Parameter(Mandatory = $true)][String]$DownloadTempPath
    )
    
    Write-Host "Checking if azure dns plugin for win-acme is installed..."
    $version = Get-VersionOfDotNetTool -Name "win-acme"
    $dllPath = "$env:USERPROFILE\.dotnet\tools\.store\win-acme\$version\win-acme\$version\tools\net5.0\any"
    $dllExists = Test-Path (Join-Path $dllPath -ChildPath "PKISharp.WACS.Plugins.ValidationPlugins.Azure.dll") -PathType Leaf
    if ($dllExists -eq $false) {
        Write-Host "downloading plugin from url $PluginUrl..."
        $installerFilePath = Join-Path $DownloadTempPath  -ChildPath "winacme-azure-dns-plugin.zip"
        Invoke-WebRequest -Uri $PluginUrl -OutFile $installerFilePath
        
        Write-Host "downloading finished. Expanding archive into plugin folder..."
        Expand-Archive $installerFilePath -DestinationPath $dllPath
        Write-Host "azure dns plugin for win-acme successfully installed" -ForegroundColor DarkGreen
    }
    else {
        Write-Host "azure dns plugin for win-acme is already installed"
    }

    return $true;
}

function Find-DotNetTool {
    param (
        [Parameter(Mandatory = $true)][String]$Name
    )
    $dotnetTools = Invoke-Expression "dotnet tool list --global"
    foreach ($line in $dotnetTools) {
        if ($line.Contains($Name) -eq $true) {
            return $true;
        }
    }

    return $false;
}

function Add-DotNetToolIfNeeded {
    param (
        [Parameter(Mandatory = $true)][String]$Name
    )

    Write-Host "Check if .NET tool '$name' is installed"

    if ( (Find-DotNetTool -Name $Name) -eq $true) {
        Write-Host ".NET tool '$name' is already installed"
        return $true;
    }
    else {
        Write-Host "Installing .NET tool '$name'..."
        Invoke-Expression "dotnet tool install $Name --global " *>$null
        if ( (Find-DotNetTool -Name $Name) -eq $true) {
            Write-Host ".NET tool '$name' installed successfully" -ForegroundColor DarkGreen
            return $true
        }
        else {
            Write-Host "Unable to install .NET tool '$name'" -ForegroundColor DarkRed
            return $false
        }
    }
}

function Set-EventStoreOperational {
    param (
        [Parameter(Mandatory = $true)][String]$RootPath,
        [Parameter(Mandatory = $true)][String]$ExternalDnsName,
        [Parameter(Mandatory = $true)][String]$ExternalIp,
        [Parameter(Mandatory = $true)][String]$AdminPassword,
        [Parameter(Mandatory = $true)][string]$EmailAddress,
        [Parameter(Mandatory = $true)][string]$AzureTenendId,
        [Parameter(Mandatory = $true)][string]$AzureClientId,
        [Parameter(Mandatory = $true)][string]$AzureClientPassword,
        [Parameter(Mandatory = $true)][string]$AzureSubscrionId,
        [Parameter(Mandatory = $true)][string]$AzureResourceGroupName
    )

    Write-Host "Preparing EventSource Database for use"
    Write-Host "Check if root directory $RootPath exists"
    if ( (Test-Path $RootPath) -eq $false) {
        New-Item $RootPath -ItemType Directory
        Write-Host "$RootPath not found. Created it";
    }
    else {
        Write-Host "$RootPath exists";
    }

    #check if config file exist
    Write-Host "Check if EventSource Database config file exists"

    $pathToConfigFile = Join-Path -Path $RootPath -ChildPath "eventstore.conf"
    if ( (Test-Path $pathToConfigFile) -eq $false) {
        $newConfigurationContent = Get-Content -Path ".\event-source-starter.conf"
        $newConfigurationContent = $newConfigurationContent.Replace("{CertificateThumbPrint}", "").Replace("{ExternalDnsName}", $ExternalDnsName).Replace("{ExternalIp}", $ExternalIp).Replace("{RootFolder}", $RootPath).Replace("{DoubleEscapedRootFolder}", $RootPath.Replace('\', '\\'));

        Set-Content $pathToConfigFile -Value $newConfigurationContent
        Write-Host "EventSource Database config file created"
    }
    else {
        Write-Host "EventSource Database already exists"
    }

    #check if certificate is requested
    Write-Host "Check if a certificate is already requested"

    $existingConfigrationContent = Get-Content $pathToConfigFile | Out-String
    $requestCertficate = $true;
    if ( ($existingConfigrationContent -match '(CertificateThumbPrint: ")(.*?)(")') -eq $true) {
        $thumbprint = $Matches[2];
        $requestCertficate = [String]::IsNullOrEmpty($thumbprint)
        if ($requestCertficate -eq $true) {
            Write-Host "No certificate found, requesting it"
        }
        else {
            Write-Host "Certificate with Thumbprint $thumbprint found. No need for new certificate"
        }
    }

    if ($requestCertficate -eq $true) {
        $getCertifcateCommand = "wacs  --accepttos --emailaddress $($EmailAddress)  --friendlyname $ExternalDnsName  --usedefaulttaskuser --installation script --script `".\upgrade-tls-binding-esdb.ps1`"  --scriptparameters `"NewCertThumbprint='{CertThumbprint}' OldCertThumbprint='{OldCertThumbprint}' RootPath='$RootPath'`" --store certificatestore   --target manual  --host $externalDnsName  --validationmode dns-01 --validation azure  --azuretenantid $AzureTenendId  --azureclientid $AzureClientId --azuresecret $AzureClientPassword --azuresubscriptionid $AzureSubscrionId --azureresourcegroupname $AzureResourceGroupName"
        Invoke-Expression -Command $getCertifcateCommand *>$null
        Write-Host "certifcate requested"
    }

    #check if service exists
    Write-Host "Check if windows service for EventSource Database exists"

    if ( (Get-Service | Where-Object -Property Name -eq -Value "ESDB").Count -eq 0) {
        $eventstoreBinaryPath = Invoke-Expression -Command "where.exe EventStore.ClusterNode.exe"

        $xmlFilePath = Join-Path -Path (Get-Location) -ChildPath "event-source-windows-service-sample.xml"

        [Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | Out-Null
        $xmlRoot = [System.Xml.Linq.XElement]::Load($xmlFilePath)

        $xmlRoot.Element('executable').SetValue($eventstoreBinaryPath)
        $xmlRoot.Element('arguments').SetValue("--config " + $pathToConfigFile);

        $xmlServiceDescription = $xmlRoot.ToString()

        $serviceConfigurationFilePath = Join-Path -Path $RootPath -ChildPath "serivce-configuration.xml"

        Set-Content $serviceConfigurationFilePath -Value $xmlServiceDescription

        Invoke-Expression -Command "winsw install $serviceConfigurationFilePath"

        Start-Service -Name "ESDB"
        Write-Host "EventSource Database Service installed and running"
    }
    else {
        Write-Host "EventSource Database Service found"
    }

    Write-Host "Updating hostname file..."
    Edit-HostsSingleEntry -Hostname $ExternalDnsName -Address $ExternalIp
    Write-Host "hostname file updated"

    Write-Host "Check if default password needs to be changed"
    $users = @('ops', 'admin');
    foreach ($user in $users) {
        $pass = 'changeit'
        $pair = "$('admin'):$($pass)"

        $encodedCreds = [System.Convert]::ToBase64String([System.Text.Encoding]::ASCII.GetBytes($pair))

        $basicAuthValue = "Basic $encodedCreds"

        $headers = @{
            Authorization = $basicAuthValue
        }

        $response = try { 
            (Invoke-WebRequest -Uri "https://${ExternalDnsName}:2113/users/${user}/command/reset-password" -Headers $headers -ContentType 'application/json' -Body "{`"newPassword`":`"$AdminPassword`"}" -Method 'Post' -ErrorAction Stop).BaseResponse
        }
        catch { 
            $_.Exception.Response 
        } 

        $statusCodeInt = [int]$response.StatusCode
        switch ($statusCodeInt) {
            200 {
                Write-Host "Password for user $user updated"
            }
            401 { 
                Write-Host "Password for user $user already updated"
            }
            Default {
                Write-Host "Received status code $statusCodeInt. Unknown error while updating passwort for user $user"
            }
        }
    }
}

function Install-SelfSignCertificateIfNeeded {
    param (
        [Parameter(Mandatory = $true)][string]$CommenName,
        [Parameter(Mandatory = $false)][string]$IdentityName = "IIS_IUSRS"

    )

    Write-Host "Check if a selfsigned certificate with the name $CommenName exists"
    $existingCertifcate = Get-ChildItem -Path "Cert:\LocalMachine\My" | Where-Object -Property Subject -eq -Value $CommenName;
    if ($existingCertifcate.Count -eq 0) {
        Write-Host "Certifacte with name $CommenName not found. Creating it..."
        $certficate = New-SelfsignedCertificate -KeyExportPolicy Exportable -Subject $CommenName -KeySpec Signature -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -CertStoreLocation "cert:\LocalMachine\My"
        Write-Host "Certifacte with Thumbprint $($certficate.Thumbprint) created" -ForegroundColor DarkGreen

        $CertObj = Get-ChildItem (Join-Path -Path "Cert:\LocalMachine\my" -ChildPath $certficate.Thumbprint )

        $rsaCert = [System.Security.Cryptography.X509Certificates.RSACertificateExtensions]::GetRSAPrivateKey($CertObj)
        $fileName = $rsaCert.key.UniqueName
        $path = "$env:ALLUSERSPROFILE\Microsoft\Crypto\RSA\MachineKeys\$fileName"
        $permissions = Get-Acl -Path $path
        $rule = new-object security.accesscontrol.filesystemaccessrule $IdentityName, "read", allow
        $permissions.AddAccessRule($rule)
        Set-Acl -Path $path -AclObject $permissions

    }
    else {
        Write-Host "Certifacte with name $CommenName found. Nothing to do"
    }
}

function  Install-Beer {
    
    param (
        [string]$configFilePath = "./beer-apps.json",
        [string]$repoUrl = "https://github.com/just-the-benno/Beer.git",
        [string]$repoDestDict = "./../temp",
        [string]$downloadTempPath = "./downloadtemp",
        [string]$azureDNsWinACMEPluginDownloadUrl = "https://github.com/win-acme/win-acme/releases/download/v2.1.15/plugin.validation.dns.azure.v2.1.15.1008.zip",
        [Parameter(Mandatory = $true)][string]$PostgresqlPassword,
        [Parameter(Mandatory = $true)][string]$ESDBExternalName,
        [Parameter(Mandatory = $true)][string]$ESDBAdminPassword,
        [Parameter(Mandatory = $true)][string]$ExternalIpAddress,
        [Parameter(Mandatory = $true)][string]$EmailAddress,
        [Parameter(Mandatory = $true)][string]$AzureTenendId,
        [Parameter(Mandatory = $true)][string]$AzureClientId,
        [Parameter(Mandatory = $true)][string]$AzureClientPassword,
        [Parameter(Mandatory = $true)][string]$AzureSubscrionId,
        [Parameter(Mandatory = $true)][string]$AzureResourceGroupName
    
    )

    Write-BeerIntro
    Write-Install

    $downloadTempPath = Get-AbsolutFilePath($downloadTempPath);
    New-Item -Path  $downloadTempPath -ItemType "directory" *>$null

    $ProgressPreference = 'SilentlyContinue'

    try {
        Write-Host "Check if Chocolatey is installed"
        $chocoVersion = Invoke-Expression "choco --version"
        Write-Host "Chocolatey with version $chocoVersion is installed" -ForegroundColor Green
    }
    catch {
        $currentExecutionPolicy = Get-ExecutionPolicy

        Set-ExecutionPolicy Bypass -Scope Process -Force; 
        [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; 
        Invoke-Expression ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))
        Set-ExecutionPolicy $currentExecutionPolicy -Scope Process -Force; 
    }

    Write-Host @"

##########################################################################
## This script will install the software Beer and all of its components ##
##  If one of the component is not installed, it will be downloaded     ##
##  and installed                                                       ##
## This script will fail, as soon as one component, can't be installed  ##
## If a IIS site with the name is found, no additional steps will be tak##
## Rerunning the script again, won't have any impact                    ##
## The App config is in the file beer-apps.json                         ##
##########################################################################

"@
    try {

        $gitInstallResult = Install-ChocoPackage -Name "git" -AddtionalParameter "/GitAndUnixToolsOnPath /NoGitLfs /SChannel /NoAutoCrlf"
        $nodeInstallResult = Install-ChocoPackage -Name "nodejs" 
        $buildToolsResult = Install-NPMPackageIfNeeded -Name "windows-build-tools"
        $installIISresult = Install-IIS 

        $installHostingResult = Install-ChocoPackage -Name "dotnet-windowshosting" 
        $dotnetInstallResult = Install-ChocoPackage -Name "dotnet-sdk" 

        $installACMEResult = Add-DotNetToolIfNeeded -Name "win-acme"
        $installACMEPluginResult = Install-AzureDNSPluginForWinACME -PluginUrl $azureDNsWinACMEPluginDownloadUrl -DownloadTempPath $downloadTempPath

        $postgreInstallResult = Install-ChocoPackage -Name "postgresql13" -AddtionalParameter "/Password:$PostgresqlPassword" 
        $eventStoreInstallResult = Install-ChocoPackage -Name eventstore-oss
        
        $_ = Install-PreChocoPackage -Name "winsw"

        if ( ($gitInstallResult -and $dotnetInstallResult -and $nodeInstallResult -and $buildToolsResult -and $installIISresult -and $installHostingResult -and $installACMEResult -and $installACMEPluginResult -and $postgreInstallResult -and $eventStoreInstallResult) -eq $false) {
            Write-Host "Installation failed" -ForegroundColor DarkRed
            return;
        }

        Set-EventStoreOperational -RootPath "C:\ESDB2" -AdminPassword $ESDBAdminPassword -ExternalDnsName $ESDBExternalName -ExternalIp $ExternalIpAddress -EmailAddress $EmailAddress -AzureTenendId $AzureTenendId -AzureClientId $AzureClientId  -AzureClientPassword $AzureClientPassword -AzureSubscrionId $AzureSubscrionId -AzureResourceGroupName $AzureResourceGroupName  

        Write-Host @"

##########################################################################
###                   Sytem is prepared for Beer                       ###
##########################################################################

"@


        Write-Host "Beer will be installed based on the configuration file $configFilePath"
        $configFilePath = Get-AbsolutFilePath($configFilePath)
        $repoDestDict = Get-AbsolutFilePath($repoDestDict)

        $apps = Get-Content -Raw -Path $configFilePath | ConvertFrom-Json   

        Write-Host "Configuration:"
        $apps | Format-Table

        foreach ($app in $apps) {
            Add-Member -InputObject $app -MemberType NoteProperty  -Name "PublishDir" -Value ""
            Add-Member -InputObject $app -MemberType NoteProperty -Name "IsBuilt" -Value $false

            $hashtable = @{}

            try {
                $app.Variables.psobject.properties | ForEach-Object { $hashtable[$_.Name] = $_.Value }
                $app.Variables = $hashtable;
            }
            catch {
        
            }
        }

        Write-Host @"

##########################################################################
###                                Steps                               ###
### 1. Cloning the repository                                          ###
### 2. Building the apps                                               ###
### 3. Copy apps to directory                                          ###
### 4. Setting up the IIS site with bindings and certificates          ###
### 5. Updating existing IIS site                                      ###
### 6. Updating Host file                                              ###                                          ###
##########################################################################

"@

        try {
            # Remove-Item $repoDestDict -Recurse -Force    
        }
        catch {
    
        }
        New-Item -ItemType directory -Path $repoDestDict *>$null

        Write-Host "Cloning the repo: $repoUrl ..."
        Invoke-Expression "git clone  $repoUrl $repoDestDict"
        Write-Host "## Step 1: Cloning finished. Building apps is next..."
        Add-Apps($apps)
        Write-Host "## Step 2: Building finished. Updating existing IIS sites is next..." 
        Update-IISSites($apps)
        Write-Host "## Step 3: Updating of existing apps finished. Copy new apps to output directories is next..." 
        Copy-Files($apps)
        Write-Host "## Step 4: Copy files. Setting up new IIS websites" 
        Add-IISSites -apps $apps -EmailAddress $EmailAddress -AzureTenendId $AzureTenendId -AzureClientId $AzureClientId  -AzureClientPassword $AzureClientPassword -AzureSubscrionId $AzureSubscrionId -AzureResourceGroupName $AzureResourceGroupName  
        Write-Host "## Step 5: new IIS sites are ready. Updating Host file" 
        Edit-Hosts($apps)
        Write-Host "## Step 6: updating host file finisehd. Nothing more todo" 

        Write-Host "After publish steps..." 

        $_ = Install-SelfSignCertificateIfNeeded -CommenName "CN=IdentiyServerSignin"
        $_ = Install-SelfSignCertificateIfNeeded -CommenName "CN=IdentiyServerVerification"

        Write-Host @"

        Write-Host @"

##########################################################################
###                    Beer is ready to be used !!!                    ###
##########################################################################

"@ -ForegroundColor DarkGreen


    }
    catch {
        Write-Host "An error occurred:" -ForegroundColor DarkRed
        Write-Host $_  -ForegroundColor DarkRed
    }
    finally {
        Write-Host "Cleaning up files"
        Write-Host "Deleting download directory"
        Remove-Item $downloadTempPath -Recurse -Force
        Write-Host "Deleting repositoty directory"
        # Remove-Item $repoDestDict -Recurse -Force
    }
}

#Install-Beer -PostgresqlPassword "MyPostgresPassword" -ESDBAdminPassword "MyESDBPassword" -ESDBExternalName "esdb.mysubdomain.com" -ExternalIpAddress "10.10.10.10" -EmailAddress "someone@something.com" -AzureTenendId "aaaaaaaa-0000-0000-0000-aaaaaaaaaaaa" -AzureClientId "bbbbbbbb-0000-0000-0000-bbbbbbbbbbbb" -AzureClientPassword "MyAzureClientPassword" -AzureSubscrionId "cccccccc-0000-0000-0000-cccccccccccc" -AzureResourceGroupName "MyResourcesGroupForBeer"
Install-Beer 
