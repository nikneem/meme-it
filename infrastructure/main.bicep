targetScope = 'subscription'

param location string = deployment().location
param projectName string
param serviceName string
param environmentName string
param containerRegistrySubscriptionId string = 'c2a162ec-4baf-44f5-a66e-0fb3b8618424'
param containerRegistryResourceGroup string = 'mvp-int-env'
param containerRegistryName string = 'nvv54gsk4pteu'

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

// Reference to external registry resource group
resource externalRegistryResourceGroup 'Microsoft.Resources/resourceGroups@2025-04-01' existing = {
  scope: subscription(containerRegistrySubscriptionId)
  name: containerRegistryResourceGroup
}

// Deploy ACR role assignment to external registry resource group
module acrRoleAssignment 'acr-role-assignment.bicep' = {
  scope: externalRegistryResourceGroup
  name: 'acr-pull-role-assignment'
  params: {
    containerRegistryName: containerRegistryName
    managedIdentityPrincipalId: resources.outputs.containerRegistryPullIdentityPrincipalId
  }
}

output appInsightsConnectionString string = resources.outputs.appInsightsConnectionString
output appInsightsInstrumentationKey string = resources.outputs.appInsightsInstrumentationKey
output containerAppsEnvironmentName string = resources.outputs.containerAppsEnvironmentName
output serviceBusNamespace string = resources.outputs.serviceBusNamespace
output managedIdentityClientId string = resources.outputs.managedIdentityClientId
output httpRouteConfigFqdn string = resources.outputs.httpRouteConfigFqdn
output httpRouteConfigName string = resources.outputs.httpRouteConfigName
