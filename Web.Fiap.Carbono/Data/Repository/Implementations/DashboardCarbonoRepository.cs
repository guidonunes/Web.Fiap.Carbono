using Microsoft.EntityFrameworkCore;
using Web.Fiap.Carbono.Data.Contexts;
using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Data.Repository.Implementations;

public class DashboardCarbonoRepository: IDashboardCarbonoRepository
{
    private readonly DatabaseContext _context;

    public DashboardCarbonoRepository(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<EmpresaModel?> GetEmpresaComDadosCarbonoAsync(int idEmpresa)
    {
        return await _context.Empresas
            .AsNoTracking()
            .Include(e => e.Produtos)
            .ThenInclude(p => p.LotesProducao)
            .ThenInclude(l => l.EtapasCadeia)
            .ThenInclude(et => et.EmissoesCarbono)
            .ThenInclude(ec => ec.FatorEmissao)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.IdEmpresa == idEmpresa);
    }
}