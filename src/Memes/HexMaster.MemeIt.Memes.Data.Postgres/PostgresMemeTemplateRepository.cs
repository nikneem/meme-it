using HexMaster.MemeIt.Memes.Repositories;
using HexMaster.MemeIt.Memes.Domains;
using Microsoft.EntityFrameworkCore;

namespace HexMaster.MemeIt.Memes.Data.Postgres;

/// <summary>
/// PostgreSQL implementation of the meme template repository.
/// </summary>
public class PostgresMemeTemplateRepository : IMemeTemplateRepository
{
    private readonly MemesDbContext _context;

    public PostgresMemeTemplateRepository(MemesDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<MemeTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MemeTemplates
            .Include(m => m.TextAreas)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<MemeTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.MemeTemplates
            .Include(m => m.TextAreas)
            .OrderBy(m => m.Title)
            .ToListAsync(cancellationToken);
    }

    public async Task<MemeTemplate?> GetRandomAsync(CancellationToken cancellationToken = default)
    {
        var count = await _context.MemeTemplates.CountAsync(cancellationToken);

        if (count == 0)
            return null;

        var skip = Random.Shared.Next(0, count);

        return await _context.MemeTemplates
            .Include(m => m.TextAreas)
            .Skip(skip)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Guid> AddAsync(MemeTemplate template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);

        await _context.MemeTemplates.AddAsync(template, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return template.Id;
    }

    public async Task UpdateAsync(MemeTemplate template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);

        _context.MemeTemplates.Update(template);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var template = await GetByIdAsync(id, cancellationToken);
        if (template is not null)
        {
            _context.MemeTemplates.Remove(template);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MemeTemplates
            .AnyAsync(m => m.Id == id, cancellationToken);
    }
}
