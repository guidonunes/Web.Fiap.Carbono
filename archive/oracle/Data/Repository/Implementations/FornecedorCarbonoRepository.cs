using Microsoft.EntityFrameworkCore;
using Web.Fiap.Carbono.Data.Contexts;
using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Data.Repository.Implementations;

public class FornecedorCarbonoRepository: IFornecedorCarbonoRepository
{
    private readonly DatabaseContext _context;

    public FornecedorCarbonoRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<FornecedorModel>> GetFornecedoresComEmissoesPagedAsync(int pageNumber, int pageSize)
    {
        return await _context.Fornecedores
            .AsNoTracking()
            .Include(f => f.EtapasCadeia)
            .ThenInclude(e => e.EmissoesCarbono)
            .Where(f => f.EtapasCadeia.Any(e => e.EmissoesCarbono.Any()))
            .OrderBy(f => f.NomeFornecedor)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<int> CountFornecedoresComEmissoesAsync()
    {
        return await _context.Fornecedores
            .Where(f => f.EtapasCadeia.Any(e => e.EmissoesCarbono.Any()))
            .CountAsync();
    }
}