using './main.bicep'

param environmentName = 'dev'
param location = 'northeurope'
param containerImage = 'nvv54gsk4pteu.azurecr.io/memeit/realtime-api:0.0.2'
param containerPort = 8080
param landingzoneServiceName = 'landingzone'
