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
    public async Task LegacyEmissionRoutes_ReadFromMongo()
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(activeFactor: true);
        using var client = await AuthenticatedClientAsync();
        var created = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(references));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var body = await created.Content
            .ReadFromJsonAsync<EmissaoCarbonoMongoResponseViewModel>();
        Assert.NotNull(body);

        await fixture.Database
            .GetCollection<EmissaoCarbonoDocument>(
                MongoDbContext.EmissoesCarbonoCollectionName)
            .UpdateOneAsync(
                item => item.Id == ObjectId.Parse(body.Id),
                Builders<EmissaoCarbonoDocument>.Update.Set(
                    item => item.LegacyId,
                    41));

        var paged = await client.GetAsync(
            "/api/emissoes-carbono?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, paged.StatusCode);

        var byLegacyId = await client.GetAsync(
            "/api/emissoes-carbono/41");
        Assert.Equal(HttpStatusCode.OK, byLegacyId.StatusCode);
        var legacyBody = await byLegacyId.Content
            .ReadFromJsonAsync<EmissaoCarbonoMongoResponseViewModel>();
        Assert.NotNull(legacyBody);
        Assert.Equal(41, legacyBody.LegacyId);
    }

    [Fact]
    public async Task GetById_ReturnsPersistedMongoEmissionAndValidatesObjectId()
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(activeFactor: true);
        using var client = await AuthenticatedClientAsync();
        var created = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(references));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var body = await created.Content
            .ReadFromJsonAsync<EmissaoCarbonoMongoResponseViewModel>();
        Assert.NotNull(body);

        var response = await client.GetAsync($"/api/emissoes-carbono/mongodb/{body.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var read = await response.Content
            .ReadFromJsonAsync<EmissaoCarbonoMongoResponseViewModel>();
        Assert.NotNull(read);
        Assert.Equal(body.Id, read.Id);
        Assert.Equal(body.QuantidadeEmitidaKgCO2e, read.QuantidadeEmitidaKgCO2e);

        var malformed = await client.GetAsync("/api/emissoes-carbono/mongodb/not-an-object-id");
        Assert.Equal(HttpStatusCode.BadRequest, malformed.StatusCode);
        var missing = await client.GetAsync(
            $"/api/emissoes-carbono/mongodb/{ObjectId.GenerateNewId()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

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

        var persisted = await fixture.Database
            .GetCollection<BsonDocument>(
                MongoDbContext.EmissoesCarbonoCollectionName)
            .Find(Builders<BsonDocument>.Filter.Eq(
                "_id",
                ObjectId.Parse(body.Id)))
            .SingleAsync();

        var activity = persisted["dadosAtividade"].AsBsonDocument;
        Assert.Equal("ENERGIA", activity["tipo"].AsString);
        Assert.Equal(BsonType.Decimal128, activity["consumoKwh"].BsonType);
        Assert.Equal(
            1500m,
            Decimal128.ToDecimal(activity["consumoKwh"].AsDecimal128));
        Assert.Equal("Rede elétrica", activity["fonteEnergia"].AsString);
        Assert.Equal(
            20m,
            Decimal128.ToDecimal(
                activity["percentualRenovavel"].AsDecimal128));
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
    public async Task Calculate_WithNotYetValidFactor_ReturnsUnprocessableEntityAndPersistsNothing()
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(
            activeFactor: true,
            notYetValidFactor: true);
        using var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(references));

        await AssertErrorAsync(
            response,
            HttpStatusCode.UnprocessableEntity,
            "somente é válido a partir");

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

    [Fact]
    public async Task Calculate_WithTransportActivity_PersistsFlexibleActivityData()
    {
        await AssertSupportedActivityAsync(
            new ActivityScenario(
                Category: "TRANSPORTE",
                Unit: "ton_km",
                ActivityQuantity: 5000m,
                FactorValue: 0.1200m,
                ExpectedEmission: 600.0000m,
                ActivityData: new
                {
                    tipo = "TRANSPORTE",
                    distanciaKm = 500m,
                    cargaToneladas = 10m,
                    combustivel = "DIESEL",
                    modal = "RODOVIARIO"
                },
                DecimalFields: new Dictionary<string, decimal>
                {
                    ["distanciaKm"] = 500m,
                    ["cargaToneladas"] = 10m
                },
                StringFields: new Dictionary<string, string>
                {
                    ["combustivel"] = "DIESEL",
                    ["modal"] = "RODOVIARIO"
                }));
    }

    [Fact]
    public async Task Calculate_WithRawMaterialActivity_PersistsFlexibleActivityData()
    {
        await AssertSupportedActivityAsync(
            new ActivityScenario(
                Category: "MATERIA_PRIMA",
                Unit: "kg",
                ActivityQuantity: 450m,
                FactorValue: 0.7200m,
                ExpectedEmission: 324.0000m,
                ActivityData: new
                {
                    tipo = "MATERIA_PRIMA",
                    material = "ALUMINIO_RECICLADO",
                    pesoKg = 450m,
                    percentualReciclado = 80m,
                    origem = "Fornecedor homologado"
                },
                DecimalFields: new Dictionary<string, decimal>
                {
                    ["pesoKg"] = 450m,
                    ["percentualReciclado"] = 80m
                },
                StringFields: new Dictionary<string, string>
                {
                    ["material"] = "ALUMINIO_RECICLADO",
                    ["origem"] = "Fornecedor homologado"
                }));
    }

    [Fact]
    public async Task Calculate_WithWasteActivity_PersistsFlexibleActivityData()
    {
        await AssertSupportedActivityAsync(
            new ActivityScenario(
                Category: "RESIDUO",
                Unit: "kg",
                ActivityQuantity: 180m,
                FactorValue: 0.4500m,
                ExpectedEmission: 81.0000m,
                ActivityData: new
                {
                    tipo = "RESIDUO",
                    classe = "CLASSE_II",
                    pesoKg = 180m,
                    tipoResiduo = "EMBALAGEM",
                    tratamento = "RECICLAGEM",
                    distanciaDestinoKm = 35m,
                    percentualReciclavel = 90m
                },
                DecimalFields: new Dictionary<string, decimal>
                {
                    ["pesoKg"] = 180m,
                    ["distanciaDestinoKm"] = 35m,
                    ["percentualReciclavel"] = 90m
                },
                StringFields: new Dictionary<string, string>
                {
                    ["classe"] = "CLASSE_II",
                    ["tipoResiduo"] = "EMBALAGEM",
                    ["tratamento"] = "RECICLAGEM"
                }));
    }

    [Theory]
    [InlineData(MissingReference.Product, "Produto não encontrado.")]
    [InlineData(MissingReference.Company, "A empresa associada ao produto não foi encontrada.")]
    [InlineData(MissingReference.Supplier, "Fornecedor não encontrado.")]
    [InlineData(MissingReference.Factor, "Fator de emissão não encontrado.")]
    public async Task Calculate_WithMissingReference_ReturnsNotFoundAndPersistsNothing(
        MissingReference missingReference,
        string expectedMessage)
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(
            activeFactor: true,
            omitCompany: missingReference == MissingReference.Company);
        using var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(
                references,
                productIdOverride: missingReference == MissingReference.Product
                    ? ObjectId.GenerateNewId().ToString()
                    : null,
                supplierIdOverride: missingReference == MissingReference.Supplier
                    ? ObjectId.GenerateNewId().ToString()
                    : null,
                factorIdOverride: missingReference == MissingReference.Factor
                    ? ObjectId.GenerateNewId().ToString()
                    : null));

        await AssertErrorAsync(
            response,
            HttpStatusCode.NotFound,
            expectedMessage);

        await AssertNoEmissionPersistedAsync();
    }

    [Theory]
    [InlineData(PublicObjectId.Product, "produtoId")]
    [InlineData(PublicObjectId.Supplier, "fornecedorId")]
    [InlineData(PublicObjectId.Factor, "fatorEmissaoId")]
    public async Task Calculate_WithMalformedObjectId_ReturnsBadRequestAndPersistsNothing(
        PublicObjectId publicObjectId,
        string expectedField)
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(activeFactor: true);
        using var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(
                references,
                productIdOverride: publicObjectId == PublicObjectId.Product
                    ? "invalid-object-id"
                    : null,
                supplierIdOverride: publicObjectId == PublicObjectId.Supplier
                    ? "invalid-object-id"
                    : null,
                factorIdOverride: publicObjectId == PublicObjectId.Factor
                    ? "invalid-object-id"
                    : null));

        await AssertErrorAsync(
            response,
            HttpStatusCode.BadRequest,
            expectedField);

        await AssertNoEmissionPersistedAsync();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Calculate_WithNonPositiveQuantity_ReturnsValidationErrorAndPersistsNothing(
        int activityQuantity)
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(activeFactor: true);
        using var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(
                references,
                activityQuantity: activityQuantity));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        Assert.Equal(
            (int)HttpStatusCode.BadRequest,
            json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(
            "One or more validation errors occurred.",
            json.RootElement.GetProperty("title").GetString());
        Assert.True(json.RootElement.GetProperty("errors").EnumerateObject().Any());
        Assert.True(json.RootElement.TryGetProperty("traceId", out _));

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

    private async Task AssertSupportedActivityAsync(
        ActivityScenario scenario)
    {
        await fixture.ClearAsync();
        var references = await SeedReferencesAsync(
            activeFactor: true,
            factorCategory: scenario.Category,
            factorUnit: scenario.Unit,
            factorValue: scenario.FactorValue);
        using var client = await AuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync(
            "/api/emissoes-carbono/calcular",
            CalculationPayload(
                references,
                activityUnit: scenario.Unit,
                activityQuantity: scenario.ActivityQuantity,
                stageCategory: scenario.Category,
                activityData: scenario.ActivityData));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content
            .ReadFromJsonAsync<EmissaoCarbonoMongoResponseViewModel>();

        Assert.NotNull(body);
        Assert.Equal(scenario.ExpectedEmission, body.QuantidadeEmitidaKgCO2e);
        Assert.Equal(scenario.Category, body.Etapa.Categoria);
        Assert.Equal(scenario.FactorValue, body.FatorAplicado.Valor);

        var persisted = await fixture.Database
            .GetCollection<BsonDocument>(
                MongoDbContext.EmissoesCarbonoCollectionName)
            .Find(Builders<BsonDocument>.Filter.Eq(
                "_id",
                ObjectId.Parse(body.Id)))
            .SingleAsync();

        var activity = persisted["dadosAtividade"].AsBsonDocument;
        Assert.Equal(scenario.Category, activity["tipo"].AsString);

        foreach (var field in scenario.DecimalFields)
        {
            Assert.Equal(BsonType.Decimal128, activity[field.Key].BsonType);
            Assert.Equal(
                field.Value,
                Decimal128.ToDecimal(activity[field.Key].AsDecimal128));
        }

        foreach (var field in scenario.StringFields)
        {
            Assert.Equal(field.Value, activity[field.Key].AsString);
        }
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
        bool expiredFactor = false,
        bool notYetValidFactor = false,
        bool omitCompany = false,
        string factorCategory = "ENERGIA",
        string factorUnit = "kWh",
        decimal factorValue = 0.0817m)
    {
        var now = DateTime.UtcNow;
        var companyId = ObjectId.GenerateNewId();
        var productId = ObjectId.GenerateNewId();
        var supplierId = ObjectId.GenerateNewId();
        var factorId = ObjectId.GenerateNewId();

        if (!omitCompany)
        {
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
        }

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
                Nome = "Fator de emissão API",
                Categoria = factorCategory,
                Valor = factorValue,
                UnidadeBase = factorUnit,
                Escopo = "ESCOPO_2",
                Versao = 1,
                FonteReferencia = includeOptionalFactorFields
                    ? "Teste de API"
                    : null,
                Metodologia = includeOptionalFactorFields
                    ? "Atividade multiplicada pelo fator"
                    : null,
                ValidoDe = notYetValidFactor
                    ? now.AddDays(1)
                    : now.AddYears(-1),
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
        string activityUnit = "kWh",
        decimal activityQuantity = 1500m,
        string stageCategory = "ENERGIA",
        object? activityData = null,
        string? productIdOverride = null,
        string? supplierIdOverride = null,
        string? factorIdOverride = null)
    {
        activityData ??= new
        {
            tipo = "ENERGIA",
            consumoKwh = 1500m,
            fonteEnergia = "Rede elétrica",
            percentualRenovavel = 20m
        };

        return new
        {
            produtoId = productIdOverride
                ?? references.ProductId.ToString(),
            fornecedorId = supplierIdOverride
                ?? references.SupplierId.ToString(),
            fatorEmissaoId = factorIdOverride
                ?? references.FactorId.ToString(),
            quantidadeAtividade = activityQuantity,
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
                categoria = stageCategory
            },
            dadosAtividade = activityData,
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

    private sealed record ActivityScenario(
        string Category,
        string Unit,
        decimal ActivityQuantity,
        decimal FactorValue,
        decimal ExpectedEmission,
        object ActivityData,
        IReadOnlyDictionary<string, decimal> DecimalFields,
        IReadOnlyDictionary<string, string> StringFields);

    public enum MissingReference
    {
        Product,
        Company,
        Supplier,
        Factor
    }

    public enum PublicObjectId
    {
        Product,
        Supplier,
        Factor
    }
}
