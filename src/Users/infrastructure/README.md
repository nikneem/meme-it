# Users Service Infrastructure

This directory contains the Azure infrastructure deployment files for the Users service.

## Overview

The Users service is deployed as an Azure Container App that uses the shared Container Apps Environment from the landingzone infrastructure.

## Architecture

```
┌─────────────────────────────────────────────────────┐
│  Landingzone Resource Group                        │
│  (memeit-dev-nor-rg)                               │
│                                                     │
│  ├─ Container Apps Environment                     │
│  ├─ Application Insights                           │
│  ├─ Managed Identity                               │
│  ├─ Service Bus                                    │
│  └─ Dapr Components                                │
└─────────────────────────────────────────────────────┘
                       ▲
                       │ references
                       │
┌─────────────────────────────────────────────────────┐
│  Users Service Resource Group                      │
│  (memeit-users-dev-nor-rg)                         │
│                                                     │
│  └─ Container App (users-api)                      │
│     ├─ Uses: Shared Environment                    │
│     ├─ Identity: Shared Managed Identity           │
│     ├─ Dapr: Enabled (app-id: users-api)           │
│     └─ Telemetry: Shared App Insights              │
└─────────────────────────────────────────────────────┘
```

## Prerequisites

1. **Landingzone deployed**: The main infrastructure must be deployed first
   ```powershell
   cd ../../../infrastructure
   .\deploy.ps1
   ```

2. **Container image built**: Build and push the container image
   ```powershell
   # Build the image
   dotnet publish ../HexMaster.MemeIt.Users.Api -c Release /t:PublishContainer
   
   # Tag and push to your registry
   docker tag memeit/users-api:latest <your-registry>/memeit/users-api:latest
   docker push <your-registry>/memeit/users-api:latest
   ```

3. **Azure CLI authenticated**
   ```bash
   az login
   az account set --subscription <your-subscription-id>
   ```

## Deployment

### Quick Deploy

```powershell
cd src/Users/infrastructure
.\deploy.ps1
```

### Deploy with Custom Image

```powershell
.\deploy.ps1 -ContainerImage "myregistry.azurecr.io/memeit/users-api:v1.0.0"
```

### What-If Analysis

```powershell
.\deploy.ps1 -WhatIf
```

## Resources Deployed

The deployment creates:
- ✅ **Resource Group**: `memeit-users-dev-nor-rg`
- ✅ **Container App**: `memeit-users-dev-nor-ca`
  - CPU: 0.25 cores
  - Memory: 0.5 GB
  - Min replicas: 1
  - Max replicas: 10
  - Auto-scaling: HTTP (100 concurrent requests)

## Configuration

### Environment Variables

The container app is configured with:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_HTTP_PORTS=8080`
- `APPLICATIONINSIGHTS_CONNECTION_STRING` (from landingzone)

### Dapr Configuration

- **App ID**: `users-api`
- **Protocol**: HTTP
- **Port**: 8080
- **API Logging**: Enabled
- **Pub/Sub**: Access to `chatservice-pubsub` (Service Bus)
- **State Store**: Access to `memeit-statestore` (if configured)

### Ingress

- **External**: Yes (publicly accessible)
- **Target Port**: 8080
- **Transport**: HTTP
- **HTTPS**: Enforced (HTTP redirects to HTTPS)

## Shared Resources

The Users service references these resources from the landingzone:

| Resource | Purpose | Location |
|----------|---------|----------|
| Container Apps Environment | Runtime environment | `memeit-dev-nor-rg` |
| Managed Identity | Authentication | `memeit-dev-nor-rg` |
| Application Insights | Telemetry & monitoring | `memeit-dev-nor-rg` |
| Service Bus | Pub/sub messaging via Dapr | `memeit-dev-nor-rg` |

## Monitoring

### View Logs

```bash
az containerapp logs show \
  --name memeit-users-dev-nor-ca \
  --resource-group memeit-users-dev-nor-rg \
  --follow
```

### View Metrics

```bash
az containerapp show \
  --name memeit-users-dev-nor-ca \
  --resource-group memeit-users-dev-nor-rg
```

### Application Insights

Access logs and telemetry in the shared Application Insights resource:
```bash
az monitor app-insights component show \
  --app memeit-dev-nor-ai \
  --resource-group memeit-dev-nor-rg
```

## Scaling

### Manual Scaling

```bash
az containerapp update \
  --name memeit-users-dev-nor-ca \
  --resource-group memeit-users-dev-nor-rg \
  --min-replicas 2 \
  --max-replicas 20
```

### View Current Scale

```bash
az containerapp revision list \
  --name memeit-users-dev-nor-ca \
  --resource-group memeit-users-dev-nor-rg \
  --query "[].{name:name, replicas:properties.replicas, active:properties.active}"
```

## Updating the Service

### Deploy New Version

```powershell
# Build and push new image
dotnet publish ../HexMaster.MemeIt.Users.Api -c Release /t:PublishContainer

# Deploy with new image
.\deploy.ps1 -ContainerImage "myregistry.azurecr.io/memeit/users-api:v1.1.0"
```

### Rollback

```bash
# List revisions
az containerapp revision list \
  --name memeit-users-dev-nor-ca \
  --resource-group memeit-users-dev-nor-rg

# Activate previous revision
az containerapp revision activate \
  --name memeit-users-dev-nor-ca \
  --resource-group memeit-users-dev-nor-rg \
  --revision <previous-revision-name>
```

## Troubleshooting

### Service Not Starting

1. Check logs:
   ```bash
   az containerapp logs show --name memeit-users-dev-nor-ca --resource-group memeit-users-dev-nor-rg --tail 100
   ```

2. Check if image exists:
   ```bash
   docker pull <your-image>
   ```

3. Verify environment exists:
   ```bash
   az containerapp env show --name memeit-dev-nor-cae --resource-group memeit-dev-nor-rg
   ```

### Cannot Access Dapr Resources

Ensure the managed identity has proper RBAC roles:
```bash
az role assignment list --assignee <managed-identity-principal-id>
```

### High Memory/CPU Usage

Check metrics and adjust resources:
```bash
az containerapp update \
  --name memeit-users-dev-nor-ca \
  --resource-group memeit-users-dev-nor-rg \
  --cpu 0.5 \
  --memory 1.0Gi
```

## Clean Up

### Delete Service (Keep Landingzone)

```bash
az group delete --name memeit-users-dev-nor-rg --yes --no-wait
```

### Delete Everything (Including Landingzone)

```bash
# Delete service
az group delete --name memeit-users-dev-nor-rg --yes --no-wait

# Delete landingzone
az group delete --name memeit-dev-nor-rg --yes --no-wait
```

## Cost Optimization

- **Dev/Test**: Use min replicas = 0 (scale to zero when idle)
- **Production**: Use min replicas = 2 for high availability
- **CPU/Memory**: Start small (0.25/0.5) and scale up based on metrics

## References

- [Azure Container Apps Documentation](https://learn.microsoft.com/azure/container-apps/)
- [Dapr on Container Apps](https://learn.microsoft.com/azure/container-apps/dapr-overview)
- [Container Apps Managed Identities](https://learn.microsoft.com/azure/container-apps/managed-identity)
