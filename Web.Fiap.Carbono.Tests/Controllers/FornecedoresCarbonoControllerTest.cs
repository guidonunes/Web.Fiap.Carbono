using System.Net;
using System.Net.Http.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Data.MongoDb.Repositories;
using Web.Fiap.Carbono.Dtos.MongoDb.Analytics;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Tests.Config;

namespace Web.Fiap.Carbono.Tests.Controllers;

[Collection(MongoApiCollection.Name)]
public sealed class FornecedoresCarbonoControllerTest(
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
    public async Task GetRanking_ShouldReturnStableMongoPagination()
    {
        var fornecedorA = Fornecedor(
            "000000000000000000000001",
            "FOR-RANK-A",
            "63000000000101",
            "Fornecedor A");
        var fornecedorB = Fornecedor(
            "000000000000000000000002",
            "FOR-RANK-B",
            "63000000000102",
            "Fornecedor B");
        var fornecedorC = Fornecedor(
            "000000000000000000000003",
            "FOR-RANK-C",
            "63000000000103",
            "Fornecedor C");

        await fixture.Database
            .GetCollection<FornecedorDocument>(
                MongoDbContext.FornecedoresCollectionName)
            .InsertManyAsync(new[]
            {
                fornecedorA,
                fornecedorB,
                fornecedorC
            });

        await fixture.Database
            .GetCollection<BsonDocument>(
                MongoDbContext.EmissoesCarbonoCollectionName)
            .InsertManyAsync(new[]
            {
                Emissao(fornecedorA.Id, "125.30"),
                Emissao(fornecedorA.Id, "75.30"),
                Emissao(fornecedorB.Id, "100.00"),
                Emissao(fornecedorC.Id, "100.00")
            });

        using var client = fixture.CreateClient();

        var firstResponse = await client.GetAsync(
            "/api/fornecedores-carbono/ranking" +
            "?pageNumber=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
        var firstPage = await firstResponse.Content.ReadFromJsonAsync<
            MongoPagedResult<FornecedorRankingMongoResponse>>();
        Assert.NotNull(firstPage);
        Assert.Equal(3, firstPage.TotalItems);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.Equal(1, firstPage.PageNumber);
        Assert.Equal(2, firstPage.PageSize);
        Assert.Equal(
            new[] { "FOR-RANK-A", "FOR-RANK-B" },
            firstPage.Items.Select(item => item.CodigoFornecedor));
        Assert.Equal(200.60m, firstPage.Items[0].TotalCo2e);
        Assert.Equal(2, firstPage.Items[0].QuantidadeEmissoes);
        Assert.Equal(fornecedorA.Id.ToString(),
            firstPage.Items[0].IdFornecedor);
        Assert.Equal(fornecedorA.NomeFantasia,
            firstPage.Items[0].NomeFornecedor);

        var secondResponse = await client.GetAsync(
            "/api/fornecedores-carbono/ranking" +
            "?pageNumber=2&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
        var secondPage = await secondResponse.Content.ReadFromJsonAsync<
            MongoPagedResult<FornecedorRankingMongoResponse>>();
        Assert.NotNull(secondPage);
        var lastSupplier = Assert.Single(secondPage.Items);
        Assert.Equal("FOR-RANK-C", lastSupplier.CodigoFornecedor);
        Assert.Equal(100.00m, lastSupplier.TotalCo2e);

        var invalidNumber = await client.GetAsync(
            "/api/fornecedores-carbono/ranking" +
            "?pageNumber=0&pageSize=10");
        Assert.Equal(HttpStatusCode.BadRequest, invalidNumber.StatusCode);

        var invalidSize = await client.GetAsync(
            "/api/fornecedores-carbono/ranking" +
            "?pageNumber=1&pageSize=51");
        Assert.Equal(HttpStatusCode.BadRequest, invalidSize.StatusCode);
    }

    private static FornecedorDocument Fornecedor(
        string id,
        string codigo,
        string cnpj,
        string nome)
    {
        return new FornecedorDocument
        {
            Id = ObjectId.Parse(id),
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
        ObjectId fornecedorId,
        string total)
    {
        return new BsonDocument
        {
            ["_id"] = ObjectId.GenerateNewId(),
            ["fornecedorId"] = fornecedorId,
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
