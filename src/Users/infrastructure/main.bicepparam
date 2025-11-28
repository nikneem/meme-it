using './main.bicep'

param environmentName = 'dev'
param location = 'northeurope'
param containerImage = 'nvv54gsk4pteu.azurecr.io/memeit/users-api:0.0.2'
param containerPort = 8080
param landingzoneServiceName = 'landingzone'

// Landingzone configuration (optional override if non-standard naming)
// param landingzone = {
//   resourceGroupName: 'memeit-landingzone-dev-nor-rg'
//   containerAppsEnvironmentName: 'memeit-landingzone-dev-nor-cae'
//   managedIdentityName: 'memeit-landingzone-dev-nor-id'
//   containerRegistryPullIdentityName: 'memeit-landingzone-dev-nor-acr-pull-id'
//   appInsightsName: 'memeit-landingzone-dev-nor-ai'
// }
