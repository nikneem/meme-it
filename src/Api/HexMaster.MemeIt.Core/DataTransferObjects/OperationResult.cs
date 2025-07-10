namespace HexMaster.MemeIt.Core.DataTransferObjects;

public record OperationResult<TResponse>(bool Success, TResponse? ResponseObject);