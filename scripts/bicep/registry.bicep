@minLength(5)
@maxLength(50)
@description('Provide a globally unique name of your Azure Container Registry')
param acrName string = 'acr${uniqueString(resourceGroup().id)}'

@description('Provide a location for the registry.')
param location string = resourceGroup().location

@description('Provide a tier of your Azure Container Registry.')
param acrSku string = 'Basic'

param resourceTags object = {
  Description: 'Private container registry for ACA demo for conf 24'
   Environment: 'Demo'
   ResourceType: 'Registry'
}

resource acrResource 'Microsoft.ContainerRegistry/registries@2021-06-01-preview' = {
  name: acrName
  location: location
  tags: resourceTags
  sku: {
    name: acrSku
  }
  properties: {
    adminUserEnabled: true
  }
}

@description('Output the login server and registry name')
output loginServer string = acrResource.properties.loginServer
output loginName string = acrResource.name
