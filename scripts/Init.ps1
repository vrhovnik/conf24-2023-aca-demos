<# 
# SYNOPSIS
# init file and installs all neccessary services to be installed in Azure
#
# DESCRIPTION
# installs all neccessary services to be installed in Azure
#  RESOURCE GROUP
#  CONTAINER REGISTRY
#  LOG ANALYTICS 
#  SQL database
# NOTES
# Author      : Bojan Vrhovnik
# GitHub      : https://github.com/vrhovnik
# Version 1.3.0
# SHORT CHANGE DESCRIPTION: adding location as a parameter and bicep install for sql
#>
param(
    [Parameter(Mandatory=$false)]
    $Location="WestEurope",
    [Parameter(Mandatory=$false)]
    [switch]$InstallModules,
    [Parameter(Mandatory=$false)]
    [switch]$InstallBicep
)

Start-Transcript -Path "$HOME/Downloads/bootstrapper.log" -Force

Write-Output "Sign in to Azure account." 
# login to Azure account
Connect-AzAccount

if ($InstallModules)
{
    Write-Output "Install Az module and register providers."
    #install Az module
    Install-Module -Name Az -Scope CurrentUser -Repository PSGallery -Force
    Install-Module -Name Az.App

    #register providers
    Register-AzResourceProvider -ProviderNamespace Microsoft.App
    # add support for log analytics
    Register-AzResourceProvider -ProviderNamespace Microsoft.OperationalInsights
    Write-Output "Modules installed and registered, continuing to Azure deployment nad if selected, Bicep install."
}

if ($InstallBicep) {
    # install bicep
    Write-Output "Installing Bicep."
    # & Install-Bicep.ps1
    Start-Process powershell.exe -FilePath Install-Bicep.ps1 -NoNewWindow -Wait
    Write-Output "Bicep installed, continuing to Azure deployment."
}

# create resource group if it doesn't exist with bicep file stored in bicep folder
$groupNameExport = New-AzSubscriptionDeployment -Location "WestEurope" -TemplateFile ".\bicep\rg.bicep" -TemplateParameterFile ".\bicep\reg.parameters.json" -Verbose
Write-Information $groupNameExport
$groupName = $groupNameExport.Outputs.rgName.Value

#deploy registry file if not already deployed
New-AzResourceGroupDeployment -ResourceGroupName $groupName -TemplateFile ".\bicep\registry.bicep" -TemplateParameterFile ".\bicep\registry.parameters.json" -Verbose  
#deploy log analytics file if not already deployed
New-AzResourceGroupDeployment -ResourceGroupName $groupName -TemplateFile ".\bicep\log-analytics.bicep" -TemplateParameterFile ".\bicep\log-analytics.parameters.json" -Verbose
# deploy sql server and database
New-AzResourceGroupDeployment -ResourceGroupName $groupName -TemplateFile ".\bicep\sql.bicep" -TemplateParameterFile ".\bicep\sql.parameters.json" -Verbose

Stop-Transcript

# open file for viewing
Start-Process notepad.exe -ArgumentList "$HOME/Downloads/bootstrapper.log"