# MemeIt.Library - .NET Aspire Integration

This document explains how to use the MemeIt.Library with .NET Aspire and the CosmosDB emulator.

## Overview

The MemeIt.Library has been enhanced to support .NET Aspire orchestration with Azure CosmosDB emulator for local development. This integration provides:

- **CosmosDB Emulator**: Runs locally using Docker container
- **Automatic Database Setup**: Creates databases and containers automatically
- **Service Discovery**: Aspire manages connection strings and service configuration
- **Observability**: Built-in telemetry, logging, and health checks

## Prerequisites

- .NET 9.0 SDK
- Docker Desktop
- Visual Studio 2022 (17.12+) or VS Code with C# Dev Kit

## Project Structure

```
src/
├── Aspire/
│   └── MemeIt.Aspire/
│       ├── MemeIt.Aspire.AppHost/        # Aspire orchestrator
│       └── MemeIt.Aspire.ServiceDefaults/ # Shared configuration
├── Api/
│   ├── MemeIt.Api/                       # Web API project
│   ├── MemeIt.Core/                      # Domain models
│   └── MemeIt.Library/                   # Library implementation
└── Tests/
    └── MemeIt.Library.Tests/             # Unit tests
```

## Getting Started

### 1. Start the Aspire AppHost

Run the Aspire orchestrator to start the CosmosDB emulator and API:

```bash
cd src/Aspire/MemeIt.Aspire/MemeIt.Aspire.AppHost
dotnet run
```

This will:
- Start the CosmosDB emulator container with persistent storage
- Create the `memeit` database with `memes` and `categories` containers
- Start the MemeIt.Api project
- Open the Aspire dashboard in your browser

### 2. Access the Services

- **Aspire Dashboard**: http://localhost:15000 (shows all services and their status)
- **MemeIt API**: http://localhost:5000 (or the port shown in Aspire dashboard)
- **CosmosDB Emulator**: https://localhost:8081/_explorer/index.html (Data Explorer)

### 3. API Endpoints

The API provides the following endpoints:

#### Memes
- `GET /api/memes/random?playerId={id}&categories={cat1,cat2}&excludedMemeIds={id1,id2}` - Get random meme
- `GET /api/memes/{id}` - Get meme by ID
- `GET /api/memes?skip={n}&take={n}` - Get memes with pagination
- `POST /api/memes/{id}/usage?playerId={id}` - Record meme usage

#### Categories
- `GET /api/categories` - Get all active categories
- `GET /api/categories/{id}` - Get category by ID
- `POST /api/categories/validate?categoryIds={cat1,cat2}` - Validate categories

## Configuration

### Aspire Integration

The `AddMemeLibraryWithAspire()` extension method configures the library for Aspire:

```csharp
// In Program.cs
builder.AddAzureCosmosClient("cosmos");
builder.Services.AddMemeLibraryWithAspire();
```

This automatically configures:
- Database name: `memeit`
- Containers: `memes` and `categories`
- Default throughput: 400 RU/s per container
- Partition key: `/partitionKey`

### Traditional Configuration

For non-Aspire deployments, use the traditional configuration:

```csharp
// In Program.cs
builder.Services.AddMemeLibrary(builder.Configuration);
```

With appsettings.json:

```json
{
  "CosmosDb": {
    "ConnectionString": "AccountEndpoint=https://...",
    "DatabaseName": "memeit",
    "MemesContainerName": "memes",
    "CategoriesContainerName": "categories",
    "MaxRetryAttempts": 3,
    "MaxRetryWaitTimeInSeconds": 30,
    "CreateIfNotExists": true,
    "MemesContainerThroughput": 400,
    "CategoriesContainerThroughput": 400
  }
}
```

## Development Workflow

### Local Development with Aspire

1. **Start Services**: Run the AppHost project
2. **View Dashboard**: Monitor services at http://localhost:15000
3. **Debug API**: Set breakpoints in MemeIt.Api project
4. **View Data**: Use CosmosDB Data Explorer to inspect data
5. **Run Tests**: Tests run independently with mocked dependencies

### Adding Test Data

Use the CosmosDB Data Explorer to add sample data:

1. Navigate to https://localhost:8081/_explorer/index.html
2. Select the `memeit` database
3. Add documents to the `memes` and `categories` containers

Example meme document:
```json
{
  "id": "funny-cat-1",
  "partitionKey": "meme",
  "type": "meme",
  "name": "Funny Cat",
  "imageUrl": "https://example.com/cat.jpg",
  "categories": ["humor", "animals"],
  "textAreas": [
    {
      "id": "top",
      "x": 50,
      "y": 10,
      "width": 400,
      "height": 60,
      "fontSize": 32,
      "fontColor": "#FFFFFF",
      "strokeColor": "#000000",
      "strokeWidth": 2,
      "textAlignment": "Center"
    }
  ],
  "isActive": true,
  "usageCount": 0,
  "createdAt": "2025-01-03T00:00:00Z",
  "lastUsedAt": null
}
```

Example category document:
```json
{
  "id": "humor",
  "partitionKey": "category",
  "type": "category",
  "name": "Humor",
  "description": "Funny and humorous memes",
  "displayOrder": 1,
  "isActive": true,
  "createdAt": "2025-01-03T00:00:00Z"
}
```

## Observability

### Logs
View logs in the Aspire dashboard or console output. The library uses structured logging with relevant context.

### Metrics
Aspire automatically collects metrics for:
- HTTP requests
- CosmosDB operations
- Application performance

### Health Checks
The API includes health checks that verify:
- CosmosDB connectivity
- Service availability

## Troubleshooting

### CosmosDB Emulator Issues

1. **Container won't start**: Ensure Docker Desktop is running
2. **Port conflicts**: Check that port 8081 is available
3. **SSL issues**: Accept the emulator's self-signed certificate

### Connection Issues

1. **Check Aspire Dashboard**: Verify all services are running
2. **View Logs**: Check logs in the dashboard for error details
3. **Restart Services**: Stop and restart the AppHost if needed

### Data Issues

1. **Missing Data**: Use Data Explorer to verify containers and documents exist
2. **Permission Errors**: Ensure proper partition key usage in queries
3. **Query Performance**: Check indexing policies in container settings

## Production Deployment

For production deployments:

1. **Replace Emulator**: Use Azure CosmosDB service
2. **Configure Connection**: Update connection strings for production
3. **Scale Throughput**: Adjust RU/s based on workload
4. **Enable Monitoring**: Use Application Insights and Azure Monitor

The library seamlessly switches between emulator and production CosmosDB based on configuration.
