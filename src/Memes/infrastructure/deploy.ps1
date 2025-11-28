# Deploy Memes Service to Azure Container Apps

param(
    [Parameter(Mandatory = $false)]
    [string]$ParameterFile = "main.bicepparam",
    
    [Parameter(Mandatory = $false)]
    [switch]$WhatIf,

    [Parameter(Mandatory = $false)]
    [string]$ContainerImage = ""
)

$ErrorActionPreference = "Stop"

Write-Host "Deploying Memes Service..." -ForegroundColor Cyan

# Ensure Azure CLI is logged in
$account = az account show 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Not logged into Azure. Please run: az login" -ForegroundColor Red
    exit 1
}

Write-Host "Azure CLI authenticated" -ForegroundColor Green

$deploymentName = "memeit-memes-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
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
    Write-Host "Deploying Memes service..." -ForegroundColor Yellow
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
        Write-Host "SUCCESS: Memes service deployed!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Container App Details:" -ForegroundColor Cyan
        Write-Host "   Name: $($output.properties.outputs.containerAppName.value)" -ForegroundColor White
        Write-Host "   FQDN: $($output.properties.outputs.containerAppFqdn.value)" -ForegroundColor White
        Write-Host "   Resource Group: $($output.properties.outputs.resourceGroupName.value)" -ForegroundColor White
        Write-Host ""
        Write-Host "PostgreSQL Details:" -ForegroundColor Cyan
        Write-Host "   Server: $($output.properties.outputs.postgresServerName.value)" -ForegroundColor White
        Write-Host ""
        Write-Host "Storage Details:" -ForegroundColor Cyan
        Write-Host "   Storage Account: $($output.properties.outputs.storageAccountName.value)" -ForegroundColor White
        Write-Host "   Blob Container: $($output.properties.outputs.blobContainerName.value)" -ForegroundColor White
        Write-Host ""
        Write-Host "Service Endpoints:" -ForegroundColor Cyan
        Write-Host "   API: https://$($output.properties.outputs.containerAppFqdn.value)" -ForegroundColor White
        Write-Host ""
    }
    else {
        Write-Host "ERROR: Deployment failed!" -ForegroundColor Red
        exit 1
    }
}
