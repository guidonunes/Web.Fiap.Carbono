using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Web.Fiap.Carbono.Data.Contexts;
using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Tests.Config;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(
            (_, configurationBuilder) =>
            {
                var mongoDbConfiguration =
                    new Dictionary<string, string?>
                    {
                        ["MongoDb:ConnectionString"] =
                            "mongodb://localhost:27017",

                        ["MongoDb:DatabaseName"] =
                            "fiap_carbono_test"
                    };

                configurationBuilder.AddInMemoryCollection(
                    mongoDbConfiguration
                );
            }
        );

        builder.ConfigureServices(services =>
        {
            var databaseName = $"CarbonoTestDb_{Guid.NewGuid()}";

            services.RemoveAll<DbContextOptions<DatabaseContext>>();

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
            });

            using var scope = services.BuildServiceProvider().CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            SeedDatabase(context);
        });
    }

    private static void SeedDatabase(DatabaseContext context)
    {
        if (context.EmissoesCarbono.Any())
        {
            return;
        }

        var empresa = new EmpresaModel
        {
            IdEmpresa = 1,
            NomeEmpresa = "Empresa Teste ESG",
            Cnpj = "00.000.000/0001-00",
            SetorAtuacao = "Tecnologia",
            Cidade = "Sao Paulo",
            Estado = "SP",
            Pais = "Brasil",
            DataCadastro = DateTime.Now
        };

        var produto = new ProdutoModel
        {
            IdProduto = 1,
            IdEmpresa = 1,
            NomeProduto = "Produto Teste Carbono",
            Descricao = "Produto usado nos testes de integração",
            TipoCategoria = "Teste",
            UnidadeMedida = "unidade",
            Ativo = "S",
            DataCadastro = DateTime.Now,
            Empresa = empresa
        };

        var fornecedor = new FornecedorModel
        {
            IdFornecedor = 1,
            NomeFornecedor = "Fornecedor Teste ESG",
            Cnpj = "11.111.111/0001-11",
            TipoFornecedor = "Transporte",
            Cidade = "Sao Paulo",
            Estado = "SP",
            Pais = "Brasil",
            CertificacaoEsg = "ISO 14001",
            Ativo = "S"
        };

        var lote = new LoteProducaoModel
        {
            IdLote = 1,
            IdProduto = 1,
            CodigoLote = "LOTE-TESTE-001",
            Quantidade = 1000,
            DataProducao = DateTime.Now.AddDays(-10),
            DataValidade = DateTime.Now.AddYears(1),
            StatusLote = "PRODUZIDO",
            Produto = produto
        };

        var etapa = new EtapaCadeiaModel
        {
            IdEtapa = 1,
            TipoEtapa = "TRANSPORTE",
            Descricao = "Etapa de transporte para teste",
            Origem = "Sao Paulo",
            Destino = "Campinas",
            DataInicio = DateTime.Now.AddDays(-5),
            DataFim = DateTime.Now.AddDays(-4),
            OrdemEtapa = "1",
            IdFornecedor = 1,
            IdLote = 1,
            Fornecedor = fornecedor,
            LoteProducao = lote
        };

        var fator = new FatorEmissaoModel
        {
            IdFator = 1,
            Fonte = "Diesel",
            Escopo = "ESCOPO_1",
            UnidadeBase = "litro",
            ValorFatorCo2e = 2.68m,
            Referencia = "Fator teste",
            Ativo = "S",
            DataCadastro = DateTime.Now
        };

        var emissao = new EmissaoCarbonoModel
        {
            IdEmissao = 1,
            IdEtapa = 1,
            IdFator = 1,
            FonteEmissao = "Diesel",
            QuantidadeAtividade = 100,
            QuantidadeEmitida = 268,
            Unidade = "kgCO2e",
            MetodoCalculo = "QuantidadeAtividade * ValorFatorCo2e",
            Observacao = "Emissão usada nos testes",
            DataRegistro = DateTime.Now,
            EtapaCadeia = etapa,
            FatorEmissao = fator
        };

        context.Empresas.Add(empresa);
        context.Produtos.Add(produto);
        context.Fornecedores.Add(fornecedor);
        context.LotesProducao.Add(lote);
        context.EtapasCadeia.Add(etapa);
        context.FatoresEmissao.Add(fator);
        context.EmissoesCarbono.Add(emissao);

        context.SaveChanges();
    }
}
