# Meme Templates Management API

This module provides a complete implementation for managing meme templates in the MemeIt application.

## Features

- **Create Meme Templates**: Upload new meme templates with metadata and text areas
- **List Meme Templates**: Retrieve all available meme templates
- **Get Meme Template**: Retrieve a specific meme template by ID
- **Update Meme Template**: Update meme template metadata and text areas
- **Delete Meme Template**: Remove meme templates and associated images

## API Endpoints

All endpoints are under the `/management/memes` path:

- `GET /management/memes` - List all meme templates
- `POST /management/memes` - Create a new meme template
- `GET /management/memes/{id}` - Get a specific meme template
- `PUT /management/memes/{id}` - Update a meme template
- `DELETE /management/memes/{id}` - Delete a meme template

## Request/Response Models

### CreateMemeRequest
```json
{
  "name": "Drake Pointing",
  "description": "Drake pointing at preferred option",
  "sourceImage": "image-filename.jpg",
  "sourceWidth": 800,
  "sourceHeight": 600,
  "textareas": [
    {
      "x": 100,
      "y": 50,
      "width": 300,
      "height": 80,
      "fontFamily": "Arial",
      "fontSize": 24,
      "fontColor": "#000000",
      "maxLength": 50
    }
  ]
}
```

### MemeTemplateResponse
```json
{
  "id": "guid",
  "name": "Drake Pointing",
  "description": "Drake pointing at preferred option",
  "sourceImageUrl": "https://storage.blob.core.windows.net/memes/image.jpg",
  "sourceWidth": 800,
  "sourceHeight": 600,
  "textAreas": [...],
  "createdAt": "2025-01-01T00:00:00Z",
  "updatedAt": "2025-01-02T00:00:00Z"
}
```

## Configuration

The application uses .NET Aspire for orchestrating Azure Storage and CosmosDB services. No manual connection strings are required in development as Aspire handles the service discovery and configuration automatically.

### Aspire Resources

The AppHost configures the following resources:

- **Azure Blob Storage**: Uses Azurite emulator in development for blob operations
- **Azure CosmosDB**: Uses CosmosDB emulator in development with persistent data volume
- **Database**: `MemeItDatabase` 
- **Container**: `MemeTemplates` with partition key `/partitionKey`

### Development Environment

When running in development mode, Aspire automatically:
- Starts the Azurite storage emulator
- Starts the CosmosDB emulator with persistent data volume
- Creates the required database and container
- Handles all connection strings and service discovery

### Production Configuration

For production deployment, update the AppHost to remove the `.RunAsEmulator()` calls and configure actual Azure resources.

## Workflow

1. **Upload Process**: 
   - Files are first uploaded to the `upload` container in Azure Blob Storage
   - When creating a meme template, the file is moved from `upload` to `memes` container
   - Metadata is stored in CosmosDB

2. **Storage Structure**:
   - **Blob Storage**: Two containers - `upload` (temporary) and `memes` (permanent)
   - **CosmosDB**: `MemeTemplates` container with partition key `meme-template`

## Dependencies

- Aspire.Azure.Storage.Blobs - For .NET Aspire Azure Blob Storage integration
- Aspire.Microsoft.Azure.Cosmos - For .NET Aspire Azure CosmosDB integration
- Microsoft.AspNetCore.Mvc.Core - For MVC attributes
- Newtonsoft.Json - Required by Cosmos SDK

### AppHost Dependencies

- Aspire.Hosting.Azure.Storage - For Azure Storage hosting support
- Aspire.Hosting.Azure.CosmosDB - For Azure CosmosDB hosting support

## Architecture

The implementation follows CQRS pattern with:
- **Commands**: CreateMemeCommand, UpdateMemeCommand, DeleteMemeCommand
- **Queries**: GetMemeQuery, ListMemesQuery
- **Handlers**: Separate handlers for each command/query
- **Repository**: IMemeTemplateRepository for data access
- **Services**: IBlobStorageService for blob operations
