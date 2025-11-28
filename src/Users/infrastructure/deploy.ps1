# Deploy Users Service to Azure Container Apps
# This script deploys the Users service to an existing Container Apps Environment

param(
    [Parameter(Mandatory = $false)]
    [string]$ParameterFile = "main.bicepparam",
    
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf,

    [Parameter(Mandatory = $false)]
    [string]$ContainerImage = ""
)

$ErrorActionPreference = "Stop"

Write-Host "Deploying Users Service..." -ForegroundColor Cyan

# Ensure Azure CLI is logged in
$account = az account show 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Not logged into Azure. Please run: az login" -ForegroundColor Red
    exit 1
}

Write-Host "Azure CLI authenticated" -ForegroundColor Green

# Build the deployment command
$deploymentName = "memeit-users-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
$location = "northeurope"

if ($WhatIf) {
    Write-Host "Running What-If analysis..." -ForegroundColor Yellow
    if ($ContainerImage) {
        az deployment sub what-if `
            --name $deploymentName `
            --location $location `
            --parameters $ParameterFile `
            --parameters containerImage=$ContainerImage
    }
    else {
        az deployment sub what-if `
            --name $deploymentName `
            --location $location `
            --parameters $ParameterFile
    }
}
else {
    Write-Host "Deploying Users service..." -ForegroundColor Yellow
    Write-Host "  Using landingzone resources (automatic reference)" -ForegroundColor White
    
    if ($ContainerImage) {
        $output = az deployment sub create `
            --name $deploymentName `
            --location $location `
            --parameters $ParameterFile `
            --parameters containerImage=$ContainerImage `
            --output json | ConvertFrom-Json
    }
    else {
        $output = az deployment sub create `
            --name $deploymentName `
            --location $location `
            --parameters $ParameterFile `
            --output json | ConvertFrom-Json
    }
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "SUCCESS: Users service deployed!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Container App Details:" -ForegroundColor Cyan
        Write-Host "   Name: $($output.properties.outputs.containerAppName.value)" -ForegroundColor White
        Write-Host "   FQDN: $($output.properties.outputs.containerAppFqdn.value)" -ForegroundColor White
        Write-Host "   Resource Group: $($output.properties.outputs.resourceGroupName.value)" -ForegroundColor White
        Write-Host ""
        Write-Host "Service Endpoints:" -ForegroundColor Cyan
        Write-Host "   API: https://$($output.properties.outputs.containerAppFqdn.value)" -ForegroundColor White
        Write-Host "   Health: https://$($output.properties.outputs.containerAppFqdn.value)/health" -ForegroundColor White
        Write-Host ""
        Write-Host "Next Steps:" -ForegroundColor Yellow
        Write-Host "   1. Verify the service is running: az containerapp show --name $($output.properties.outputs.containerAppName.value) --resource-group $($output.properties.outputs.resourceGroupName.value)"
        Write-Host "   2. View logs: az containerapp logs show --name $($output.properties.outputs.containerAppName.value) --resource-group $($output.properties.outputs.resourceGroupName.value) --follow"
        Write-Host "   3. Test the API: curl https://$($output.properties.outputs.containerAppFqdn.value)/health"
        Write-Host ""
    }
    else {
        Write-Host "ERROR: Deployment failed!" -ForegroundColor Red
        exit 1
    }
}
