targetScope = 'subscription'

param location string = deployment().location
param projectName string
param environmentName string

var defaultResourceName = '${projectName}-${environmentName}-${substring(location, 0, 3)}'
var resourceGroupName = toLower('${defaultResourceName}-rg')

resource resourceGroup 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: resourceGroupName
  location: location
  tags: {
    project: projectName
    environment: environmentName
  }
}
