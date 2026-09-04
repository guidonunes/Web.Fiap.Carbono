using System.Net;
using System.Net.Http.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Dtos.MongoDb.Analytics;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Tests.Config;

namespace Web.Fiap.Carbono.Tests.Controllers;

[Collection(MongoApiCollection.Name)]
public sealed class DashboardCarbonoControllerTest(
    MongoApiFixture fixture
) : IAsyncLifetime
{
    public Task InitializeAsync()
    {
        return fixture.ClearAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task GetResumoEmpresa_ShouldReturnMongoFacets()
    {
        var empresa = Empresa();
        var produtoA = Produto(
            empresa.Id,
            "PROD-DASH-A",
            "Produto principal");
        var produtoB = Produto(
            empresa.Id,
            "PROD-DASH-B",
            "Produto secundário");
        var fornecedorA = Fornecedor(
            "FORN-DASH-A",
            "62000000000101",
            "Fornecedor principal");
        var fornecedorB = Fornecedor(
            "FORN-DASH-B",
            "62000000000102",
            "Fornecedor secundário");

        await fixture.Database
            .GetCollection<EmpresaDocument>(
                MongoDbContext.EmpresasCollectionName)
            .InsertOneAsync(empresa);
        await fixture.Database
            .GetCollection<ProdutoDocument>(
                MongoDbContext.ProdutosCollectionName)
            .InsertManyAsync(new[] { produtoA, produtoB });
        await fixture.Database
            .GetCollection<FornecedorDocument>(
                MongoDbContext.FornecedoresCollectionName)
            .InsertManyAsync(new[] { fornecedorA, fornecedorB });
        await fixture.Database
            .GetCollection<BsonDocument>(
                MongoDbContext.EmissoesCarbonoCollectionName)
            .InsertManyAsync(new[]
            {
                Emissao(
                    empresa.Id,
                    produtoA.Id,
                    fornecedorA.Id,
                    "100.10",
                    Utc(2026, 6, 1),
                    "ESCOPO_1"),
                Emissao(
                    empresa.Id,
                    produtoA.Id,
                    fornecedorA.Id,
                    "25.20",
                    Utc(2026, 7, 1),
                    "ESCOPO_2"),
                Emissao(
                    empresa.Id,
                    produtoB.Id,
                    fornecedorB.Id,
                    "75.30",
                    Utc(2026, 6, 2),
                    "ESCOPO_1")
            });

        using var client = fixture.CreateClient();
        var response = await client.GetAsync(
            $"/api/dashboard-carbono/empresas/{empresa.Id}/resumo");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content
            .ReadFromJsonAsync<DashboardEmpresaMongoResponse>();
        Assert.NotNull(body);
        Assert.Equal(empresa.Id.ToString(), body.IdEmpresa);
        Assert.Equal(empresa.NomeFantasia, body.NomeEmpresa);
        Assert.Equal(200.60m, body.TotalCo2e);
        Assert.Equal(3, body.QuantidadeEmissoes);
        Assert.Equal(2, body.QuantidadeProdutos);
        Assert.Equal(100.30m, body.MediaEmissaoPorProduto);
        Assert.Equal(produtoA.Nome, body.ProdutoMaisEmissor);
        Assert.Equal(
            fornecedorA.NomeFantasia,
            body.FornecedorMaisEmissor);
        Assert.Equal(
            new[] { (2026, 6, 175.40m), (2026, 7, 25.20m) },
            body.EmissoesPorMes.Select(
                item => (item.Ano, item.Mes, item.TotalCo2e)));
        Assert.Equal(
            new[] { ("ESCOPO_1", 175.40m), ("ESCOPO_2", 25.20m) },
            body.EmissoesPorEscopo.Select(
                item => (item.Escopo, item.TotalCo2e)));

        var malformed = await client.GetAsync(
            "/api/dashboard-carbono/empresas/id-invalido/resumo");
        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);

        var missing = await client.GetAsync(
            $"/api/dashboard-carbono/empresas/" +
            $"{ObjectId.GenerateNewId()}/resumo");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    private static EmpresaDocument Empresa()
    {
        return new EmpresaDocument
        {
            Id = ObjectId.GenerateNewId(),
            Codigo = "EMP-DASHBOARD",
            RazaoSocial = "Empresa Dashboard",
            NomeFantasia = "Empresa Dashboard",
            Cnpj = "62000000000100",
            Ativa = true,
            CriadoEm = Utc(2026, 1, 1),
            AtualizadoEm = Utc(2026, 1, 1),
            SchemaVersion = 1
        };
    }

    private static ProdutoDocument Produto(
        ObjectId empresaId,
        string codigo,
        string nome)
    {
        return new ProdutoDocument
        {
            Id = ObjectId.GenerateNewId(),
            EmpresaId = empresaId,
            Codigo = codigo,
            Nome = nome,
            UnidadeFuncional = "unidade",
            Ativo = true,
            CriadoEm = Utc(2026, 1, 1),
            AtualizadoEm = Utc(2026, 1, 1),
            SchemaVersion = 1
        };
    }

    private static FornecedorDocument Fornecedor(
        string codigo,
        string cnpj,
        string nome)
    {
        return new FornecedorDocument
        {
            Id = ObjectId.GenerateNewId(),
            Codigo = codigo,
            RazaoSocial = nome,
            NomeFantasia = nome,
            Cnpj = cnpj,
            Ativo = true,
            CriadoEm = Utc(2026, 1, 1),
            AtualizadoEm = Utc(2026, 1, 1),
            SchemaVersion = 1
        };
    }

    private static BsonDocument Emissao(
        ObjectId empresaId,
        ObjectId produtoId,
        ObjectId fornecedorId,
        string total,
        DateTime dataEmissao,
        string escopo)
    {
        return new BsonDocument
        {
            ["_id"] = ObjectId.GenerateNewId(),
            ["empresaId"] = empresaId,
            ["produtoId"] = produtoId,
            ["fornecedorId"] = fornecedorId,
            ["quantidadeEmitidaKgCO2e"] =
                new BsonDecimal128(Decimal128.Parse(total)),
            ["dataEmissao"] = new BsonDateTime(dataEmissao),
            ["fatorAplicado"] = new BsonDocument(
                "escopo",
                escopo)
        };
    }

    private static DateTime Utc(int year, int month, int day)
    {
        return new DateTime(
            year,
            month,
            day,
            0,
            0,
            0,
            DateTimeKind.Utc);
    }
}
