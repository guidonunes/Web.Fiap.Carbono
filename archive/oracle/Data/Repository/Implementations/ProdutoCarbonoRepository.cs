using Microsoft.EntityFrameworkCore;
using Web.Fiap.Carbono.Data.Contexts;
using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Data.Repository.Implementations;

public class ProdutoCarbonoRepository: IProdutoCarbonoRepository
{
    private readonly DatabaseContext _context;

    public ProdutoCarbonoRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<ProdutoModel?> GetProdutoComEmissoesAsync(int idProduto)
    {
        return await _context.Produtos
            .AsNoTracking()
            .Include(p => p.Empresa)
            .Include(p => p.LotesProducao)
            .ThenInclude(l => l.EtapasCadeia)
            .ThenInclude(e => e.EmissoesCarbono)
            .ThenInclude(ec => ec.FatorEmissao)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.IdProduto == idProduto);
    }
}