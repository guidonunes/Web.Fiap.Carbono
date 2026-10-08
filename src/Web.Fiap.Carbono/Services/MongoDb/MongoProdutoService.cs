using MongoDB.Bson;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;
using Web.Fiap.Carbono.Dtos.MongoDb.Analytics;
using Web.Fiap.Carbono.Dtos.MongoDb.Common;
using Web.Fiap.Carbono.Dtos.MongoDb.Produtos;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Models.Documents.Embedded;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Services.MongoDb;

public sealed class MongoProdutoService : IMongoProdutoService
{
    private readonly IMongoProdutoRepository _produtoRepository;
    private readonly IMongoEmpresaRepository _empresaRepository;
    private readonly IMongoEmissaoCarbonoRepository _emissaoRepository;

    public MongoProdutoService(
        IMongoProdutoRepository produtoRepository,
        IMongoEmpresaRepository empresaRepository,
        IMongoEmissaoCarbonoRepository emissaoRepository)
    {
        _produtoRepository = produtoRepository;
        _empresaRepository = empresaRepository;
        _emissaoRepository = emissaoRepository;
    }

    public async Task<ProdutoDocument> CreateAsync(
        CreateProdutoRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var empresaId = MongoServiceRules.ParseObjectId(
            request.EmpresaId,
            "empresaId");

        await EnsureCompanyExistsAsync(empresaId, cancellationToken);

        var now = DateTime.UtcNow;

        var document = BuildDocument(
            ObjectId.GenerateNewId(),
            empresaId,
            request.Codigo,
            request.Nome,
            request.Categoria,
            request.UnidadeFuncional,
            request.Ativo,
            request.AtributosAmbientais,
            null,
            null,
            now,
            now,
            1);

        try
        {
            await _produtoRepository.CreateAsync(document, cancellationToken);
            return document;
        }
        catch (MongoDuplicateKeyException exception)
        {
            throw new ConflictException(
                "Já existe um produto com esse código para a empresa.",
                exception);
        }
    }

    public async Task<ProdutoDocument> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var objectId = MongoServiceRules.ParseObjectId(id);

