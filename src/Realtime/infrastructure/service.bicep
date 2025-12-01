param defaultResourceName string
param location string
param containerAppsEnvironmentId string
param managedIdentityId string
param containerRegistryPullIdentityId string
param appInsightsConnectionString string
param containerImage string
param containerPort int
param allowedCorsOrigin string

resource realtimeContainerApp 'Microsoft.App/containerApps@2024-03-01' = {
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
          allowedOrigins: [
            allowedCorsOrigin
          ]
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
        appId: 'realtime-api'
        appProtocol: 'http'
        appPort: containerPort
        enableApiLogging: true
      }
    }
    template: {
      containers: [
        {
          name: 'realtime-api'
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

output containerAppName string = realtimeContainerApp.name
output containerAppFqdn string = realtimeContainerApp.properties.configuration.ingress.fqdn
output containerAppId string = realtimeContainerApp.id
