using MongoDB.Bson;
using Web.Fiap.Carbono.Data.MongoDb.Repositories;
using Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Services.MongoDb;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;
using Web.Fiap.Carbono.Tests.Data.MongoDb;

namespace Web.Fiap.Carbono.Tests.Services.MongoDb;

[Collection(MongoRepositoryCollection.Name)]
public sealed class MongoEmissionReferenceLoadingTest(
    MongoRepositoryFixture fixture)
{
    [Fact]
    public async Task LoadReferencesAsync_DerivesCompanyFromProduct()
    {
        var context = await fixture.CreateContextAsync();
        var empresaRepository = new MongoEmpresaRepository(context);
        var produtoRepository = new MongoProdutoRepository(context);
        var fornecedorRepository = new MongoFornecedorRepository(context);
        var fatorRepository = new MongoFatorEmissaoRepository(context);
        var emissaoRepository = new MongoEmissaoCarbonoRepository(context);

        var empresa = CreateEmpresa();
        var produto = CreateProduto(empresa.Id);
        var fornecedor = CreateFornecedor();
        var fator = CreateFator();

        await empresaRepository.CreateAsync(empresa, default);
        await produtoRepository.CreateAsync(produto, default);
        await fornecedorRepository.CreateAsync(fornecedor, default);
        await fatorRepository.CreateAsync(fator, default);

        var service = new MongoEmissaoCarbonoService(
            emissaoRepository,
            produtoRepository,
            empresaRepository,
            fornecedorRepository,
            fatorRepository,
            TimeProvider.System);

        var result = await service.LoadReferencesAsync(
            CreateRequest(produto.Id, fornecedor.Id, fator.Id),
            default);

        Assert.Equal(empresa.Id, result.Empresa.Id);
        Assert.Equal(produto.Id, result.Produto.Id);
        Assert.Equal(fornecedor.Id, result.Fornecedor.Id);
        Assert.Equal(fator.Id, result.Fator.Id);
        Assert.Equal(result.Produto.EmpresaId, result.Empresa.Id);
    }

    [Fact]
    public async Task LoadReferencesAsync_WithMissingFactor_ThrowsNotFound()
    {
        var context = await fixture.CreateContextAsync();
        var empresaRepository = new MongoEmpresaRepository(context);
        var produtoRepository = new MongoProdutoRepository(context);
        var fornecedorRepository = new MongoFornecedorRepository(context);
        var fatorRepository = new MongoFatorEmissaoRepository(context);
        var emissaoRepository = new MongoEmissaoCarbonoRepository(context);

        var empresa = CreateEmpresa();
        var produto = CreateProduto(empresa.Id);
        var fornecedor = CreateFornecedor();

        await empresaRepository.CreateAsync(empresa, default);
        await produtoRepository.CreateAsync(produto, default);
        await fornecedorRepository.CreateAsync(fornecedor, default);

        var service = new MongoEmissaoCarbonoService(
            emissaoRepository,
            produtoRepository,
            empresaRepository,
            fornecedorRepository,
            fatorRepository,
            TimeProvider.System);

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => service.LoadReferencesAsync(
                CreateRequest(
                    produto.Id,
                    fornecedor.Id,
                    ObjectId.GenerateNewId()),
                default));

        Assert.Equal(
            "Fator de emissão não encontrado.",
            exception.Message);
    }

    [Fact]
    public async Task LoadReferencesAsync_WithMalformedId_ThrowsBadRequest()
    {
        var context = await fixture.CreateContextAsync();
        var service = new MongoEmissaoCarbonoService(
            new MongoEmissaoCarbonoRepository(context),
            new MongoProdutoRepository(context),
            new MongoEmpresaRepository(context),
            new MongoFornecedorRepository(context),
            new MongoFatorEmissaoRepository(context),
            TimeProvider.System);

        var request = CreateRequest(
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            ObjectId.GenerateNewId(),
            produtoIdOverride: "invalid-object-id");

        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => service.LoadReferencesAsync(request, default));

        Assert.Contains("produtoId", exception.Message);
    }

    [Fact]
    public async Task GetCalculationTimestampUtc_UsesConfiguredClockAndReturnsUtc()
    {
        var context = await fixture.CreateContextAsync();
        var expected = new DateTimeOffset(
            2026,
            9,
            2,
            14,
            30,
            0,
            TimeSpan.Zero);

        var service = new MongoEmissaoCarbonoService(
            new MongoEmissaoCarbonoRepository(context),
            new MongoProdutoRepository(context),
            new MongoEmpresaRepository(context),
            new MongoFornecedorRepository(context),
            new MongoFatorEmissaoRepository(context),
            new FixedTimeProvider(expected));

        var result = service.GetCalculationTimestampUtc();

        Assert.Equal(expected.UtcDateTime, result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    private static CalcularEmissaoMongoRequest CreateRequest(
        ObjectId produtoId,
        ObjectId fornecedorId,
        ObjectId fatorId,
        string? produtoIdOverride = null)
    {
        return new CalcularEmissaoMongoRequest
        {
            ProdutoId = produtoIdOverride ?? produtoId.ToString(),
            FornecedorId = fornecedorId.ToString(),
            FatorEmissaoId = fatorId.ToString(),
            QuantidadeAtividade = 100m,
            UnidadeAtividade = "kWh",
            Lote = new LoteEmissaoRequest
            {
                Codigo = "LOTE-TESTE"
            },
            Etapa = new EtapaEmissaoRequest
            {
                Nome = "Energia",
                Ordem = 1,
                Categoria = "ENERGIA"
            },
            DadosAtividade = new DadosEnergiaRequest
            {
                ConsumoKwh = 100m,
                FonteEnergia = "Rede elétrica"
            }
        };
    }

    private static EmpresaDocument CreateEmpresa()
    {
        return new EmpresaDocument
        {
            Id = ObjectId.GenerateNewId(),
            Codigo = $"EMP-{Guid.NewGuid():N}",
            RazaoSocial = "Empresa teste",
            Cnpj = Random.Shared.NextInt64(
                10_000_000_000_000,
                99_999_999_999_999).ToString(),
            Ativa = true,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
            SchemaVersion = 1
        };
    }

    private static ProdutoDocument CreateProduto(ObjectId empresaId)
    {
        return new ProdutoDocument
        {
            Id = ObjectId.GenerateNewId(),
            EmpresaId = empresaId,
            Codigo = $"PROD-{Guid.NewGuid():N}",
            Nome = "Produto teste",
            UnidadeFuncional = "unidade",
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
            SchemaVersion = 1
        };
    }

    private static FornecedorDocument CreateFornecedor()
    {
        return new FornecedorDocument
        {
            Id = ObjectId.GenerateNewId(),
            Codigo = $"FORN-{Guid.NewGuid():N}",
            RazaoSocial = "Fornecedor teste",
            Cnpj = Random.Shared.NextInt64(
                10_000_000_000_000,
                99_999_999_999_999).ToString(),
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
            SchemaVersion = 1
        };
    }

    private static FatorEmissaoDocument CreateFator()
    {
        return new FatorEmissaoDocument
        {
            Id = ObjectId.GenerateNewId(),
            Codigo = $"FATOR-{Guid.NewGuid():N}",
            Nome = "Fator teste",
            Categoria = "ENERGIA",
            Valor = 0.0817m,
            UnidadeBase = "kWh",
            Escopo = "ESCOPO_2",
            Versao = 1,
            ValidoDe = DateTime.UtcNow.AddDays(-1),
            Ativo = true,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow,
            SchemaVersion = 1
        };
    }

    private sealed class FixedTimeProvider(
        DateTimeOffset timestamp) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return timestamp;
        }
    }
}
