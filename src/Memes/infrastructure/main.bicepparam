using './main.bicep'

param environmentName = 'dev'
param location = 'northeurope'
param containerImage = 'nvv54gsk4pteu.azurecr.io/memeit/memes-api:0.0.2'
param containerPort = 8080
param landingzoneServiceName = 'landingzone'
param postgresAdminUsername = 'memeadmin'
param postgresAdminPassword = readEnvironmentVariable('POSTGRES_ADMIN_PASSWORD', 'DefaultP@ssw0rd!')
param managementApiKey = readEnvironmentVariable('MANAGEMENT_API_KEY', 'dev-meme-management-key-2024')
param allowedCorsOrigins = [
  'https://localhost:4200'
  'https://memeit.hexmaster.nl'
]
