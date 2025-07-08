namespace Localizr.Core.Abstractions.Cqrs;

public interface ICommand
{
    Guid CommandId { get; }
}