using Microsoft.EntityFrameworkCore;
using Web.Fiap.Carbono.Data.Contexts;
using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Data.Repository.Implementations;

public class EmissaoCarbonoRepository: IEmissaoCarbonoRepository
{
    private readonly DatabaseContext _context;

    public EmissaoCarbonoRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<EmissaoCarbonoModel>> GetPagedAsync(int pageNumber, int pageSize)
    {
        return await _context.EmissoesCarbono
            .AsNoTracking()
            .Include(e => e.EtapaCadeia)
            .Include(e => e.FatorEmissao)
            .OrderByDescending(e => e.DataRegistro)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _context.EmissoesCarbono.CountAsync();
    }

    public async Task<EmissaoCarbonoModel?> GetByIdAsync(int idEmissao)
    {
        return await _context.EmissoesCarbono
            .AsNoTracking()
            .Include(e => e.EtapaCadeia)
            .Include(e => e.FatorEmissao)
            .FirstOrDefaultAsync(e => e.IdEmissao == idEmissao);
    }
}