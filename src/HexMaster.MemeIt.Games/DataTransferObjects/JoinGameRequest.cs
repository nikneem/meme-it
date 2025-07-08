namespace HexMaster.MemeIt.Games.DataTransferObjects;

public record JoinGameRequest(string GameCode, string PlayerName, string? Password);