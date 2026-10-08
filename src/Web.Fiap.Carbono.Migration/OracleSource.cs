using System.Data;
using Microsoft.EntityFrameworkCore;
using Web.Fiap.Carbono.Data.Contexts;
using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Migration;

// In-memory source snapshot is intentional for the small academic dataset.
public sealed record OracleSource
{
    public List<EmpresaModel> Empresas { get; init; } = [];
    public List<ProdutoModel> Produtos { get; init; } = [];
    public List<FornecedorModel> Fornecedores { get; init; } = [];
    public List<FatorEmissaoModel> Fatores { get; init; } = [];
    public List<LoteProducaoModel> Lotes { get; init; } = [];
    public List<EtapaCadeiaModel> Etapas { get; init; } = [];
    public List<EmissaoCarbonoModel> Emissoes { get; init; } = [];

    public static async Task<OracleSource> ReadAsync(
        string connectionString, CancellationToken token)
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseOracle(connectionString)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .EnableSensitiveDataLogging(false).Options;
        await using var context = new DatabaseContext(options);
        // One consistent Oracle snapshot for all seven tables, without DML.
        await using var transaction = await context.Database
            .BeginTransactionAsync(IsolationLevel.Serializable, token);
        var source = new OracleSource
        {
            Empresas = await context.Empresas.OrderBy(x => x.IdEmpresa).ToListAsync(token),
            Produtos = await context.Produtos.OrderBy(x => x.IdProduto).ToListAsync(token),
            Fornecedores = await context.Fornecedores.OrderBy(x => x.IdFornecedor).ToListAsync(token),
            Fatores = await context.FatoresEmissao.OrderBy(x => x.IdFator).ToListAsync(token),
            Lotes = await context.LotesProducao.OrderBy(x => x.IdLote).ToListAsync(token),
            Etapas = await context.EtapasCadeia.OrderBy(x => x.IdEtapa).ToListAsync(token),
            Emissoes = await context.EmissoesCarbono.OrderBy(x => x.IdEmissao).ToListAsync(token)
        };
        await transaction.RollbackAsync(token);
        return source;
    }
}
