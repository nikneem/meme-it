using HexMaster.MemeIt.Games.ValueObjects;
namespace HexMaster.MemeIt.Games.DataTransferObjects;

public record UpdateSettingsRequest(string PlayerId, string GameCode, GameSettings Settings);
