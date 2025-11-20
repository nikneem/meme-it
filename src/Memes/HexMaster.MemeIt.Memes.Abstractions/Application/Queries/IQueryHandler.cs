using System.Threading;
using System.Threading.Tasks;

namespace HexMaster.MemeIt.Memes.Abstractions.Application.Queries;

/// <summary>
/// Handles a query and returns a result.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
/// <typeparam name="TResult">The result type.</typeparam>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery
{
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
