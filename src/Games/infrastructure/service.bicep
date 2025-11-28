param defaultResourceName string
param location string
param containerAppsEnvironmentId string
param managedIdentityId string
param containerRegistryPullIdentityId string
param appInsightsConnectionString string
param containerImage string
param containerPort int

// Cosmos DB for MongoDB
resource cosmosDbAccount 'Microsoft.DocumentDB/databaseAccounts@2024-05-15' = {
  name: '${defaultResourceName}-cosmos'
  location: location
  kind: 'MongoDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    capabilities: [
      {
        name: 'EnableMongo'
      }
      {
        name: 'EnableServerless'
      }
    ]
    apiProperties: {
      serverVersion: '6.0'
    }
    publicNetworkAccess: 'Enabled'
    enableAutomaticFailover: false
    enableMultipleWriteLocations: false
  }
}

// MongoDB Database
resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/mongodbDatabases@2024-05-15' = {
  parent: cosmosDbAccount
  name: 'gamesdb'
  properties: {
    resource: {
      id: 'gamesdb'
    }
  }
}

// Grant managed identity access to Cosmos DB
resource cosmosDbDataContributorRole 'Microsoft.DocumentDB/databaseAccounts/mongodbRoleDefinitions@2024-05-15' existing = {
  parent: cosmosDbAccount
  name: '00000000-0000-0000-0000-000000000001' // Built-in MongoDB data contributor role
}

resource cosmosDbRoleAssignment 'Microsoft.DocumentDB/databaseAccounts/mongodbRoleAssignments@2024-05-15' = {
  parent: cosmosDbAccount
  name: guid(cosmosDbAccount.id, managedIdentityId, 'MongoDBDataContributor')
  properties: {
    roleDefinitionId: cosmosDbDataContributorRole.id
    principalId: reference(managedIdentityId, '2023-01-31').principalId
    scope: cosmosDbAccount.id
  }
}

resource gamesContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: '${defaultResourceName}-ca'
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentityId}': {}
      '${containerRegistryPullIdentityId}': {}
    }
  }
  properties: {
    environmentId: containerAppsEnvironmentId
    configuration: {
      activeRevisionsMode: 'Single'
      registries: [
        {
          server: 'nvv54gsk4pteu.azurecr.io'
          identity: containerRegistryPullIdentityId
        }
      ]
      ingress: {
        external: true
        targetPort: containerPort
        transport: 'http'
        allowInsecure: false
        traffic: [
          {
            latestRevision: true
            weight: 100
          }
        ]
      }
      dapr: {
        enabled: true
        appId: 'games-api'
        appProtocol: 'http'
        appPort: containerPort
        enableApiLogging: true
      }
    }
    template: {
      containers: [
        {
          name: 'games-api'
          image: containerImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ASPNETCORE_HTTP_PORTS'
              value: string(containerPort)
            }
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsightsConnectionString
            }
            {
              name: 'ConnectionStrings__MongoDb'
              value: cosmosDbAccount.listConnectionStrings().connectionStrings[0].connectionString
            }
            {
              name: 'MongoDB__DatabaseName'
              value: cosmosDatabase.name
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
        rules: [
          {
            name: 'http-scaling'
            http: {
              metadata: {
                concurrentRequests: '100'
              }
            }
          }
        ]
      }
    }
  }
  dependsOn: [
    cosmosDbRoleAssignment
  ]
}

output containerAppName string = gamesContainerApp.name
output containerAppFqdn string = gamesContainerApp.properties.configuration.ingress.fqdn
output containerAppId string = gamesContainerApp.id
output cosmosDbAccountName string = cosmosDbAccount.name
output cosmosDbDatabaseName string = cosmosDatabase.name
