using Azure.Messaging.WebPubSub;
using HexMaster.MemeIt.Core.DataTransferObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HexMaster.MemeIt.Core.Services;

public interface IWebPubSubService
{
    Task<WebPubSubConnectionResponse> GenerateConnectionUrlAsync(string gameCode, string playerId, string? userId = null);
    Task BroadcastToGameAsync(string gameCode, GameUpdateMessage message);
    Task AddPlayerToGameGroupAsync(string gameCode, string playerId);
    Task RemovePlayerFromGameGroupAsync(string gameCode, string playerId);
    Task SendToPlayerAsync(string playerId, GameUpdateMessage message);
}

public class WebPubSubService : IWebPubSubService
{
    private readonly WebPubSubServiceClient _webPubSubClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WebPubSubService> _logger;
    private readonly string _hubName;

    public WebPubSubService(IConfiguration configuration, ILogger<WebPubSubService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _hubName = _configuration["WebPubSub:HubName"] ?? "gameHub";
        
        var connectionString = _configuration.GetConnectionString("WebPubSub") 
            ?? throw new InvalidOperationException("WebPubSub connection string is not configured");
            
        _webPubSubClient = new WebPubSubServiceClient(connectionString, _hubName);
    }

    public async Task<WebPubSubConnectionResponse> GenerateConnectionUrlAsync(string gameCode, string playerId, string? userId = null)
    {
        try
        {
            var groupName = $"game-{gameCode}";
            
            // Generate client access URI with specific permissions
            var clientAccessUri = await _webPubSubClient.GetClientAccessUriAsync(
                DateTimeOffset.UtcNow.AddHours(1),
                userId ?? playerId,
                new[] { $"webpubsub.joinLeaveGroup.{groupName}", $"webpubsub.sendToGroup.{groupName}" }
            );

            return new WebPubSubConnectionResponse
            {
                ConnectionUrl = clientAccessUri.ToString(),
                HubName = _hubName,
                GroupName = groupName,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate Web PubSub connection URL for player {PlayerId} in game {GameCode}", playerId, gameCode);
            
            return new WebPubSubConnectionResponse
            {
                IsSuccess = false,
                ErrorMessage = "Failed to generate connection URL"
            };
        }
    }

    public async Task BroadcastToGameAsync(string gameCode, GameUpdateMessage message)
    {
        try
        {
            var groupName = $"game-{gameCode}";
            var messageJson = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await _webPubSubClient.SendToGroupAsync(groupName, messageJson);
            
            _logger.LogInformation("Broadcasted message of type {MessageType} to game {GameCode}", message.Type, gameCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast message to game {GameCode}", gameCode);
        }
    }

    public async Task AddPlayerToGameGroupAsync(string gameCode, string playerId)
    {
        try
        {
            var groupName = $"game-{gameCode}";
            await _webPubSubClient.AddUserToGroupAsync(groupName, playerId);
            
            _logger.LogInformation("Added player {PlayerId} to game group {GroupName}", playerId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add player {PlayerId} to game group {GameCode}", playerId, gameCode);
        }
    }

    public async Task RemovePlayerFromGameGroupAsync(string gameCode, string playerId)
    {
        try
        {
            var groupName = $"game-{gameCode}";
            await _webPubSubClient.RemoveUserFromGroupAsync(groupName, playerId);
            
            _logger.LogInformation("Removed player {PlayerId} from game group {GroupName}", playerId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove player {PlayerId} from game group {GameCode}", playerId, gameCode);
        }
    }

    public async Task SendToPlayerAsync(string playerId, GameUpdateMessage message)
    {
        try
        {
            var messageJson = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await _webPubSubClient.SendToUserAsync(playerId, messageJson);
            
            _logger.LogInformation("Sent message of type {MessageType} to player {PlayerId}", message.Type, playerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to player {PlayerId}", playerId);
        }
    }
}
