using MongoDB.Bson;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;
using Web.Fiap.Carbono.Dtos.MongoDb.FatoresEmissao;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Services.MongoDb;

public sealed class MongoFatorEmissaoService : IMongoFatorEmissaoService
{
    private readonly IMongoFatorEmissaoRepository _fatorRepository;
    private readonly IMongoEmissaoCarbonoRepository _emissaoRepository;

    public MongoFatorEmissaoService(
        IMongoFatorEmissaoRepository fatorRepository,
        IMongoEmissaoCarbonoRepository emissaoRepository)
    {
        _fatorRepository = fatorRepository;
        _emissaoRepository = emissaoRepository;
    }

    public async Task<FatorEmissaoDocument> CreateAsync(
        CreateFatorEmissaoRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var now = DateTime.UtcNow;
        var document = BuildDocument(
            ObjectId.GenerateNewId(),
            request.Codigo,
            request.Nome,
            request.Categoria,
            request.Valor,
            request.UnidadeBase,
            request.Escopo,
            request.Versao,
            request.FonteReferencia,
            request.Metodologia,
            request.ValidoDe,
            request.ValidoAte,
            request.Ativo,
            null,
            now,
            now,
            1);

        try
        {
            await _fatorRepository.CreateAsync(document, cancellationToken);
            return document;
        }
        catch (MongoDuplicateKeyException exception)
        {
            throw new ConflictException(
                "Já existe um fator com esse código e versão.",
                exception);
        }
    }

    public async Task<FatorEmissaoDocument> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var objectId = MongoServiceRules.ParseObjectId(id);

        return await _fatorRepository.GetByIdAsync(
                   objectId.ToString(),
                   cancellationToken)
               ?? throw new NotFoundException(
                   "Fator de emissão não encontrado.");
    }

    public Task<IReadOnlyList<FatorEmissaoDocument>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _fatorRepository.GetAllAsync(cancellationToken);
    }

    public async Task<FatorEmissaoDocument> UpdateAsync(
        string id,
        UpdateFatorEmissaoRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var objectId = MongoServiceRules.ParseObjectId(id);
        var current = await GetByIdAsync(id, cancellationToken);

        var updated = BuildDocument(
            current.Id,
            request.Codigo,
            request.Nome,
            request.Categoria,
            request.Valor,
            request.UnidadeBase,
            request.Escopo,
            request.Versao,
            request.FonteReferencia,
            request.Metodologia,
            request.ValidoDe,
            request.ValidoAte,
            request.Ativo,
            current.LegacyId,
            current.CriadoEm,
            DateTime.UtcNow,
            current.SchemaVersion);

        try
        {
            var changed = await _fatorRepository.UpdateAsync(
                objectId.ToString(),
                updated,
                cancellationToken);

            if (!changed)
            {
                throw new NotFoundException(
                    "Fator de emissão não encontrado.");
            }

            return updated;
        }
        catch (MongoDuplicateKeyException exception)
        {
            throw new ConflictException(
                "Já existe um fator com esse código e versão.",
                exception);
        }
    }

    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var document = await GetByIdAsync(id, cancellationToken);

        if (await _emissaoRepository.ExistsByFatorEmissaoIdAsync(
                document.Id.ToString(),
                cancellationToken))
        {
            throw new ConflictException(
                "O fator não pode ser excluído porque possui emissões.");
        }

        if (MongoServiceRules.IsTemporaryRecord(document.Codigo))
        {
            var deleted = await _fatorRepository.DeleteAsync(
                document.Id.ToString(),
                cancellationToken);

            if (!deleted)
            {
                throw new NotFoundException(
                    "Fator de emissão não encontrado.");
            }

            return;
        }

        var deactivated = new FatorEmissaoDocument
        {
            Id = document.Id,
            LegacyId = document.LegacyId,
            Codigo = document.Codigo,
            Nome = document.Nome,
            Categoria = document.Categoria,
            Valor = document.Valor,
            UnidadeBase = document.UnidadeBase,
            Escopo = document.Escopo,
            Versao = document.Versao,
            FonteReferencia = document.FonteReferencia,
            Metodologia = document.Metodologia,
            ValidoDe = document.ValidoDe,
            ValidoAte = document.ValidoAte,
            Ativo = false,
            CriadoEm = document.CriadoEm,
            AtualizadoEm = DateTime.UtcNow,
            SchemaVersion = document.SchemaVersion
        };

        var updated = await _fatorRepository.UpdateAsync(
            document.Id.ToString(),
            deactivated,
            cancellationToken);

        if (!updated)
        {
            throw new NotFoundException(
                "Fator de emissão não encontrado.");
        }
    }

    private static FatorEmissaoDocument BuildDocument(
        ObjectId id,
        string? codigo,
        string? nome,
        string? categoria,
        decimal valor,
        string? unidadeBase,
        string? escopo,
        int versao,
        string? fonteReferencia,
        string? metodologia,
        DateTime validoDeValue,
        DateTime? validoAteValue,
        bool ativo,
        int? legacyId,
        DateTime criadoEm,
        DateTime atualizadoEm,
        int schemaVersion)
    {
        var validoDe = MongoServiceRules.AsUtc(validoDeValue);

        DateTime? validoAte = validoAteValue.HasValue
            ? MongoServiceRules.AsUtc(validoAteValue.Value)
            : null;

        if (validoAte.HasValue && validoAte.Value < validoDe)
        {
            throw new BadRequestException(
                "A validade final deve ser posterior à validade inicial.");
        }

        if (versao < 1)
        {
            throw new BadRequestException(
                "A versão do fator deve ser maior ou igual a 1.");
        }

        return new FatorEmissaoDocument
        {
            Id = id,
            Codigo = MongoServiceRules.Required(
                codigo,
                "codigo",
                100),
            Nome = MongoServiceRules.Required(
                nome,
                "nome"),
            Categoria = MongoServiceRules.Required(
                categoria,
                "categoria",
                100),
            Valor = MongoServiceRules.NonNegative(
                valor,
                "valor"),
            UnidadeBase = MongoServiceRules.Required(
                unidadeBase,
                "unidadeBase",
                50),
            Escopo = MongoServiceRules.ValidateScope(
                escopo),
            Versao = versao,
            FonteReferencia = MongoServiceRules.Optional(
                fonteReferencia,
                "fonteReferencia",
                300),
            Metodologia = MongoServiceRules.Optional(
                metodologia,
                "metodologia",
                500),
            ValidoDe = validoDe,
            ValidoAte = validoAte,
            Ativo = ativo,
            LegacyId = legacyId,
            CriadoEm = criadoEm,
            AtualizadoEm = atualizadoEm,
            SchemaVersion = schemaVersion
        };
    }
}
