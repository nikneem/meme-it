# Deploy Azure Infrastructure for Meme-It
# This script deploys Log Analytics, Application Insights, and Container Apps Environment

param(
    [Parameter(Mandatory = $false)]
    [string]$ParameterFile = "main.bicepparam",
    
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf
)

$ErrorActionPreference = "Stop"

Write-Host "Deploying Meme-It Infrastructure..." -ForegroundColor Cyan

# Ensure Azure CLI is logged in
$account = az account show 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Not logged into Azure. Please run: az login" -ForegroundColor Red
    exit 1
}

Write-Host "Azure CLI authenticated" -ForegroundColor Green

# Build the deployment command
$deploymentName = "memeit-infra-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
$location = "northeurope"

if ($WhatIf) {
    Write-Host "Running What-If analysis..." -ForegroundColor Yellow
    az deployment sub what-if `
        --name $deploymentName `
        --location $location `
        --template-file "main.bicep" `
        --parameters $ParameterFile
}
else {
    Write-Host "Deploying resources..." -ForegroundColor Yellow
    
    $output = az deployment sub create `
        --name $deploymentName `
        --location $location `
        --template-file "main.bicep" `
        --parameters $ParameterFile `
        --output json | ConvertFrom-Json
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "SUCCESS: Deployment succeeded!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Application Insights Details:" -ForegroundColor Cyan
        Write-Host "   Connection String: $($output.properties.outputs.appInsightsConnectionString.value)" -ForegroundColor White
        Write-Host "   Instrumentation Key: $($output.properties.outputs.appInsightsInstrumentationKey.value)" -ForegroundColor White
        Write-Host ""
        Write-Host "Azure Service Bus (Pub/Sub):" -ForegroundColor Cyan
        Write-Host "   Namespace: $($output.properties.outputs.serviceBusNamespace.value)" -ForegroundColor White
        Write-Host "   Dapr Component: chatservice-pubsub (configured)" -ForegroundColor Green
        Write-Host ""
        Write-Host "Managed Identity:" -ForegroundColor Cyan
        Write-Host "   Client ID: $($output.properties.outputs.managedIdentityClientId.value)" -ForegroundColor White
        Write-Host ""
        Write-Host "Next Steps:" -ForegroundColor Yellow
        Write-Host "   1. Container apps will automatically use Service Bus via Dapr"
        Write-Host "   2. Add managed identity to container app deployments"
        Write-Host "   3. Uncomment Azure Monitor in ServiceDefaults/Extensions.cs"
        Write-Host "   4. No code changes needed - Dapr component name matches!"
        Write-Host ""
    }
    else {
        Write-Host "ERROR: Deployment failed!" -ForegroundColor Red
        exit 1
    }
}
