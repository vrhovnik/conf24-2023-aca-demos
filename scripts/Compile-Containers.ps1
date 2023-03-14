<# 
# SYNOPSIS
# compile containers from folder
#
# DESCRIPTION
# uses Azure Registry Containers to compile containers from folder
# NOTES
# Author      : Bojan Vrhovnik
# GitHub      : https://github.com/vrhovnik
# Version 1.5.1
# SHORT CHANGE DESCRIPTION: added parameters and build script to build all containers in folder with transcript enabled
#>
param(
    [Parameter(Mandatory = $false)]
    $ResourceGroupName = "Conf24RG",
    [Parameter(Mandatory = $false)]
    $RegistryName = "conf24",
    [Parameter(Mandatory = $false)]
    $FolderName = "..\containers",
    [Parameter(Mandatory = $false)]
    $TagName = "latest",
    [Parameter(Mandatory = $false)]
    $SourceFolder = "..\src",
    [Parameter(Mandatory=$false)]
    [switch]$InstallCli
)
$logPath = "$HOME/Downloads/container-build.log"
Start-Transcript -Path $logPath -Force
if ($InstallCli)
{
    Start-Process Install-AzCli.ps1 -NoNewWindow -Wait
}

Write-Output "Reading registry $RegistryName in Azure"
$registry = Get-AzContainerRegistry -ResourceGroupName $ResourceGroupName -Name $RegistryName
Write-Output "Registry $($registry.Name) has been read"

Write-Output "Reading the folder $FolderName"
Get-ChildItem -Path $FolderName | ForEach-Object {
    $imageName = $_.Name.Split('-')[1]
    $dockerFile = $_.FullName
    Write-Output "Building image $imageName with tag $TagName based on $dockerFile"
    $imageNameWithTag = "$($imageName):$TagName"
    Write-Output "Taging image with $imageNameWithTag"
    Write-Information "Call data with AZ cli as we don't have support in Azure PowerShell for this yet."
    # you can install by providing the switch -InstallCli 
    az acr build --registry $registry.Name --image $imageNameWithTag -f $dockerFile $SourceFolder
}
Write-Output "Building images done"
Stop-Transcript
#read it in notepad
Start-Process "notepad" -ArgumentList $logPath 
