namespace Localizr.Core.Abstractions.Cqrs;

public interface IQueryHandler
{
}

public interface IQueryHandler<in TQuery, TResult> : IQueryHandler
{
    ValueTask<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
}