@description('Specifies the name of the log analytics workspace.')
param laName string = 'LawAca-${uniqueString(resourceGroup().id)}'

@description('Specifies the location for all resources.')
@allowed([
  'WestEurope'
  'EastUs'
  'NorthEurope'  
])
param location string

param resourceTags object = {
  Description: 'Log analytics for ACA demo for Conf24'
   Environment: 'Demo'
   ResourceType: 'Analytics'
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: laName
  location: location  
  tags: resourceTags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}
