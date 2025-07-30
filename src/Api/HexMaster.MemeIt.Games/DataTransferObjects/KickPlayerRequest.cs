namespace HexMaster.MemeIt.Games.DataTransferObjects;

public record KickPlayerRequest(string HostPlayerId, string TargetPlayerId, string GameCode);
