using HexMaster.MemeIt.Memes.Models;

namespace HexMaster.MemeIt.Memes.Abstractions;

public interface IMemeTemplateRepository
{
    Task<MemeTemplate> CreateAsync(MemeTemplate memeTemplate, CancellationToken cancellationToken = default);
    Task<MemeTemplate?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IEnumerable<MemeTemplate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MemeTemplate> UpdateAsync(MemeTemplate memeTemplate, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
