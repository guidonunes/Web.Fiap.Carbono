using MongoDB.Bson;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Data.MongoDb.Repositories;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;
using Web.Fiap.Carbono.Exceptions;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Models.Documents.Embedded;

namespace Web.Fiap.Carbono.Tests.Data.MongoDb;

[Collection(MongoRepositoryCollection.Name)]
public sealed class MongoRepositoryIntegrationTest(
    MongoRepositoryFixture fixture
)
{
    private static readonly CancellationToken CancellationToken =
        System.Threading.CancellationToken.None;

    [Fact]
    public async Task ShouldPerformCrudForAllFiveRepositories()
    {
        var context = await fixture.CreateContextAsync();
        var empresaRepository = new MongoEmpresaRepository(context);
        var produtoRepository = new MongoProdutoRepository(context);
        var fornecedorRepository = new MongoFornecedorRepository(context);
        var fatorRepository = new MongoFatorEmissaoRepository(context);
        var emissaoRepository = new MongoEmissaoCarbonoRepository(context);

        var empresa = CreateEmpresa("CRUD-EMP", "10000000000100");
        await empresaRepository.CreateAsync(empresa, CancellationToken);
        Assert.True(
            await empresaRepository.ExistsAsync(
                empresa.Id.ToString(),
                CancellationToken
            )
        );
        Assert.Equal(
            empresa.Cnpj,
            (await empresaRepository.GetByIdAsync(
                empresa.Id.ToString(),
                CancellationToken
            ))?.Cnpj
        );
        var empresaAtualizada = CreateEmpresa(
            empresa.Codigo,
            empresa.Cnpj,
            empresa.Id,
            "Empresa atualizada"
        );
        Assert.True(
            await empresaRepository.UpdateAsync(
                empresa.Id.ToString(),
                empresaAtualizada,
                CancellationToken
            )
        );
        Assert.Equal(
            "Empresa atualizada",
            (await empresaRepository.GetByCnpjAsync(
                empresa.Cnpj,
                CancellationToken
            ))?.RazaoSocial
        );
        Assert.Single(
            await empresaRepository.GetAllAsync(CancellationToken)
        );

        var produto = CreateProduto(
            empresa.Id,
            "CRUD-PROD"
        );
        await produtoRepository.CreateAsync(produto, CancellationToken);
        Assert.True(
            await produtoRepository.ExistsAsync(
                produto.Id.ToString(),
                CancellationToken
            )
        );
        Assert.NotNull(
            await produtoRepository.GetByEmpresaIdAndCodigoAsync(
                empresa.Id.ToString(),
                produto.Codigo,
                CancellationToken
            )
        );
        var produtoAtualizado = CreateProduto(
            empresa.Id,
            produto.Codigo,
            produto.Id,
            "Produto atualizado"
        );
        Assert.True(
            await produtoRepository.UpdateAsync(
                produto.Id.ToString(),
                produtoAtualizado,
                CancellationToken
            )
        );
        Assert.Equal(
            "Produto atualizado",
            (await produtoRepository.GetByIdAsync(
                produto.Id.ToString(),
                CancellationToken
            ))?.Nome
        );
        Assert.Single(
            await produtoRepository.GetAllAsync(CancellationToken)
        );

        var fornecedor = CreateFornecedor(
            "CRUD-FORN",
            "20000000000100"
        );
        await fornecedorRepository.CreateAsync(
            fornecedor,
            CancellationToken
        );
        Assert.True(
            await fornecedorRepository.ExistsAsync(
                fornecedor.Id.ToString(),
                CancellationToken
            )
        );
        Assert.NotNull(
            await fornecedorRepository.GetByCodigoAsync(
                fornecedor.Codigo,
                CancellationToken
            )
        );
        var fornecedorAtualizado = CreateFornecedor(
            fornecedor.Codigo,
            fornecedor.Cnpj,
            fornecedor.Id,
            "Fornecedor atualizado"
        );
        Assert.True(
            await fornecedorRepository.UpdateAsync(
                fornecedor.Id.ToString(),
                fornecedorAtualizado,
                CancellationToken
            )
        );
        Assert.Equal(
            "Fornecedor atualizado",
            (await fornecedorRepository.GetByCnpjAsync(
                fornecedor.Cnpj,
                CancellationToken
            ))?.RazaoSocial
        );
        Assert.Single(
            await fornecedorRepository.GetAllAsync(CancellationToken)
        );

        var fator = CreateFator("CRUD-FATOR", 1);
        await fatorRepository.CreateAsync(fator, CancellationToken);
        Assert.True(
            await fatorRepository.ExistsAsync(
                fator.Id.ToString(),
                CancellationToken
            )
        );
        Assert.NotNull(
            await fatorRepository.GetByCodigoAndVersaoAsync(
                fator.Codigo,
                fator.Versao,
                CancellationToken
            )
        );
        var fatorAtualizado = CreateFator(
            fator.Codigo,
            fator.Versao,
            fator.Id,
            2.75m
        );
        Assert.True(
            await fatorRepository.UpdateAsync(
                fator.Id.ToString(),
                fatorAtualizado,
                CancellationToken
            )
        );
        Assert.Equal(
            2.75m,
            (await fatorRepository.GetByIdAsync(
                fator.Id.ToString(),
                CancellationToken
            ))?.Valor
        );
        Assert.Single(
            await fatorRepository.GetAllAsync(CancellationToken)
        );

        var emissao = CreateEmissao(
            "CRUD-EMISSAO",
            empresa.Id,
            produto.Id,
            fornecedor.Id,
            fator.Id,
            10m,
            Utc(2026, 6, 1),
            "TRANSPORTE",
            "ESCOPO_1"
        );
        await emissaoRepository.CreateAsync(emissao, CancellationToken);
        Assert.True(
            await emissaoRepository.ExistsAsync(
                emissao.Id.ToString(),
                CancellationToken
            )
        );
        Assert.NotNull(
            await emissaoRepository.GetByCodigoAsync(
                emissao.Codigo,
                CancellationToken
            )
        );
        var emissaoAtualizada = CreateEmissao(
            emissao.Codigo,
            empresa.Id,
            produto.Id,
            fornecedor.Id,
            fator.Id,
            12m,
            emissao.DataEmissao,
            "TRANSPORTE",
            "ESCOPO_1",
            emissao.Id
        );
        Assert.True(
            await emissaoRepository.UpdateAsync(
                emissao.Id.ToString(),
                emissaoAtualizada,
                CancellationToken
            )
        );
        Assert.Equal(
            12m,
            (await emissaoRepository.GetByIdAsync(
                emissao.Id.ToString(),
                CancellationToken
            ))?.QuantidadeEmitidaKgCO2e
        );
        Assert.Single(
            await emissaoRepository.GetAllAsync(CancellationToken)
        );

        Assert.True(
            await emissaoRepository.DeleteAsync(
                emissao.Id.ToString(),
                CancellationToken
            )
        );
        Assert.True(
            await fatorRepository.DeleteAsync(
                fator.Id.ToString(),
                CancellationToken
            )
        );
        Assert.True(
            await fornecedorRepository.DeleteAsync(
                fornecedor.Id.ToString(),
                CancellationToken
            )
        );
        Assert.True(
            await produtoRepository.DeleteAsync(
                produto.Id.ToString(),
                CancellationToken
            )
        );
        Assert.True(
            await empresaRepository.DeleteAsync(
                empresa.Id.ToString(),
                CancellationToken
            )
        );

        Assert.Null(
            await emissaoRepository.GetByIdAsync(
                emissao.Id.ToString(),
                CancellationToken
            )
        );
        Assert.False(
            await empresaRepository.DeleteAsync(
                ObjectId.GenerateNewId().ToString(),
                CancellationToken
            )
        );
    }

    [Fact]
    public async Task ShouldRejectMalformedObjectIdsBeforeQuerying()
    {
        var context = await fixture.CreateContextAsync();
        const string malformedId = "not-an-object-id";

        await Assert.ThrowsAsync<DomainValidationException>(
            () => new MongoEmpresaRepository(context).GetByIdAsync(
                malformedId,
                CancellationToken
            )
        );
        await Assert.ThrowsAsync<DomainValidationException>(
            () => new MongoProdutoRepository(context).GetByIdAsync(
                malformedId,
                CancellationToken
            )
        );
        await Assert.ThrowsAsync<DomainValidationException>(
            () => new MongoFornecedorRepository(context).GetByIdAsync(
                malformedId,
                CancellationToken
            )
        );
        await Assert.ThrowsAsync<DomainValidationException>(
            () => new MongoFatorEmissaoRepository(context).GetByIdAsync(
                malformedId,
                CancellationToken
            )
        );
        await Assert.ThrowsAsync<DomainValidationException>(
            () => new MongoEmissaoCarbonoRepository(context).GetByIdAsync(
                malformedId,
                CancellationToken
            )
        );
    }

    [Fact]
    public async Task ShouldTranslateUniqueIndexViolations()
    {
        var context = await fixture.CreateContextAsync();
        var empresaRepository = new MongoEmpresaRepository(context);
        var produtoRepository = new MongoProdutoRepository(context);
        var fornecedorRepository = new MongoFornecedorRepository(context);
        var fatorRepository = new MongoFatorEmissaoRepository(context);
        var emissaoRepository = new MongoEmissaoCarbonoRepository(context);

        var empresa = CreateEmpresa("DUP-EMP-1", "30000000000100");
        await empresaRepository.CreateAsync(empresa, CancellationToken);
        var empresaException = await Assert.ThrowsAsync<
            MongoDuplicateKeyException
        >(
            () => empresaRepository.CreateAsync(
                CreateEmpresa("DUP-EMP-2", empresa.Cnpj),
                CancellationToken
            )
        );
        Assert.IsAssignableFrom<ConflictException>(empresaException);

        var produto = CreateProduto(empresa.Id, "DUP-PROD");
        await produtoRepository.CreateAsync(produto, CancellationToken);
        await Assert.ThrowsAsync<MongoDuplicateKeyException>(
            () => produtoRepository.CreateAsync(
                CreateProduto(empresa.Id, produto.Codigo),
                CancellationToken
            )
        );

        var fornecedor = CreateFornecedor(
            "DUP-FORN-1",
            "40000000000100"
        );
        await fornecedorRepository.CreateAsync(
            fornecedor,
            CancellationToken
        );
        await Assert.ThrowsAsync<MongoDuplicateKeyException>(
            () => fornecedorRepository.CreateAsync(
                CreateFornecedor("DUP-FORN-2", fornecedor.Cnpj),
                CancellationToken
            )
        );

        var fator = CreateFator("DUP-FATOR", 1);
        await fatorRepository.CreateAsync(fator, CancellationToken);
        await Assert.ThrowsAsync<MongoDuplicateKeyException>(
            () => fatorRepository.CreateAsync(
                CreateFator(fator.Codigo, fator.Versao),
                CancellationToken
            )
        );

        var emissao = CreateEmissao(
            "DUP-EMISSAO",
            empresa.Id,
            produto.Id,
            fornecedor.Id,
            fator.Id,
            1m,
            Utc(2026, 6, 1),
            "TRANSPORTE",
            "ESCOPO_1"
        );
        await emissaoRepository.CreateAsync(emissao, CancellationToken);
        await Assert.ThrowsAsync<MongoDuplicateKeyException>(
            () => emissaoRepository.CreateAsync(
                CreateEmissao(
                    emissao.Codigo,
                    empresa.Id,
                    produto.Id,
                    fornecedor.Id,
                    fator.Id,
                    2m,
                    Utc(2026, 6, 2),
                    "ENERGIA",
                    "ESCOPO_2"
                ),
                CancellationToken
            )
        );
    }

    [Fact]
    public async Task ShouldApplyOneBasedStablePaginationAndLimits()
    {
        var context = await fixture.CreateContextAsync();
        var repository = new MongoEmissaoCarbonoRepository(context);
        var empresaId = ObjectId.GenerateNewId();
        var produtoId = ObjectId.GenerateNewId();
        var fornecedorId = ObjectId.GenerateNewId();
        var fatorId = ObjectId.GenerateNewId();

        var primeiroId = ObjectId.Parse("000000000000000000000001");
        var segundoId = ObjectId.Parse("000000000000000000000002");
        var terceiroId = ObjectId.Parse("000000000000000000000003");

        await repository.CreateAsync(
            CreateEmissao(
                "PAGE-1",
                empresaId,
                produtoId,
                fornecedorId,
                fatorId,
                1m,
                Utc(2026, 7, 1),
                "TRANSPORTE",
                "ESCOPO_1",
                primeiroId
            ),
            CancellationToken
        );
        await repository.CreateAsync(
            CreateEmissao(
                "PAGE-2",
                empresaId,
                produtoId,
                fornecedorId,
                fatorId,
                2m,
                Utc(2026, 7, 1),
                "TRANSPORTE",
                "ESCOPO_1",
                segundoId
            ),
            CancellationToken
        );
        await repository.CreateAsync(
            CreateEmissao(
                "PAGE-3",
                empresaId,
                produtoId,
                fornecedorId,
                fatorId,
                3m,
                Utc(2026, 6, 1),
                "TRANSPORTE",
                "ESCOPO_1",
                terceiroId
            ),
            CancellationToken
        );

        var firstPage = await repository.GetPaginatedAsync(
            1,
            2,
            CancellationToken
        );
        var secondPage = await repository.GetPaginatedAsync(
            2,
            2,
            CancellationToken
        );

        Assert.Equal(3, firstPage.TotalItems);
        Assert.Equal(2, firstPage.TotalPages);
        Assert.Equal(
            new[] { primeiroId, segundoId },
            firstPage.Items.Select(item => item.Id)
        );
        Assert.Equal(
            new[] { terceiroId },
            secondPage.Items.Select(item => item.Id)
        );

        var pageNumberException = await Assert.ThrowsAsync<
            MongoPaginationException
        >(
            () => repository.GetPaginatedAsync(
                0,
                10,
                CancellationToken
            )
        );
        Assert.IsAssignableFrom<DomainValidationException>(
            pageNumberException
        );
        await Assert.ThrowsAsync<MongoPaginationException>(
            () => repository.GetPaginatedAsync(
                1,
                51,
                CancellationToken
            )
        );
    }

    [Fact]
    public async Task ShouldCalculateDecimalAnalyticsWithStableRanking()
    {
        var context = await fixture.CreateContextAsync();
        var fornecedorRepository = new MongoFornecedorRepository(context);
        var repository = new MongoEmissaoCarbonoRepository(context);
        var empresaId = ObjectId.GenerateNewId();
        var produtoId = ObjectId.GenerateNewId();
        var fatorId = ObjectId.GenerateNewId();
        var fornecedorA = CreateFornecedor(
            "AN-FORN-A",
            "50000000000100",
            nome: "Fornecedor A"
        );
        var fornecedorB = CreateFornecedor(
            "AN-FORN-B",
            "60000000000100",
            nome: "Fornecedor B"
        );

        await fornecedorRepository.CreateAsync(
            fornecedorA,
            CancellationToken
        );
        await fornecedorRepository.CreateAsync(
            fornecedorB,
            CancellationToken
        );

        await repository.CreateAsync(
            CreateEmissao(
                "AN-1",
                empresaId,
                produtoId,
                fornecedorA.Id,
                fatorId,
                100.10m,
                Utc(2026, 6, 1),
                "TRANSPORTE",
                "ESCOPO_1"
            ),
            CancellationToken
        );
        await repository.CreateAsync(
            CreateEmissao(
                "AN-2",
                empresaId,
                produtoId,
                fornecedorA.Id,
                fatorId,
                25.20m,
                Utc(2026, 7, 1),
                "ENERGIA",
                "ESCOPO_2"
            ),
            CancellationToken
        );
        await repository.CreateAsync(
            CreateEmissao(
                "AN-3",
                empresaId,
                produtoId,
                fornecedorB.Id,
                fatorId,
                75.30m,
                Utc(2026, 6, 2),
                "TRANSPORTE",
                "ESCOPO_1"
            ),
            CancellationToken
        );

        var footprint = await repository.GetProductFootprintAsync(
            produtoId.ToString(),
            CancellationToken
        );
        Assert.NotNull(footprint);
        Assert.Equal(200.60m, footprint.TotalKgCO2e);
        Assert.Equal(
            new[]
            {
                ("ENERGIA", 25.20m),
                ("TRANSPORTE", 175.40m)
            },
            footprint.PorEtapa.Select(
                item => (item.Categoria, item.TotalKgCO2e)
            )
        );

        var ranking = await repository.GetSupplierRankingAsync(
            1,
            1,
            CancellationToken
        );
        Assert.Equal(2, ranking.TotalItems);
        Assert.Equal(2, ranking.TotalPages);
        var firstSupplier = Assert.Single(ranking.Items);
        Assert.Equal(fornecedorA.Id, firstSupplier.FornecedorId);
        Assert.Equal(fornecedorA.Codigo, firstSupplier.Codigo);
        Assert.Equal(fornecedorA.NomeFantasia, firstSupplier.Nome);
        Assert.Equal(125.30m, firstSupplier.TotalKgCO2e);
        Assert.Equal(2, firstSupplier.QuantidadeEmissoes);

        var dashboard = await repository.GetCompanyDashboardAsync(
            empresaId.ToString(),
            CancellationToken
        );
        Assert.NotNull(dashboard);
        Assert.Equal(200.60m, dashboard.TotalKgCO2e);
        Assert.Equal(3, dashboard.QuantidadeEmissoes);
        Assert.Equal(
            new[] { (2026, 6, 175.40m), (2026, 7, 25.20m) },
            dashboard.PorMes.Select(
                item => (item.Ano, item.Mes, item.TotalKgCO2e)
            )
        );
        Assert.Equal(
            new[] { ("ESCOPO_1", 175.40m), ("ESCOPO_2", 25.20m) },
            dashboard.PorEscopo.Select(
                item => (item.Escopo, item.TotalKgCO2e)
            )
        );
    }

    private static EmpresaDocument CreateEmpresa(
        string codigo,
        string cnpj,
        ObjectId? id = null,
        string nome = "Empresa de teste"
    )
    {
        return new EmpresaDocument
        {
            Id = id ?? ObjectId.GenerateNewId(),
            Codigo = codigo,
            RazaoSocial = nome,
            NomeFantasia = nome,
            Cnpj = cnpj,
            Ativa = true,
            CriadoEm = Utc(2026, 1, 1),
            AtualizadoEm = Utc(2026, 1, 1),
            SchemaVersion = 1
        };
    }

    private static ProdutoDocument CreateProduto(
        ObjectId empresaId,
        string codigo,
        ObjectId? id = null,
        string nome = "Produto de teste"
    )
    {
        return new ProdutoDocument
        {
            Id = id ?? ObjectId.GenerateNewId(),
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

    private static FornecedorDocument CreateFornecedor(
        string codigo,
        string cnpj,
        ObjectId? id = null,
        string nome = "Fornecedor de teste"
    )
    {
        return new FornecedorDocument
        {
            Id = id ?? ObjectId.GenerateNewId(),
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

    private static FatorEmissaoDocument CreateFator(
        string codigo,
        int versao,
        ObjectId? id = null,
        decimal valor = 1.25m
    )
    {
        return new FatorEmissaoDocument
        {
            Id = id ?? ObjectId.GenerateNewId(),
            Codigo = codigo,
            Nome = "Fator de teste",
            Categoria = "TRANSPORTE",
            Valor = valor,
            UnidadeBase = "km",
            Escopo = "ESCOPO_1",
            Versao = versao,
            ValidoDe = Utc(2026, 1, 1),
            Ativo = true,
            CriadoEm = Utc(2026, 1, 1),
            AtualizadoEm = Utc(2026, 1, 1),
            SchemaVersion = 1
        };
    }

    private static EmissaoCarbonoDocument CreateEmissao(
        string codigo,
        ObjectId empresaId,
        ObjectId produtoId,
        ObjectId fornecedorId,
        ObjectId fatorId,
        decimal quantidadeEmitida,
        DateTime dataEmissao,
        string categoria,
        string escopo,
        ObjectId? id = null
    )
    {
        return new EmissaoCarbonoDocument
        {
            Id = id ?? ObjectId.GenerateNewId(),
            Codigo = codigo,
            EmpresaId = empresaId,
            ProdutoId = produtoId,
            FornecedorId = fornecedorId,
            FatorEmissaoId = fatorId,
            Lote = new LoteSnapshot
            {
                Codigo = $"LOTE-{codigo}",
                QuantidadeProduzida = 1m,
                Unidade = "unidade",
                DataProducao = dataEmissao
            },
            Etapa = new EtapaSnapshot
            {
                Nome = categoria,
                Ordem = 1,
                Categoria = categoria
            },
            QuantidadeAtividade = quantidadeEmitida,
            DadosAtividade = new BsonDocument(
                "tipo",
                categoria
            ),
            FatorAplicado = new FatorEmissaoSnapshot
            {
                Codigo = "FATOR-TESTE",
                Nome = "Fator de teste",
                Valor = 1m,
                UnidadeBase = "unidade",
                Escopo = escopo,
                Versao = 1
            },
            QuantidadeEmitidaKgCO2e = quantidadeEmitida,
            MetodoCalculo =
                "quantidadeAtividade * fatorAplicado.valor",
            CalculadoPor = "integration-test",
            DataEmissao = dataEmissao,
            CriadoEm = dataEmissao,
            AtualizadoEm = dataEmissao,
            SchemaVersion = 1
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
            DateTimeKind.Utc
        );
    }
}
