param defaultResourceName string
param location string
param containerAppsEnvironmentId string
param managedIdentityId string
param containerRegistryPullIdentityId string
param appInsightsConnectionString string
param containerImage string
param containerPort int
@secure()
param jwtSigningKey string

resource usersContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
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
        appId: 'users-api'
        appProtocol: 'http'
        appPort: containerPort
        enableApiLogging: true
      }
    }
    template: {
      containers: [
        {
          name: 'users-api'
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
              name: 'UsersJwt__SigningKey'
              value: jwtSigningKey
            }
            {
              name: 'UsersJwt__Issuer'
              value: 'HexMaster.MemeIt.Users'
            }
            {
              name: 'UsersJwt__Audience'
              value: 'HexMaster.MemeIt.Clients'
            }
            {
              name: 'UsersJwt__ExpiryMinutes'
              value: '1440'
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
}

output containerAppName string = usersContainerApp.name
output containerAppFqdn string = usersContainerApp.properties.configuration.ingress.fqdn
output containerAppId string = usersContainerApp.id
