using System.Collections.Generic;
namespace HexMaster.MemeIt.Games.DataTransferObjects;

public record UpdateSettingsRequest(string PlayerId, string GameCode, Dictionary<string, string> Settings);
