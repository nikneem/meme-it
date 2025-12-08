param defaultResourceName string
param location string
param containerAppsEnvironmentId string
param managedIdentityId string
param containerRegistryPullIdentityId string
param appInsightsConnectionString string
param containerImage string
param containerPort int
param postgresAdminUsername string
@secure()
param postgresAdminPassword string
@secure()
param managementApiKey string
param allowedCorsOrigins array

// PostgreSQL Flexible Server
resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-03-01-preview' = {
  name: '${defaultResourceName}-pg'
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: postgresAdminUsername
    administratorLoginPassword: postgresAdminPassword
    version: '16'
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

// Allow Azure services to access PostgreSQL
resource postgresFirewallRule 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-03-01-preview' = {
  parent: postgresServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// Create database
resource postgresDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-03-01-preview' = {
  parent: postgresServer
  name: 'memesdb'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

// Storage Account for blob storage
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: replace('${defaultResourceName}st', '-', '')
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: true
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

// Blob Service
resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    cors: {
      corsRules: [
        {
          allowedOrigins: allowedCorsOrigins
          allowedMethods: [
            'GET'
            'PUT'
            'POST'
          ]
          allowedHeaders: [
            '*'
          ]
          exposedHeaders: [
            '*'
          ]
          maxAgeInSeconds: 3600
        }
      ]
    }
  }
}

// Upload Container - Private access (files are temporary and not meant to be publicly accessible)
resource uploadContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'upload'
  properties: {
    publicAccess: 'None'
  }
}

// Memes Container - Public blob access (finalized meme templates need to be publicly readable)
resource memesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'memes'
  properties: {
    publicAccess: 'Blob'
  }
}

// Grant managed identity access to storage
resource storageBlobDataContributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, managedIdentityId, 'StorageBlobDataContributor')
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
    ) // Storage Blob Data Contributor
    principalId: reference(managedIdentityId, '2023-01-31').principalId
    principalType: 'ServicePrincipal'
  }
}

// Grant managed identity Storage Blob Delegator role for SAS token generation
resource storageBlobDelegatorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, managedIdentityId, 'StorageBlobDelegator')
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      'db58b8e5-c6ad-4a2a-8342-4190687cbf4a'
    ) // Storage Blob Delegator
    principalId: reference(managedIdentityId, '2023-01-31').principalId
    principalType: 'ServicePrincipal'
  }
}

// Container App
resource memesContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
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
        corsPolicy: {
          allowedOrigins: allowedCorsOrigins
          allowedMethods: [
            'GET'
            'POST'
            'PUT'
            'DELETE'
            'OPTIONS'
          ]
          allowedHeaders: [
            '*'
          ]
          exposeHeaders: [
            '*'
          ]
          maxAge: 3600
          allowCredentials: true
        }
      }
      dapr: {
        enabled: true
        appId: 'memes-api'
        appProtocol: 'http'
        appPort: containerPort
        enableApiLogging: true
      }
    }
    template: {
      initContainers: [
        {
          name: 'db-migration'
          image: containerImage
          command: [
            'dotnet'
            'HexMaster.MemeIt.Memes.Api.dll'
            '--migrate'
          ]
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: 'Production'
            }
            {
              name: 'ConnectionStrings__memes-db'
              value: 'Host=${postgresServer.properties.fullyQualifiedDomainName};Database=memesdb;Username=${postgresAdminUsername};Password=${postgresAdminPassword};SSL Mode=Require'
            }
          ]
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
        }
      ]
      containers: [
        {
          name: 'memes-api'
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
              name: 'ConnectionStrings__memes-db'
              value: 'Host=${postgresServer.properties.fullyQualifiedDomainName};Database=memesdb;Username=${postgresAdminUsername};Password=${postgresAdminPassword};SSL Mode=Require'
            }
            {
              name: 'ConnectionStrings__memes-blobs'
              value: 'https://${storageAccount.name}.blob.${environment().suffixes.storage}'
            }
            {
              name: 'Azure__Storage__AccountName'
              value: storageAccount.name
            }
            {
              name: 'BlobStorage__UploadContainerName'
              value: uploadContainer.name
            }
            {
              name: 'BlobStorage__MemesContainerName'
              value: memesContainer.name
            }
            {
              name: 'Management__ApiKey'
              value: managementApiKey
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: reference(managedIdentityId, '2023-01-31').clientId
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
    storageBlobDataContributorRole
  ]
}

output containerAppName string = memesContainerApp.name
output containerAppFqdn string = memesContainerApp.properties.configuration.ingress.fqdn
output containerAppId string = memesContainerApp.id
output postgresServerName string = postgresServer.name
output postgresServerFqdn string = postgresServer.properties.fullyQualifiedDomainName
output storageAccountName string = storageAccount.name
output uploadContainerName string = uploadContainer.name
output memesContainerName string = memesContainer.name
