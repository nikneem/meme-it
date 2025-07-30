namespace HexMaster.MemeIt.Core.DataTransferObjects;

public class WebPubSubConnectionResponse
{
    public string ConnectionUrl { get; set; } = string.Empty;
    public string HubName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}
