using MongoDB.Bson;
using Web.Fiap.Carbono.Data.MongoDb.Repositories;
using Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Services.MongoDb;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;
using Web.Fiap.Carbono.Tests.Data.MongoDb;

namespace Web.Fiap.Carbono.Tests.Services.MongoDb;

[Collection(MongoRepositoryCollection.Name)]
public sealed class MongoEmissionCalculationServiceTest(
    MongoRepositoryFixture fixture)
{
    private static readonly DateTimeOffset CalculationTimestamp =
        new(
            2026,
            9,
            2,
            14,
            30,
            0,
            TimeSpan.Zero);

    [Fact]
    public async Task CalculateAsync_PersistsCompleteAuditableEmission()
    {
        var setup = await CreateSetupAsync(activeFactor: true);

        var created = await setup.Service.CalculateAsync(
            CreateRequest(
                setup.Product.Id,
                setup.Supplier.Id,
                setup.Factor.Id),
            "  analista@carbono.com  ");

        var persisted = await setup.EmissionRepository.GetByIdAsync(
            created.Id.ToString(),
            default);

        Assert.NotNull(persisted);
        Assert.StartsWith("EMI-", persisted.Codigo);
        Assert.Equal(setup.Company.Id, persisted.EmpresaId);
        Assert.Equal(setup.Product.Id, persisted.ProdutoId);
        Assert.Equal(setup.Supplier.Id, persisted.FornecedorId);
        Assert.Equal(setup.Factor.Id, persisted.FatorEmissaoId);
        Assert.Equal(1500m, persisted.QuantidadeAtividade);
        Assert.Equal(122.5500m, persisted.QuantidadeEmitidaKgCO2e);
        Assert.Equal(0.0817m, persisted.FatorAplicado.Valor);
        Assert.Equal("FE-ENERGIA-TEST", persisted.FatorAplicado.Codigo);
        Assert.Equal(
            "ENERGIA",
            persisted.DadosAtividade["tipo"].AsString);
        Assert.Equal(
            1500m,
            Decimal128.ToDecimal(
                persisted.DadosAtividade["consumoKwh"].AsDecimal128));
        Assert.Equal("LOTE-CALCULO", persisted.Lote.Codigo);
        Assert.Equal("ENERGIA", persisted.Etapa.Categoria);
        Assert.Equal("analista@carbono.com", persisted.CalculadoPor);
        Assert.Equal(CalculationTimestamp.UtcDateTime, persisted.DataEmissao);
        Assert.Equal(CalculationTimestamp.UtcDateTime, persisted.CriadoEm);
        Assert.Equal(CalculationTimestamp.UtcDateTime, persisted.AtualizadoEm);
        Assert.Equal(DateTimeKind.Utc, persisted.DataEmissao.Kind);
        Assert.Equal(1, persisted.SchemaVersion);

        var revisedFactor = CreateFactor(
            setup.Factor.Id,
            active: true,
            value: 0.2500m);

        Assert.True(await setup.FactorRepository.UpdateAsync(
            setup.Factor.Id.ToString(),
            revisedFactor,
            default));

        var historical = await setup.EmissionRepository.GetByIdAsync(
            created.Id.ToString(),
            default);

        Assert.NotNull(historical);
        Assert.Equal(0.0817m, historical.FatorAplicado.Valor);
        Assert.Equal(122.5500m, historical.QuantidadeEmitidaKgCO2e);
    }

    [Fact]
    public async Task CalculateAsync_WithInactiveFactor_DoesNotPersistEmission()
    {
        var setup = await CreateSetupAsync(activeFactor: false);

        await Assert.ThrowsAsync<UnprocessableEntityException>(
            () => setup.Service.CalculateAsync(
                CreateRequest(
                    setup.Product.Id,
                    setup.Supplier.Id,
                    setup.Factor.Id),
                "analista@carbono.com"));

        Assert.Empty(
            await setup.EmissionRepository.GetAllAsync(default));
    }

    private async Task<CalculationSetup> CreateSetupAsync(
        bool activeFactor)
    {
        var context = await fixture.CreateContextAsync();
        var companyRepository = new MongoEmpresaRepository(context);
        var productRepository = new MongoProdutoRepository(context);
        var supplierRepository = new MongoFornecedorRepository(context);
        var factorRepository = new MongoFatorEmissaoRepository(context);
        var emissionRepository = new MongoEmissaoCarbonoRepository(context);

        var company = CreateCompany();
        var product = CreateProduct(company.Id);
        var supplier = CreateSupplier();
        var factor = CreateFactor(
            ObjectId.GenerateNewId(),
            activeFactor,
            0.0817m);

        await companyRepository.CreateAsync(company, default);
        await productRepository.CreateAsync(product, default);
        await supplierRepository.CreateAsync(supplier, default);
        await factorRepository.CreateAsync(factor, default);

        var service = new MongoEmissaoCarbonoService(
            emissionRepository,
            productRepository,
            companyRepository,
            supplierRepository,
            factorRepository,
            new FixedTimeProvider(CalculationTimestamp));

        return new CalculationSetup(
            service,
            emissionRepository,
            factorRepository,
            company,
            product,
            supplier,
            factor);
    }

    private static CalcularEmissaoMongoRequest CreateRequest(
        ObjectId productId,
        ObjectId supplierId,
        ObjectId factorId)
    {
        return new CalcularEmissaoMongoRequest
        {
            ProdutoId = productId.ToString(),
            FornecedorId = supplierId.ToString(),
            FatorEmissaoId = factorId.ToString(),
            QuantidadeAtividade = 1500m,
            UnidadeAtividade = "kWh",
            Lote = new LoteEmissaoRequest
            {
                Codigo = "LOTE-CALCULO",
                QuantidadeProduzida = 100m,
                Unidade = "unidades",
                DataProducao = CalculationTimestamp.UtcDateTime.AddDays(-1)
            },
            Etapa = new EtapaEmissaoRequest
            {
                Nome = "Consumo de energia",
                Ordem = 1,
                Categoria = "ENERGIA"
            },
            DadosAtividade = new DadosEnergiaRequest
            {
                ConsumoKwh = 1500m,
                FonteEnergia = "Rede elétrica",
                PercentualRenovavel = 20m
            },
            FonteEmissao = "  Energia elétrica  ",
            Observacao = "  Evidência acadêmica  "
        };
    }

    private static EmpresaDocument CreateCompany()
    {
        return new EmpresaDocument
        {
            Id = ObjectId.GenerateNewId(),
            Codigo = $"EMP-{Guid.NewGuid():N}",
            RazaoSocial = "Empresa de cálculo",
            Cnpj = Random.Shared.NextInt64(
                10_000_000_000_000,
                99_999_999_999_999).ToString(),
            Ativa = true,
            CriadoEm = CalculationTimestamp.UtcDateTime,
            AtualizadoEm = CalculationTimestamp.UtcDateTime,
            SchemaVersion = 1
        };
    }

    private static ProdutoDocument CreateProduct(ObjectId companyId)
    {
        return new ProdutoDocument
        {
            Id = ObjectId.GenerateNewId(),
            EmpresaId = companyId,
            Codigo = $"PROD-{Guid.NewGuid():N}",
            Nome = "Produto de cálculo",
            UnidadeFuncional = "unidade",
            Ativo = true,
            CriadoEm = CalculationTimestamp.UtcDateTime,
            AtualizadoEm = CalculationTimestamp.UtcDateTime,
            SchemaVersion = 1
        };
    }

    private static FornecedorDocument CreateSupplier()
    {
        return new FornecedorDocument
        {
            Id = ObjectId.GenerateNewId(),
            Codigo = $"FORN-{Guid.NewGuid():N}",
            RazaoSocial = "Fornecedor de cálculo",
            Cnpj = Random.Shared.NextInt64(
                10_000_000_000_000,
                99_999_999_999_999).ToString(),
            Ativo = true,
            CriadoEm = CalculationTimestamp.UtcDateTime,
            AtualizadoEm = CalculationTimestamp.UtcDateTime,
            SchemaVersion = 1
        };
    }

    private static FatorEmissaoDocument CreateFactor(
        ObjectId id,
        bool active,
        decimal value)
    {
        return new FatorEmissaoDocument
        {
            Id = id,
            Codigo = "FE-ENERGIA-TEST",
            Nome = "Energia elétrica",
            Categoria = "ENERGIA",
            Valor = value,
            UnidadeBase = "kWh",
            Escopo = "ESCOPO_2",
            Versao = 1,
            FonteReferencia = "Fonte de teste",
            Metodologia = "Consumo multiplicado pelo fator",
            ValidoDe = CalculationTimestamp.UtcDateTime.AddYears(-1),
            ValidoAte = CalculationTimestamp.UtcDateTime.AddYears(1),
            Ativo = active,
            CriadoEm = CalculationTimestamp.UtcDateTime,
            AtualizadoEm = CalculationTimestamp.UtcDateTime,
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

    private sealed record CalculationSetup(
        MongoEmissaoCarbonoService Service,
        MongoEmissaoCarbonoRepository EmissionRepository,
        MongoFatorEmissaoRepository FactorRepository,
        EmpresaDocument Company,
        ProdutoDocument Product,
        FornecedorDocument Supplier,
        FatorEmissaoDocument Factor);
}
