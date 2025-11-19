namespace HexMaster.MemeIt.Users.Abstractions.Application.Commands;

/// <summary>
/// Marker interface to describe a command within the Users module.
/// </summary>
/// <typeparam name="TResponse">Response type produced by the command handler.</typeparam>
public interface ICommand<TResponse>
{
}
