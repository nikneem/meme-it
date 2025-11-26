param defaultResourceName string

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2025-07-01' = {
  name: '${defaultResourceName}-cae'
  location: resourceGroup().location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: '<your-log-analytics-workspace-id>'
        sharedKey: '<your-log-analytics-shared-key>'
      }
    }
  }
}
