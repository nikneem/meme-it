using System.Threading;
using System.Threading.Tasks;

namespace HexMaster.MemeIt.Games.Abstractions.Application.Commands;

/// <summary>
/// Contract for processing strongly-typed commands within the Games module.
/// </summary>
/// <typeparam name="TCommand">Command type being executed.</typeparam>
/// <typeparam name="TResponse">Response produced by the command handler.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Executes the requested command.
    /// </summary>
    /// <param name="command">Command payload.</param>
    /// <param name="cancellationToken">Cancellation notification token.</param>
    /// <returns>Command response.</returns>
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
