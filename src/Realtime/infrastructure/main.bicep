targetScope = 'subscription'

param location string = deployment().location
param projectName string = 'memeit'
param serviceName string = 'realtime'
param environmentName string
param containerImage string
param containerPort int = 8080
param allowedCorsOrigin string = 'https://localhost:4200'

// Landingzone configuration
param landingzoneServiceName string = 'landingzone'
param landingzone object = {
  resourceGroupName: '${projectName}-${landingzoneServiceName}-${environmentName}-${substring(location, 0, 3)}-rg'
  containerAppsEnvironmentName: '${projectName}-${landingzoneServiceName}-${environmentName}-${substring(location, 0, 3)}-env'
  managedIdentityName: '${projectName}-${landingzoneServiceName}-${environmentName}-${substring(location, 0, 3)}-id'
  containerRegistryPullIdentityName: '${projectName}-${landingzoneServiceName}-${environmentName}-${substring(location, 0, 3)}-acr-pull-id'
  appInsightsName: '${projectName}-${landingzoneServiceName}-${environmentName}-${substring(location, 0, 3)}-ai'
}

var defaultResourceName = '${projectName}-${serviceName}-${environmentName}-${substring(location, 0, 3)}'
var resourceGroupName = toLower('${defaultResourceName}-rg')

// Reference landingzone resources
resource landingzoneResourceGroup 'Microsoft.Resources/resourceGroups@2025-04-01' existing = {
  name: landingzone.resourceGroupName
}

module landingzoneResources 'landingzone-refs.bicep' = {
  scope: landingzoneResourceGroup
  name: 'landingzone-resources'
  params: {
    containerAppsEnvironmentName: landingzone.containerAppsEnvironmentName
    managedIdentityName: landingzone.managedIdentityName
    containerRegistryPullIdentityName: landingzone.containerRegistryPullIdentityName
    appInsightsName: landingzone.appInsightsName
  }
}

resource resourceGroup 'Microsoft.Resources/resourceGroups@2025-04-01' = {
  name: resourceGroupName
  location: location
  tags: {
    project: projectName
    service: serviceName
    environment: environmentName
  }
}

module realtimeService 'service.bicep' = {
  scope: resourceGroup
  name: 'realtime-service-deployment'
  params: {
    defaultResourceName: defaultResourceName
    location: location
    containerAppsEnvironmentId: landingzoneResources.outputs.containerAppsEnvironmentId
    managedIdentityId: landingzoneResources.outputs.managedIdentityId
    containerRegistryPullIdentityId: landingzoneResources.outputs.containerRegistryPullIdentityId
    appInsightsConnectionString: landingzoneResources.outputs.appInsightsConnectionString
    containerImage: containerImage
    containerPort: containerPort
    allowedCorsOrigin: allowedCorsOrigin
  }
}

output containerAppName string = realtimeService.outputs.containerAppName
output containerAppFqdn string = realtimeService.outputs.containerAppFqdn
output resourceGroupName string = resourceGroupName