        return await _produtoRepository.GetByIdAsync(
                   objectId.ToString(),
                   cancellationToken)
               ?? throw new NotFoundException("Produto não encontrado.");
    }

    public Task<IReadOnlyList<ProdutoDocument>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _produtoRepository.GetAllAsync(cancellationToken);
    }

    public async Task<ProdutoDocument> UpdateAsync(
        string id,
        UpdateProdutoRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var objectId = MongoServiceRules.ParseObjectId(id);
        var current = await GetByIdAsync(id, cancellationToken);

        var empresaId = MongoServiceRules.ParseObjectId(
            request.EmpresaId,
            "empresaId");

        await EnsureCompanyExistsAsync(empresaId, cancellationToken);

        var updated = BuildDocument(
            current.Id,
            empresaId,
            request.Codigo,
            request.Nome,
            request.Categoria,
            request.UnidadeFuncional,
            request.Ativo,
            request.AtributosAmbientais,
            current.AtributosAmbientais?.CamposAdicionais,
            current.LegacyId,
            current.CriadoEm,
            DateTime.UtcNow,
            current.SchemaVersion);

        try
        {
            var changed = await _produtoRepository.UpdateAsync(
                objectId.ToString(),
                updated,
                cancellationToken);

            if (!changed)
            {
                throw new NotFoundException("Produto não encontrado.");
            }

            return updated;
        }
        catch (MongoDuplicateKeyException exception)
        {
            throw new ConflictException(
                "Já existe um produto com esse código para a empresa.",
                exception);
        }
    }

    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var document = await GetByIdAsync(id, cancellationToken);

        if (await _emissaoRepository.ExistsByProdutoIdAsync(
                document.Id.ToString(),
                cancellationToken))
        {
            throw new ConflictException(
                "O produto não pode ser excluído porque possui emissões.");
        }

        if (MongoServiceRules.IsTemporaryRecord(document.Codigo))
        {
            var deleted = await _produtoRepository.DeleteAsync(
                document.Id.ToString(),
                cancellationToken);

            if (!deleted)
            {
                throw new NotFoundException("Produto não encontrado.");
            }

            return;
        }

        var deactivated = new ProdutoDocument
        {
            Id = document.Id,
            LegacyId = document.LegacyId,
            EmpresaId = document.EmpresaId,
            Codigo = document.Codigo,
            Nome = document.Nome,
            Categoria = document.Categoria,
            UnidadeFuncional = document.UnidadeFuncional,
            Ativo = false,
            AtributosAmbientais = document.AtributosAmbientais,
            CriadoEm = document.CriadoEm,
            AtualizadoEm = DateTime.UtcNow,
            SchemaVersion = document.SchemaVersion
        };

        var updated = await _produtoRepository.UpdateAsync(
            document.Id.ToString(),
            deactivated,
            cancellationToken);

        if (!updated)
        {
            throw new NotFoundException("Produto não encontrado.");
        }
    }

    public async Task<ProdutoPegadaMongoResponse> GetFootprintAsync(
        string produtoId,
        CancellationToken cancellationToken = default)
    {
        var produto = await GetByIdAsync(
            produtoId,
            cancellationToken);

        var empresa = await _empresaRepository.GetByIdAsync(
                          produto.EmpresaId.ToString(),
                          cancellationToken)
                      ?? throw new NotFoundException(
                          "A empresa vinculada ao produto não foi encontrada.");

        var pegada = await _emissaoRepository.GetProductFootprintAsync(
            produto.Id.ToString(),
            cancellationToken);

        return new ProdutoPegadaMongoResponse
        {
            IdProduto = produto.Id.ToString(),
            NomeProduto = produto.Nome,
            NomeEmpresa =
                empresa.NomeFantasia ?? empresa.RazaoSocial,
            TotalCo2e = pegada?.TotalKgCO2e ?? 0m,
            Unidade = "kgCO2e",
            EmissoesPorEtapa = pegada is null
                ? Array.Empty<EmissaoPorEtapaMongoResponse>()
                : pegada.PorEtapa
                    .Select(item => new EmissaoPorEtapaMongoResponse
                    {
                        TipoEtapa = item.Categoria,
                        TotalCo2e = item.TotalKgCO2e
                    })
                    .ToList()
        };
    }

    private async Task EnsureCompanyExistsAsync(
        ObjectId empresaId,
        CancellationToken cancellationToken)
    {
        if (!await _empresaRepository.ExistsAsync(
                empresaId.ToString(),
                cancellationToken))
        {
            throw new NotFoundException(
                "A empresa informada não existe.");
        }
    }

    private static ProdutoDocument BuildDocument(
        ObjectId id,
        ObjectId empresaId,
        string? codigo,
        string? nome,
        string? categoria,
        string? unidadeFuncional,
        bool ativo,
        AtributosAmbientaisDto? atributos,
        BsonDocument? camposAdicionais,
        int? legacyId,
        DateTime criadoEm,
        DateTime atualizadoEm,
        int schemaVersion)
    {
        return new ProdutoDocument
        {
            Id = id,
            EmpresaId = empresaId,
            Codigo = MongoServiceRules.Required(codigo, "codigo", 50),
            Nome = MongoServiceRules.Required(nome, "nome"),
            Categoria = MongoServiceRules.Optional(
                categoria,
                "categoria",
                100),
            UnidadeFuncional = MongoServiceRules.Required(
                unidadeFuncional,
                "unidadeFuncional",
                50),
            Ativo = ativo,
            AtributosAmbientais = atributos is null
                ? null
                : new AtributosAmbientais
                {
                    PercentualReciclavel =
                        MongoServiceRules.OptionalPercentage(
                            atributos.PercentualReciclavel,
                            "percentualReciclavel"),
                    PercentualMaterialReciclado =
                        MongoServiceRules.OptionalPercentage(
                            atributos.PercentualMaterialReciclado,
                            "percentualMaterialReciclado"),
                    Embalagem = atributos.Embalagem,
                    SeloSustentabilidade =
                        atributos.SeloSustentabilidade,
                    CompensacaoCarbono = atributos.CompensacaoCarbono,
                    OrigemAgriculturaOrganica =
                        atributos.OrigemAgriculturaOrganica,
                    VidaUtilAnos = atributos.VidaUtilAnos,
                    CamposAdicionais = camposAdicionais,
                    Materiais = atributos.Materiais
                        .Select(material => new MaterialProduto
                        {
                            Nome = MongoServiceRules.Required(
                                material.Nome,
                                "nomeMaterial",
                                150),
                            PercentualComposicao =
                                MongoServiceRules.Percentage(
                                    material.PercentualComposicao,
                                    "percentualComposicao"),
                            OrigemRenovavel = material.OrigemRenovavel,
                            OrigemReciclada = material.OrigemReciclada
                        })
                        .ToList()
                },
            LegacyId = legacyId,
            CriadoEm = criadoEm,
            AtualizadoEm = atualizadoEm,
            SchemaVersion = schemaVersion
        };
    }
}
