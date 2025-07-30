namespace HexMaster.MemeIt.Core.DataTransferObjects;

public class GameUpdateMessage
{
    public string Type { get; set; } = string.Empty;
    public object Data { get; set; } = new();
    public string GameCode { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public static class GameUpdateMessageTypes
{
    public const string GameUpdated = "GameUpdated";
    public const string PlayerJoined = "PlayerJoined";
    public const string PlayerLeft = "PlayerLeft";
    public const string PlayerReadyStatusChanged = "PlayerReadyStatusChanged";
    public const string PlayerKicked = "PlayerKicked";
    public const string GameStarted = "GameStarted";
    public const string GameSettingsUpdated = "GameSettingsUpdated";
}
