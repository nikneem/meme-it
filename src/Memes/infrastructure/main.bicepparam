using './main.bicep'

param environmentName = 'dev'
param location = 'northeurope'
param containerImage = 'nvv54gsk4pteu.azurecr.io/memeit/memes-api:0.0.2'
param containerPort = 8080
param landingzoneServiceName = 'landingzone'
param postgresAdminUsername = 'memeadmin'
param postgresAdminPassword = readEnvironmentVariable('POSTGRES_ADMIN_PASSWORD', 'DefaultP@ssw0rd!')
param allowedCorsOrigin = 'https://memeit.hexmaster.nl'
