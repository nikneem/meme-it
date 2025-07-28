namespace HexMaster.MemeIt.Memes.DataTransferObjects;

public record UpdateMemeRequest(
    string Name,
    string Description,
    MemeTextArea[] TextAreas);
