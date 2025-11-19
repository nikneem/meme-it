namespace HexMaster.MemeIt.Games.Abstractions.Application.Commands;

/// <summary>
/// Marker interface describing a request that flows through the Games command pipeline.
/// </summary>
/// <typeparam name="TResponse">Type produced when the command completes.</typeparam>
public interface ICommand<TResponse>
{
}
