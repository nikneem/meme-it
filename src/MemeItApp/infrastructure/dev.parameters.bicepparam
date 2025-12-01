using './main.bicep'

param location = 'westeurope'
param environmentName = 'dev'
param systemName = 'spreavw-frontend'
param hostnames = [
  'dev.spreaview.com'
  'beta.spreaview.com'
]
