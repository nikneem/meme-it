namespace HexMaster.MemeIt.Memes.Abstractions.Application.Commands;

/// <summary>
/// Marker interface describing a request that flows through the Memes command pipeline.
/// </summary>
/// <typeparam name="TResponse">Type produced when the command completes.</typeparam>
public interface ICommand<TResponse>
{
}
