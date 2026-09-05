using Web.Fiap.Carbono.Migration;
using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Tests.Migration;

internal static class MigrationTestData
{
    internal static DateTime Date => new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    internal static MigrationPolicy Policy => new()
    {
        SourceId = "synthetic-test-source", OracleTimeZone = "UTC", MigrationTimestampUtc = Date,
        CompanyActiveDefault = true, SupplierCreatedAtUtc = Date, AcceptReconstructedFactorSnapshots = true,
        Factors = new() { [2] = new FactorPolicy { Category = "ENERGIA", Version = 1,
            ValidFromUtc = Date, DecisionNote = "Synthetic test fixture, not real Oracle metadata." } }
    };

    internal static OracleSource Source(int emissionCount = 1) => new()
    {
        Empresas = [new EmpresaModel { IdEmpresa = 1, NomeEmpresa = "Empresa teste", Cnpj = "12.345.678/0001-90", DataCadastro = Date }],
        Produtos = [new ProdutoModel { IdProduto = 1, IdEmpresa = 1, NomeProduto = "Produto teste", TipoCategoria = "TESTE", UnidadeMedida = "kg", DataCadastro = Date }],
        Fornecedores = [new FornecedorModel { IdFornecedor = 1, NomeFornecedor = "Fornecedor teste", Cnpj = "98765432000190", TipoFornecedor = "TRANSPORTE" }],
        Fatores = [new FatorEmissaoModel { IdFator = 2, Fonte = "Energia", ValorFatorCo2e = 0.0817m, UnidadeBase = "kWh", Escopo = "ESCOPO_2", DataCadastro = Date }],
        Lotes = [new LoteProducaoModel { IdLote = 1, IdProduto = 1, CodigoLote = "LOTE-1", Quantidade = 100m, DataProducao = Date, DataValidade = Date.AddYears(1) }],
        Etapas = [new EtapaCadeiaModel { IdEtapa = 1, IdLote = 1, IdFornecedor = 1, TipoEtapa = "TRANSPORTE", OrdemEtapa = "1", DataInicio = Date }],
        Emissoes = Enumerable.Range(41, emissionCount).Select(id => new EmissaoCarbonoModel
        {
            IdEmissao = id, IdEtapa = 1, IdFator = 2, QuantidadeAtividade = 1500m,
            QuantidadeEmitida = 122.55m, Unidade = "kgCO2e", MetodoCalculo = "QuantidadeAtividade * ValorFatorCo2e",
            DataRegistro = Date.AddMonths(1)
        }).ToList()
    };
}
