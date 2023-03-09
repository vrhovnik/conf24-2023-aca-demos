targetScope = 'subscription'
@description('Resource Group Name')
param resourceGroupName string = 'Conf24RG'

@description('Resource Group Location')
param resourceGroupLocation string = 'WestEurope'

param resourceTags object = {
  Description: 'Resource group for ACA demo for conf 24'
  Environment: 'Demo'
  ResourceType: 'ResourceGroup'
}

// Creating resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName 
  tags: resourceTags 
  location: resourceGroupLocation
}

@description('Output resource group name')
output rgName string = resourceGroupName