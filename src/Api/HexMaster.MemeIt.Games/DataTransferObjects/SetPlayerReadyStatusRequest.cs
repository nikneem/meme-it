namespace HexMaster.MemeIt.Games.DataTransferObjects;

public record SetPlayerReadyStatusRequest(string PlayerId, string GameCode, bool IsReady);
