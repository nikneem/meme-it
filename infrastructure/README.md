# Meme-It Infrastructure Deployment

This directory contains the Bicep templates for deploying the Azure infrastructure for Meme-It.

## Resources Deployed

- **Log Analytics Workspace**: Centralized logging for Container Apps and Application Insights
- **Application Insights**: APM and telemetry collection with OpenTelemetry support
- **Azure Service Bus**: Pub/sub messaging for microservices communication via Dapr
- **Managed Identity**: Secure authentication without connection strings
- **Container Apps Environment**: Managed environment with OpenTelemetry agent configured

## Prerequisites

- Azure CLI installed and authenticated (`az login`)
- Bicep CLI (included with Azure CLI 2.20.0+)
- Appropriate Azure subscription permissions

## Quick Start

### 1. Deploy Infrastructure

```powershell
cd infrastructure
.\deploy.ps1
```

### 2. What-If Analysis (Dry Run)

```powershell
.\deploy.ps1 -WhatIf
```

### 3. Custom Parameters

Edit `main.bicepparam` to change:
- Project name
- Environment name (dev/test/prod)
- Azure region

## Deployment Outputs

After deployment, you'll receive:
- **Application Insights Connection String**: Use for `APPLICATIONINSIGHTS_CONNECTION_STRING` env var
- **Instrumentation Key**: Legacy support (connection string is preferred)
- **Service Bus Namespace**: Pre-configured for Dapr pub/sub
- **Managed Identity Client ID**: For container app authentication
## OpenTelemetry Configuration

The Container Apps Environment is pre-configured with:
- ✅ **Traces** → Application Insights
- ✅ **Logs** → Application Insights
- ✅ **Dapr Traces** → Included (pub/sub operations tracked)
- ✅ **Automatic correlation** across microservices

## Dapr Pub/Sub Configuration

The infrastructure automatically configures:
- ✅ **Azure Service Bus** namespace with 8 topics (one per event type)
- ✅ **Dapr component** named `chatservice-pubsub` (matches dev environment)
- ✅ **Managed Identity** authentication (no connection strings needed)
- ✅ **Dead-letter queues** for failed messages
- ✅ **14-day message retention**

**Zero code changes required** - the Dapr component name matches your existing code!

See [PUBSUB.md](./PUBSUB.md) for detailed pub/sub architecture and configuration.
- ✅ **Logs** → Application Insights
- ✅ **Automatic correlation** across microservices

## Application Configuration

### Enable in ServiceDefaults

Uncomment in `src/Aspire/HexMaster.MemeIt.Aspire/HexMaster.MemeIt.Aspire.ServiceDefaults/Extensions.cs`:

```csharp
if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddOpenTelemetry()
       .UseAzureMonitor();
}
```

### Container App Environment Variables

Add to each container app deployment:

```bicep
env: [
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: appInsightsConnectionString
  }
]
```

## Cost Optimization

- **Log Analytics**: ~$2.30/GB after 5GB free tier
- **Application Insights**: Included with Log Analytics (same ingestion cost)
- **Data Retention**: Set to 30 days (configurable)

## Monitoring & Queries

Access Application Insights in Azure Portal to:
- View distributed traces across microservices
- Monitor custom metrics (GamesMetrics, MemesMetrics, UsersMetrics)
- Set up alerts on failures or performance degradation
- Create custom dashboards with KQL queries

## Clean Up

```bash
az group delete --name memeit-dev-weu-rg --yes --no-wait
```
