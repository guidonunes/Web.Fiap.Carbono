using Microsoft.EntityFrameworkCore;
using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Data.Contexts;

public class DatabaseContext: DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public DbSet<EmpresaModel> Empresas { get; set; }
    public DbSet<ProdutoModel> Produtos { get; set; }
    public DbSet<FornecedorModel> Fornecedores { get; set; }
    public DbSet<LoteProducaoModel> LotesProducao { get; set; }
    public DbSet<EtapaCadeiaModel> EtapasCadeia { get; set; }
    public DbSet<EmissaoCarbonoModel> EmissoesCarbono { get; set; }
    public DbSet<FatorEmissaoModel> FatoresEmissao { get; set; }
}