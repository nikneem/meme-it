using System.Text.Json;
using Microsoft.Azure.Cosmos;

// Test the camelCase serialization configuration
var options = new CosmosSerializationOptions
{
    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
};

// Create a test object to verify serialization
var testObject = new 
{
    Name = "Test Template",
    SourceImageUrl = "https://example.com/image.jpg",
    SourceWidth = 800,
    SourceHeight = 600,
    CreatedAt = DateTimeOffset.Now
};

// Serialize using System.Text.Json with camelCase
var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

var json = JsonSerializer.Serialize(testObject, jsonOptions);
Console.WriteLine("CamelCase JSON:");
Console.WriteLine(json);
