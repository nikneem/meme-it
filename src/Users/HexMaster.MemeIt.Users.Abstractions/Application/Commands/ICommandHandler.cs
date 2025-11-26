namespace HexMaster.MemeIt.Users.Abstractions.Application.Commands;

/// <summary>
/// Contract for handling commands executed by the Users module.
/// </summary>
/// <typeparam name="TCommand">Command type to execute.</typeparam>
/// <typeparam name="TResponse">Response produced after execution.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
