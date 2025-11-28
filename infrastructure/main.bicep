targetScope = 'subscription'

param location string = deployment().location
param projectName string
param serviceName string
param environmentName string

var defaultResourceName = '${projectName}-${serviceName}-${environmentName}-${substring(location, 0, 3)}'
var resourceGroupName = toLower('${defaultResourceName}-rg')

resource resourceGroup 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: resourceGroupName
  location: location
  tags: {
    project: projectName
    environment: environmentName
  }
}

module resources 'resources.bicep' = {
  scope: resourceGroup
  name: 'resources-deployment'
  params: {
    defaultResourceName: defaultResourceName
    location: location
  }
}

output appInsightsConnectionString string = resources.outputs.appInsightsConnectionString
output appInsightsInstrumentationKey string = resources.outputs.appInsightsInstrumentationKey
output containerAppsEnvironmentName string = resources.outputs.containerAppsEnvironmentName
output serviceBusNamespace string = resources.outputs.serviceBusNamespace
output managedIdentityClientId string = resources.outputs.managedIdentityClientId
