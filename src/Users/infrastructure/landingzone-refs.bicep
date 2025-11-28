// This module references existing landingzone resources
// It runs in the scope of the landingzone resource group

param containerAppsEnvironmentName string
param managedIdentityName string
param appInsightsName string
param containerRegistryPullIdentityName string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppsEnvironmentName
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: managedIdentityName
}

resource containerRegistryPullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: containerRegistryPullIdentityName
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: appInsightsName
}

output containerAppsEnvironmentId string = containerAppsEnvironment.id
output managedIdentityId string = managedIdentity.id
output containerRegistryPullIdentityId string = containerRegistryPullIdentity.id
output appInsightsConnectionString string = appInsights.properties.ConnectionString
