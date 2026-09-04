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
public sealed class ProdutosCarbonoControllerTest(
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
    public async Task GetPegadaCarbono_ShouldReturnMongoAggregation()
    {
        var empresa = new EmpresaDocument
        {
            Id = ObjectId.GenerateNewId(),
            Codigo = "EMP-ANALYTICS-PRODUCT",
            RazaoSocial = "Empresa Analytics",
            NomeFantasia = "Empresa Analytics",
            Cnpj = "61000000000101",
            Ativa = true,
            CriadoEm = Utc(2026, 1, 1),
            AtualizadoEm = Utc(2026, 1, 1),
            SchemaVersion = 1
        };
        var produto = new ProdutoDocument
        {
            Id = ObjectId.GenerateNewId(),
            EmpresaId = empresa.Id,
            Codigo = "PROD-ANALYTICS",
            Nome = "Produto Analytics",
            UnidadeFuncional = "unidade",
            Ativo = true,
            CriadoEm = Utc(2026, 1, 1),
            AtualizadoEm = Utc(2026, 1, 1),
            SchemaVersion = 1
        };

        await fixture.Database
            .GetCollection<EmpresaDocument>(
                MongoDbContext.EmpresasCollectionName)
            .InsertOneAsync(empresa);
        await fixture.Database
            .GetCollection<ProdutoDocument>(
                MongoDbContext.ProdutosCollectionName)
            .InsertOneAsync(produto);
        await fixture.Database
            .GetCollection<BsonDocument>(
                MongoDbContext.EmissoesCarbonoCollectionName)
            .InsertManyAsync(new[]
            {
                Emissao(produto.Id, "TRANSPORTE", "100.10"),
                Emissao(produto.Id, "ENERGIA", "25.20"),
                Emissao(produto.Id, "TRANSPORTE", "75.30")
            });

        using var client = fixture.CreateClient();
        var response = await client.GetAsync(
            $"/api/produtos-carbono/{produto.Id}/pegada");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content
            .ReadFromJsonAsync<ProdutoPegadaMongoResponse>();
        Assert.NotNull(body);
        Assert.Equal(produto.Id.ToString(), body.IdProduto);
        Assert.Equal(produto.Nome, body.NomeProduto);
        Assert.Equal(empresa.NomeFantasia, body.NomeEmpresa);
        Assert.Equal(200.60m, body.TotalCo2e);
        Assert.Collection(
            body.EmissoesPorEtapa,
            energia =>
            {
                Assert.Equal("ENERGIA", energia.TipoEtapa);
                Assert.Equal(25.20m, energia.TotalCo2e);
            },
            transporte =>
            {
                Assert.Equal("TRANSPORTE", transporte.TipoEtapa);
                Assert.Equal(175.40m, transporte.TotalCo2e);
            });

        var malformed = await client.GetAsync(
            "/api/produtos-carbono/id-invalido/pegada");
        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);

        var missing = await client.GetAsync(
            $"/api/produtos-carbono/{ObjectId.GenerateNewId()}/pegada");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    private static BsonDocument Emissao(
        ObjectId produtoId,
        string categoria,
        string total)
    {
        return new BsonDocument
        {
            ["_id"] = ObjectId.GenerateNewId(),
            ["produtoId"] = produtoId,
            ["etapa"] = new BsonDocument(
                "categoria",
                categoria),
            ["quantidadeEmitidaKgCO2e"] =
                new BsonDecimal128(Decimal128.Parse(total))
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
