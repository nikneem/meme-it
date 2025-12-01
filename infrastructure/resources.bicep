param defaultResourceName string
param location string = resourceGroup().location

// Log Analytics Workspace for Container Apps and Application Insights
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${defaultResourceName}-law'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
}

// Azure Service Bus Namespace for Dapr Pub/Sub
resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-10-01-preview' = {
  name: '${defaultResourceName}-bus'
  location: location
  sku: {
    name: 'Standard'
    tier: 'Standard'
  }
  properties: {
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false // Set to true in production for Managed Identity only
  }
}

// Create topics for each event type
var topics = [
  'playerstatechanged'
  'playerremoved'
  'playerjoined'
  'gamestarted'
  'roundstarted'
  'creativephaseended'
  'scorephasestarted'
  'roundended'
]

resource serviceBusTopics 'Microsoft.ServiceBus/namespaces/topics@2022-10-01-preview' = [
  for topic in topics: {
    parent: serviceBusNamespace
    name: topic
    properties: {
      maxSizeInMegabytes: 1024
      defaultMessageTimeToLive: 'P14D' // 14 days retention
      enableBatchedOperations: true
      supportOrdering: true
    }
  }
]

// Create default subscriptions for each topic
resource serviceBusSubscriptions 'Microsoft.ServiceBus/namespaces/topics/subscriptions@2022-10-01-preview' = [
  for topic in topics: {
    parent: serviceBusTopics[indexOf(topics, topic)]
    name: 'default'
    properties: {
      maxDeliveryCount: 10
      defaultMessageTimeToLive: 'P14D'
      lockDuration: 'PT5M'
      enableBatchedOperations: true
    }
  }
]

// Managed Identity for Container Apps to access Service Bus
resource containerAppsManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${defaultResourceName}-id'
  location: location
}

// Managed Identity for Container Apps to pull images from external registry
resource containerRegistryPullIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${defaultResourceName}-acr-pull-id'
  location: location
}

// Grant Container Apps identity permissions to Service Bus
resource serviceBusDataSenderRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBusNamespace.id, containerAppsManagedIdentity.id, 'ServiceBusDataSender')
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39'
    ) // Azure Service Bus Data Sender
    principalId: containerAppsManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource serviceBusDataReceiverRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(serviceBusNamespace.id, containerAppsManagedIdentity.id, 'ServiceBusDataReceiver')
  scope: serviceBusNamespace
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0'
    ) // Azure Service Bus Data Receiver
    principalId: containerAppsManagedIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

// Application Insights for OpenTelemetry data collection
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${defaultResourceName}-ai'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    IngestionMode: 'LogAnalytics'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

// Container Apps Environment with OpenTelemetry configured
resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2025-10-02-preview' = {
  name: '${defaultResourceName}-env'
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
    openTelemetryConfiguration: {
      destinationsConfiguration: {
        otlpConfigurations: [
          {
            name: 'appInsightsOtlp'
            endpoint: appInsights.properties.ConnectionString
          }
        ]
      }
      tracesConfiguration: {
        destinations: ['appInsightsOtlp']
      }
      logsConfiguration: {
        destinations: ['appInsightsOtlp']
      }
    }
    workloadProfiles: [
      {
        name: 'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

// Dapr Component: Azure Service Bus Pub/Sub
resource daprPubSubComponent 'Microsoft.App/managedEnvironments/daprComponents@2024-03-01' = {
  parent: containerAppsEnvironment
  name: 'chatservice-pubsub'
  properties: {
    componentType: 'pubsub.azure.servicebus.topics'
    version: 'v1'
    metadata: [
      {
        name: 'namespaceName'
        value: '${serviceBusNamespace.name}.servicebus.windows.net'
      }
      {
        name: 'azureClientId'
        value: containerAppsManagedIdentity.properties.clientId
      }
    ]
    scopes: [] // Empty = available to all apps in environment
  }
}

// HTTP Route Configuration for API Gateway
resource httpRouteConfig 'Microsoft.App/managedEnvironments/httpRouteConfigs@2024-10-02-preview' = {
  parent: containerAppsEnvironment
  name: 'api-gateway'
  properties: {
    rules: [
      {
        description: 'Route /users to Users API'
        routes: [
          {
            match: {
              pathSeparatedPrefix: '/users'
            }
            action: {
              prefixRewrite: '/api/users'
            }
          }
        ]
        targets: [
          {
            containerApp: 'memeit-users-dev-nor-ca'
          }
        ]
      }
      {
        description: 'Route /realtime to Realtime API'
        routes: [
          {
            match: {
              pathSeparatedPrefix: '/realtime'
            }
            action: {
              prefixRewrite: '/api/realtime'
            }
          }
        ]
        targets: [
          {
            containerApp: 'memeit-realtime-dev-nor-ca'
          }
        ]
      }
      {
        description: 'Route /games to Games API'
        routes: [
          {
            match: {
              pathSeparatedPrefix: '/games'
            }
            action: {
              prefixRewrite: '/api/games'
            }
          }
        ]
        targets: [
          {
            containerApp: 'memeit-games-dev-nor-ca'
          }
        ]
      }
      {
        description: 'Route /memes to Memes API'
        routes: [
          {
            match: {
              pathSeparatedPrefix: '/memes'
            }
            action: {
              prefixRewrite: '/api/memes'
            }
          }
        ]
        targets: [
          {
            containerApp: 'memeit-memes-dev-nor-ca'
          }
        ]
      }
    ]
  }
}

// Outputs for use in container app deployments
output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output appInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
output containerAppsEnvironmentId string = containerAppsEnvironment.id
output containerAppsEnvironmentName string = containerAppsEnvironment.name
output serviceBusNamespace string = serviceBusNamespace.name
output managedIdentityClientId string = containerAppsManagedIdentity.properties.clientId
output managedIdentityId string = containerAppsManagedIdentity.id
output containerRegistryPullIdentityId string = containerRegistryPullIdentity.id
output containerRegistryPullIdentityClientId string = containerRegistryPullIdentity.properties.clientId
output containerRegistryPullIdentityPrincipalId string = containerRegistryPullIdentity.properties.principalId
output httpRouteConfigFqdn string = httpRouteConfig.properties.fqdn
output httpRouteConfigName string = httpRouteConfig.name
