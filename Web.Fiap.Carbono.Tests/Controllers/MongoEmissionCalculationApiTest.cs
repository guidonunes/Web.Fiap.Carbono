using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Tests.Config;
using Web.Fiap.Carbono.ViewModel;
using Web.Fiap.Carbono.ViewModel.MongoDb;

namespace Web.Fiap.Carbono.Tests.Controllers;

[Collection(MongoApiCollection.Name)]
public sealed class MongoEmissionCalculationApiTest(
    MongoApiFixture fixture)
{
    [Fact]
    public async Task Calculate_WithAnalystToken_ReturnsCreatedAndResolvableLocation()
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(activeFactor: true);
        using var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(references));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content
            .ReadFromJsonAsync<EmissaoCarbonoMongoResponseViewModel>();

        Assert.NotNull(body);
        Assert.Equal(122.5500m, body.QuantidadeEmitidaKgCO2e);
        Assert.Equal("analista@carbono.com", body.CalculadoPor);
        Assert.Equal(0.0817m, body.FatorAplicado.Valor);
        Assert.Equal(references.CompanyId.ToString(), body.EmpresaId);
        Assert.Equal(DateTimeKind.Utc, body.DataEmissao.Kind);
        Assert.Equal(body.DataEmissao, body.CriadoEm);
        Assert.Equal(body.DataEmissao, body.AtualizadoEm);

        var expectedLocation =
            $"/api/emissoes-carbono/mongodb/{body.Id}";
        Assert.Equal(
            expectedLocation,
            response.Headers.Location.AbsolutePath);

        var readResponse = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, readResponse.StatusCode);

        var persistedCount = await fixture.Database
            .GetCollection<EmissaoCarbonoDocument>(
                MongoDbContext.EmissoesCarbonoCollectionName)
            .CountDocumentsAsync(document => document.Id == ObjectId.Parse(body.Id));

        Assert.Equal(1, persistedCount);
    }

    [Fact]
    public async Task Calculate_WithoutAuthentication_ReturnsUnauthorized()
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(activeFactor: true);
        using var client = fixture.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(references));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Calculate_WithInactiveFactor_ReturnsUnprocessableEntityAndPersistsNothing()
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(activeFactor: false);
        using var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(references));

        await AssertErrorAsync(
            response,
            HttpStatusCode.UnprocessableEntity,
            "O fator de emissão informado está inativo.");

        await AssertNoEmissionPersistedAsync();
    }

    [Fact]
    public async Task Calculate_WithExpiredFactor_ReturnsUnprocessableEntityAndPersistsNothing()
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(
            activeFactor: true,
            expiredFactor: true);
        using var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(references));

        await AssertErrorAsync(
            response,
            HttpStatusCode.UnprocessableEntity,
            "O fator de emissão expirou");

        await AssertNoEmissionPersistedAsync();
    }

    [Fact]
    public async Task Calculate_WithIncompatibleUnit_ReturnsUnprocessableEntityAndPersistsNothing()
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(activeFactor: true);
        using var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(
                references,
                activityUnit: "litro"));

        await AssertErrorAsync(
            response,
            HttpStatusCode.UnprocessableEntity,
            "não é compatível com a unidade base 'kWh'");

        await AssertNoEmissionPersistedAsync();
    }

    private async Task AssertNoEmissionPersistedAsync()
    {
        var emissionCount = await fixture.Database
            .GetCollection<EmissaoCarbonoDocument>(
                MongoDbContext.EmissoesCarbonoCollectionName)
            .CountDocumentsAsync(FilterDefinition<EmissaoCarbonoDocument>.Empty);

        Assert.Equal(0, emissionCount);
    }

    [Fact]
    public async Task Calculate_WithoutOptionalValues_PassesValidatorAndOmitsNullFields()
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(
            activeFactor: true,
            includeOptionalFactorFields: false);
        using var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(
                references,
                includeOptionalEmissionFields: false));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content
            .ReadFromJsonAsync<EmissaoCarbonoMongoResponseViewModel>();
        Assert.NotNull(body);

        var persisted = await fixture.Database
            .GetCollection<BsonDocument>(
                MongoDbContext.EmissoesCarbonoCollectionName)
            .Find(Builders<BsonDocument>.Filter.Eq(
                "_id",
                ObjectId.Parse(body.Id)))
            .SingleAsync();

        Assert.False(persisted["etapa"].AsBsonDocument.Contains("local"));
        Assert.False(persisted["fatorAplicado"].AsBsonDocument
            .Contains("fonteReferencia"));
        Assert.False(persisted["fatorAplicado"].AsBsonDocument
            .Contains("metodologia"));
        Assert.False(persisted.Contains("fonteEmissao"));
        Assert.False(persisted.Contains("observacao"));
        Assert.False(persisted.Contains("revisadoEm"));
        Assert.False(persisted.Contains("revisadoPor"));
    }

    private async Task<ReferenceIds> SeedReferencesAsync(
        bool activeFactor,
        bool includeOptionalFactorFields = true,
        bool expiredFactor = false)
    {
        var now = DateTime.UtcNow;
        var companyId = ObjectId.GenerateNewId();
        var productId = ObjectId.GenerateNewId();
        var supplierId = ObjectId.GenerateNewId();
        var factorId = ObjectId.GenerateNewId();

        await fixture.Database
            .GetCollection<EmpresaDocument>(
                MongoDbContext.EmpresasCollectionName)
            .InsertOneAsync(new EmpresaDocument
            {
                Id = companyId,
                Codigo = "EMP-CALCULO-API",
                RazaoSocial = "Empresa cálculo API",
                Cnpj = "61000000000101",
                Ativa = true,
                CriadoEm = now,
                AtualizadoEm = now,
                SchemaVersion = 1
            });

        await fixture.Database
            .GetCollection<ProdutoDocument>(
                MongoDbContext.ProdutosCollectionName)
            .InsertOneAsync(new ProdutoDocument
            {
                Id = productId,
                EmpresaId = companyId,
                Codigo = "PROD-CALCULO-API",
                Nome = "Produto cálculo API",
                UnidadeFuncional = "unidade",
                Ativo = true,
                CriadoEm = now,
                AtualizadoEm = now,
                SchemaVersion = 1
            });

        await fixture.Database
            .GetCollection<FornecedorDocument>(
                MongoDbContext.FornecedoresCollectionName)
            .InsertOneAsync(new FornecedorDocument
            {
                Id = supplierId,
                Codigo = "FORN-CALCULO-API",
                RazaoSocial = "Fornecedor cálculo API",
                Cnpj = "62000000000102",
                Ativo = true,
                CriadoEm = now,
                AtualizadoEm = now,
                SchemaVersion = 1
            });

        await fixture.Database
            .GetCollection<FatorEmissaoDocument>(
                MongoDbContext.FatoresEmissaoCollectionName)
            .InsertOneAsync(new FatorEmissaoDocument
            {
                Id = factorId,
                Codigo = "FE-ENERGIA-CALCULO-API",
                Nome = "Energia elétrica API",
                Categoria = "ENERGIA",
                Valor = 0.0817m,
                UnidadeBase = "kWh",
                Escopo = "ESCOPO_2",
                Versao = 1,
                FonteReferencia = includeOptionalFactorFields
                    ? "Teste de API"
                    : null,
                Metodologia = includeOptionalFactorFields
                    ? "Atividade multiplicada pelo fator"
                    : null,
                ValidoDe = now.AddYears(-1),
                ValidoAte = expiredFactor
                    ? now.AddDays(-1)
                    : now.AddYears(1),
                Ativo = activeFactor,
                CriadoEm = now,
                AtualizadoEm = now,
                SchemaVersion = 1
            });

        return new ReferenceIds(
            companyId,
            productId,
            supplierId,
            factorId);
    }

    private async Task<HttpClient> AuthenticatedClientAsync()
    {
        var client = fixture.CreateClient();
        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "analista@carbono.com",
                senha = "Carbono@123"
            });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = await login.Content.ReadFromJsonAsync<TokenViewModel>();
        Assert.NotNull(token);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.Token);

        return client;
    }

    private static object CalculationPayload(
        ReferenceIds references,
        bool includeOptionalEmissionFields = true,
        string activityUnit = "kWh")
    {
        return new
        {
            produtoId = references.ProductId.ToString(),
            fornecedorId = references.SupplierId.ToString(),
            fatorEmissaoId = references.FactorId.ToString(),
            quantidadeAtividade = 1500m,
            unidadeAtividade = activityUnit,
            lote = new
            {
                codigo = "LOTE-CALCULO-API",
                quantidadeProduzida = 100m,
                unidade = "unidades",
                dataProducao = DateTime.UtcNow.AddDays(-1)
            },
            etapa = new
            {
                nome = "Consumo de energia",
                ordem = 1,
                categoria = "ENERGIA"
            },
            dadosAtividade = new
            {
                tipo = "ENERGIA",
                consumoKwh = 1500m,
                fonteEnergia = "Rede elétrica",
                percentualRenovavel = 20m
            },
            fonteEmissao = includeOptionalEmissionFields
                ? "Energia elétrica"
                : null,
            observacao = includeOptionalEmissionFields
                ? "Teste de cálculo via API"
                : null
        };
    }

    private static async Task AssertErrorAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus,
        string expectedMessageFragment)
    {
        Assert.Equal(expectedStatus, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            (int)expectedStatus,
            json.RootElement.GetProperty("statusCode").GetInt32());
        Assert.Equal(
            expectedStatus.ToString(),
            json.RootElement.GetProperty("error").GetString());
        Assert.Contains(
            expectedMessageFragment,
            json.RootElement.GetProperty("message").GetString()
                ?? string.Empty);
        Assert.True(
            json.RootElement.GetProperty("timestamp").TryGetDateTime(out _));
    }

    private sealed record ReferenceIds(
        ObjectId CompanyId,
        ObjectId ProductId,
        ObjectId SupplierId,
        ObjectId FactorId);
}
