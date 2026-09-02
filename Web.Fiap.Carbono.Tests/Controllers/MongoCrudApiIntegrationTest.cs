using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Dtos.MongoDb.Empresas;
using Web.Fiap.Carbono.Dtos.MongoDb.FatoresEmissao;
using Web.Fiap.Carbono.Dtos.MongoDb.Fornecedores;
using Web.Fiap.Carbono.Dtos.MongoDb.Produtos;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Models.Documents.Embedded;
using Web.Fiap.Carbono.Tests.Config;
using Web.Fiap.Carbono.ViewModel;

namespace Web.Fiap.Carbono.Tests.Controllers;

[Collection(MongoApiCollection.Name)]
public sealed class MongoCrudApiIntegrationTest(
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
    public async Task ShouldKeepReadsPublicAndEnforceWriteRoles()
    {
        using var publicClient = fixture.CreateClient();

        var publicRead = await publicClient.GetAsync("/api/empresas");
        Assert.Equal(HttpStatusCode.OK, publicRead.StatusCode);

        var unauthenticatedWrite = await publicClient.PostAsJsonAsync(
            "/api/empresas",
            CompanyPayload("CRUD-TEMP-EMP-AUTH", "51000000000101")
        );
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            unauthenticatedWrite.StatusCode
        );

        using var analystClient = await AuthenticatedClientAsync(
            "ANALISTA_ESG"
        );
        var analystCreate = await analystClient.PostAsJsonAsync(
            "/api/empresas",
            CompanyPayload("CRUD-TEMP-EMP-AUTH", "51000000000101")
        );
        Assert.Equal(HttpStatusCode.Created, analystCreate.StatusCode);

        var company = await analystCreate.Content
            .ReadFromJsonAsync<EmpresaResponse>();
        Assert.NotNull(company);

        var factorCreate = await analystClient.PostAsJsonAsync(
            "/api/fatores-emissao",
            FactorPayload("CRUD-TEMP-FATOR-AUTH")
        );
        Assert.Equal(HttpStatusCode.Forbidden, factorCreate.StatusCode);

        var analystDelete = await analystClient.DeleteAsync(
            $"/api/empresas/{company.Id}"
        );
        Assert.Equal(HttpStatusCode.Forbidden, analystDelete.StatusCode);

        using var adminClient = await AuthenticatedClientAsync("ADMIN");
        var adminDelete = await adminClient.DeleteAsync(
            $"/api/empresas/{company.Id}"
        );
        Assert.Equal(HttpStatusCode.NoContent, adminDelete.StatusCode);
    }

    [Fact]
    public async Task ShouldPerformCrudForFourPhaseNineRestResources()
    {
        using var client = await AuthenticatedClientAsync("ADMIN");

        var company = await CreateCompanyAsync(client);
        var companyRead = await client.GetAsync(
            $"/api/empresas/{company.Id}"
        );
        Assert.Equal(HttpStatusCode.OK, companyRead.StatusCode);

        var companyUpdate = await client.PutAsJsonAsync(
            $"/api/empresas/{company.Id}",
            CompanyPayload(
                "CRUD-TEMP-EMP-API",
                "52000000000102",
                "Empresa atualizada"
            )
        );
        Assert.Equal(HttpStatusCode.OK, companyUpdate.StatusCode);

        var duplicateCompany = await client.PostAsJsonAsync(
            "/api/empresas",
            CompanyPayload(
                "CRUD-TEMP-EMP-DUP",
                "52000000000102"
            )
        );
        await AssertErrorAsync(
            duplicateCompany,
            HttpStatusCode.Conflict
        );

        var missingParentProduct = await client.PostAsJsonAsync(
            "/api/produtos",
            ProductPayload(ObjectId.GenerateNewId().ToString())
        );
        await AssertErrorAsync(
            missingParentProduct,
            HttpStatusCode.NotFound
        );

        var productCreate = await client.PostAsJsonAsync(
            "/api/produtos",
            ProductPayload(company.Id)
        );
        Assert.Equal(HttpStatusCode.Created, productCreate.StatusCode);
        var product = await productCreate.Content
            .ReadFromJsonAsync<ProdutoResponse>();
        Assert.NotNull(product);
        Assert.Single(product.AtributosAmbientais!.Materiais);

        await fixture.Database
            .GetCollection<BsonDocument>(
                MongoDbContext.ProdutosCollectionName
            )
            .UpdateOneAsync(
                Builders<BsonDocument>.Filter.Eq(
                    "_id",
                    ObjectId.Parse(product.Id)
                ),
                Builders<BsonDocument>.Update.Set(
                    "atributosAmbientais.percentualFrotaEletrificada",
                    new Decimal128(45m)
                )
            );

        var productUpdate = await client.PutAsJsonAsync(
            $"/api/produtos/{product.Id}",
            ProductPayload(company.Id, "Produto atualizado")
        );
        Assert.Equal(HttpStatusCode.OK, productUpdate.StatusCode);

        var storedProduct = await fixture.Database
            .GetCollection<ProdutoDocument>(
                MongoDbContext.ProdutosCollectionName
            )
            .Find(document => document.Id == ObjectId.Parse(product.Id))
            .FirstAsync();
        Assert.Equal(
            45m,
            Decimal128.ToDecimal(
                storedProduct.AtributosAmbientais!
                    .CamposAdicionais!["percentualFrotaEletrificada"]
                    .AsDecimal128
            )
        );

        var supplierCreate = await client.PostAsJsonAsync(
            "/api/fornecedores",
            SupplierPayload()
        );
        Assert.Equal(HttpStatusCode.Created, supplierCreate.StatusCode);
        var supplier = await supplierCreate.Content
            .ReadFromJsonAsync<FornecedorResponse>();
        Assert.NotNull(supplier);

        var supplierUpdate = await client.PutAsJsonAsync(
            $"/api/fornecedores/{supplier.Id}",
            SupplierPayload("Fornecedor atualizado")
        );
        Assert.Equal(HttpStatusCode.OK, supplierUpdate.StatusCode);

        var factorCreate = await client.PostAsJsonAsync(
            "/api/fatores-emissao",
            FactorPayload()
        );
        Assert.Equal(HttpStatusCode.Created, factorCreate.StatusCode);
        var factor = await factorCreate.Content
            .ReadFromJsonAsync<FatorEmissaoResponse>();
        Assert.NotNull(factor);
        Assert.Equal(0.0817m, factor.Valor);

        var factorUpdate = await client.PutAsJsonAsync(
            $"/api/fatores-emissao/{factor.Id}",
            FactorPayload(value: 0.09m)
        );
        Assert.Equal(HttpStatusCode.OK, factorUpdate.StatusCode);

        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/produtos/{product.Id}"))
                .StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync(
                $"/api/fornecedores/{supplier.Id}"
            )).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync(
                $"/api/fatores-emissao/{factor.Id}"
            )).StatusCode
        );
        Assert.Equal(
            HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/empresas/{company.Id}"))
                .StatusCode
        );

        await AssertErrorAsync(
            await client.GetAsync($"/api/produtos/{product.Id}"),
            HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task ShouldReturnGlobalErrorContractAndProtectReferences()
    {
        using var client = await AuthenticatedClientAsync("ADMIN");

        await AssertErrorAsync(
            await client.GetAsync("/api/empresas/not-an-object-id"),
            HttpStatusCode.BadRequest
        );

        var invalidFactorValidity = await client.PostAsJsonAsync(
            "/api/fatores-emissao",
            FactorPayload(
                validFrom: new DateTime(
                    2026,
                    12,
                    31,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc
                ),
                validTo: new DateTime(
                    2026,
                    1,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc
                )
            )
        );
        await AssertErrorAsync(
            invalidFactorValidity,
            HttpStatusCode.BadRequest
        );

        var company = await CreateCompanyAsync(client);
        var product = await PostAsync<ProdutoResponse>(
            client,
            "/api/produtos",
            ProductPayload(company.Id)
        );
        var supplier = await PostAsync<FornecedorResponse>(
            client,
            "/api/fornecedores",
            SupplierPayload()
        );
        var factor = await PostAsync<FatorEmissaoResponse>(
            client,
            "/api/fatores-emissao",
            FactorPayload()
        );

        await fixture.Database
            .GetCollection<EmissaoCarbonoDocument>(
                MongoDbContext.EmissoesCarbonoCollectionName
            )
            .InsertOneAsync(
                CreateReferenceEmission(company, product, supplier, factor)
            );

        await AssertErrorAsync(
            await client.DeleteAsync($"/api/empresas/{company.Id}"),
            HttpStatusCode.Conflict
        );
        await AssertErrorAsync(
            await client.DeleteAsync($"/api/produtos/{product.Id}"),
            HttpStatusCode.Conflict
        );
        await AssertErrorAsync(
            await client.DeleteAsync(
                $"/api/fornecedores/{supplier.Id}"
            ),
            HttpStatusCode.Conflict
        );
        await AssertErrorAsync(
            await client.DeleteAsync(
                $"/api/fatores-emissao/{factor.Id}"
            ),
            HttpStatusCode.Conflict
        );
    }

    [Fact]
    public async Task ShouldDocumentPhaseNineEndpointsAndAuthorizationInSwagger()
    {
        using var client = fixture.CreateClient();
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var document = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync()
        );
        var root = document.RootElement;
        var paths = root.GetProperty("paths");

        foreach (var path in new[]
                 {
                     "/api/empresas",
                     "/api/produtos",
                     "/api/fornecedores",
                     "/api/fatores-emissao"
                 })
        {
            var endpoint = paths.GetProperty(path);
            Assert.True(endpoint.TryGetProperty("get", out _));
            Assert.True(endpoint.TryGetProperty("post", out _));
        }

        var companyOperations = paths.GetProperty("/api/empresas");
        Assert.Equal(
            "Cadastra uma nova empresa ESG no MongoDB.",
            companyOperations
                .GetProperty("post")
                .GetProperty("summary")
                .GetString()
        );
        Assert.True(
            companyOperations
                .GetProperty("post")
                .GetProperty("security")
                .GetArrayLength() > 0
        );
        Assert.False(
            companyOperations
                .GetProperty("get")
                .TryGetProperty("security", out _)
        );
        Assert.True(
            root.GetProperty("components")
                .GetProperty("securitySchemes")
                .TryGetProperty("Bearer", out _)
        );
    }

    private async Task<HttpClient> AuthenticatedClientAsync(string role)
    {
        var client = fixture.CreateClient();
        var email = role == "ADMIN"
            ? "admin@carbono.com"
            : "analista@carbono.com";

        var login = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email,
                senha = "Carbono@123"
            }
        );
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = await login.Content.ReadFromJsonAsync<TokenViewModel>();
        Assert.NotNull(token);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token.Token
            );
        return client;
    }

    private static async Task<EmpresaResponse> CreateCompanyAsync(
        HttpClient client
    )
    {
        return await PostAsync<EmpresaResponse>(
            client,
            "/api/empresas",
            CompanyPayload("CRUD-TEMP-EMP-API", "52000000000102")
        );
    }

    private static async Task<TResponse> PostAsync<TResponse>(
        HttpClient client,
        string uri,
        object payload
    )
    {
        var response = await client.PostAsJsonAsync(uri, payload);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TResponse>();
        return Assert.IsType<TResponse>(body);
    }

    private static async Task AssertErrorAsync(
        HttpResponseMessage response,
        HttpStatusCode expectedStatus
    )
    {
        Assert.Equal(expectedStatus, response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync()
        );
        var root = json.RootElement;

        Assert.Equal(
            (int)expectedStatus,
            root.GetProperty("statusCode").GetInt32()
        );
        Assert.False(string.IsNullOrWhiteSpace(
            root.GetProperty("error").GetString()
        ));
        Assert.False(string.IsNullOrWhiteSpace(
            root.GetProperty("message").GetString()
        ));
        Assert.True(root.TryGetProperty("timestamp", out _));
    }

    private static object CompanyPayload(
        string code,
        string cnpj,
        string companyName = "Empresa REST"
    ) => new
    {
        codigo = code,
        razaoSocial = companyName,
        nomeFantasia = "Empresa REST",
        cnpj,
        setor = "TECNOLOGIA",
        ativa = true,
        metasReducao = new[]
        {
            new
            {
                tipo = "EMISSOES_GEE",
                anoBase = 2025,
                anoMeta = 2030,
                percentualReducao = 25m
            }
        },
        governanca = new
        {
            responsavelEsg = "Responsável REST",
            comiteEsg = true,
            frequenciaAuditoria = "ANUAL"
        }
    };

    private static object ProductPayload(
        string companyId,
        string name = "Produto REST"
    ) => new
    {
        empresaId = companyId,
        codigo = "CRUD-TEMP-PROD-API",
        nome = name,
        categoria = "EMBALAGEM",
        unidadeFuncional = "UNIDADE",
        ativo = true,
        atributosAmbientais = new
        {
            percentualReciclavel = 90m,
            materiais = new[]
            {
                new
                {
                    nome = "Alumínio reciclado",
                    percentualComposicao = 100m,
                    origemRenovavel = false,
                    origemReciclada = true
                }
            }
        }
    };

    private static object SupplierPayload(
        string companyName = "Fornecedor REST"
    ) => new
    {
        codigo = "CRUD-TEMP-FOR-API",
        razaoSocial = companyName,
        nomeFantasia = "Fornecedor REST",
        cnpj = "53000000000103",
        ativo = true,
        categoriasAtuacao = new[] { "TRANSPORTE" },
        certificacoes = Array.Empty<object>(),
        statusAuditoria = "APROVADO",
        nivelRiscoEsg = "BAIXO"
    };

    private static object FactorPayload(
        string code = "CRUD-TEMP-FATOR-API",
        decimal value = 0.0817m,
        DateTime? validFrom = null,
        DateTime? validTo = null
    ) => new
    {
        codigo = code,
        nome = "Energia elétrica REST",
        categoria = "ENERGIA",
        valor = value,
        unidadeBase = "KWH",
        escopo = "ESCOPO_2",
        versao = 1,
        fonteReferencia = "Teste de integração",
        metodologia = "Atividade multiplicada pelo fator",
        validoDe = validFrom ??
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        validoAte = validTo ??
            new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
        ativo = true
    };

    private static EmissaoCarbonoDocument CreateReferenceEmission(
        EmpresaResponse company,
        ProdutoResponse product,
        FornecedorResponse supplier,
        FatorEmissaoResponse factor
    )
    {
        var now = new DateTime(
            2026,
            9,
            2,
            12,
            0,
            0,
            DateTimeKind.Utc
        );

        return new EmissaoCarbonoDocument
        {
            Id = ObjectId.GenerateNewId(),
            Codigo = "REFERENCE-EMISSION-PHASE-9",
            EmpresaId = ObjectId.Parse(company.Id),
            ProdutoId = ObjectId.Parse(product.Id),
            FornecedorId = ObjectId.Parse(supplier.Id),
            FatorEmissaoId = ObjectId.Parse(factor.Id),
            Lote = new LoteSnapshot
            {
                Codigo = "LOTE-PHASE-9",
                QuantidadeProduzida = 1m,
                Unidade = "UNIDADE",
                DataProducao = now
            },
            Etapa = new EtapaSnapshot
            {
                Nome = "Transporte",
                Ordem = 1,
                Categoria = "TRANSPORTE"
            },
            QuantidadeAtividade = 1m,
            DadosAtividade = new BsonDocument("tipo", "TRANSPORTE"),
            FatorAplicado = new FatorEmissaoSnapshot
            {
                Codigo = factor.Codigo,
                Nome = factor.Nome,
                Valor = factor.Valor,
                UnidadeBase = factor.UnidadeBase,
                Escopo = factor.Escopo,
                Versao = factor.Versao
            },
            QuantidadeEmitidaKgCO2e = factor.Valor,
            MetodoCalculo =
                "QuantidadeAtividade * ValorFatorCo2e",
            CalculadoPor = "phase9@test.invalid",
            DataEmissao = now,
            CriadoEm = now,
            AtualizadoEm = now,
            SchemaVersion = 1
        };
    }
}
