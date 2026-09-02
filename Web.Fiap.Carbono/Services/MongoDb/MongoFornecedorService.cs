using MongoDB.Bson;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;
using Web.Fiap.Carbono.Dtos.MongoDb.Common;
using Web.Fiap.Carbono.Dtos.MongoDb.Fornecedores;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Models.Documents.Embedded;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Services.MongoDb;

public sealed class MongoFornecedorService : IMongoFornecedorService
{
    private readonly IMongoFornecedorRepository _fornecedorRepository;
    private readonly IMongoEmissaoCarbonoRepository _emissaoRepository;

    public MongoFornecedorService(
        IMongoFornecedorRepository fornecedorRepository,
        IMongoEmissaoCarbonoRepository emissaoRepository)
    {
        _fornecedorRepository = fornecedorRepository;
        _emissaoRepository = emissaoRepository;
    }

    public async Task<FornecedorDocument> CreateAsync(
        CreateFornecedorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var now = DateTime.UtcNow;
        var document = BuildDocument(
            ObjectId.GenerateNewId(),
            request.Codigo,
            request.RazaoSocial,
            request.NomeFantasia,
            request.Cnpj,
            request.Ativo,
            request.CategoriasAtuacao,
            request.Certificacoes,
            request.IndicadoresSociais,
            request.ConformidadeAmbiental,
            request.StatusAuditoria,
            request.NivelRiscoEsg,
            null,
            null,
            now,
            now,
            1);

        try
        {
            await _fornecedorRepository.CreateAsync(
                document,
                cancellationToken);

            return document;
        }
        catch (MongoDuplicateKeyException exception)
        {
            throw new ConflictException(
                "Já existe um fornecedor com o CNPJ informado.",
                exception);
        }
    }

    public async Task<FornecedorDocument> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var objectId = MongoServiceRules.ParseObjectId(id);

        return await _fornecedorRepository.GetByIdAsync(
                   objectId.ToString(),
                   cancellationToken)
               ?? throw new NotFoundException("Fornecedor não encontrado.");
    }

    public Task<IReadOnlyList<FornecedorDocument>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _fornecedorRepository.GetAllAsync(cancellationToken);
    }

    public async Task<FornecedorDocument> UpdateAsync(
        string id,
        UpdateFornecedorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var objectId = MongoServiceRules.ParseObjectId(id);
        var current = await GetByIdAsync(id, cancellationToken);

        var updated = BuildDocument(
            current.Id,
            request.Codigo,
            request.RazaoSocial,
            request.NomeFantasia,
            request.Cnpj,
            request.Ativo,
            request.CategoriasAtuacao,
            request.Certificacoes,
            request.IndicadoresSociais,
            request.ConformidadeAmbiental,
            request.StatusAuditoria,
            request.NivelRiscoEsg,
            current.ConformidadeAmbiental?.CamposAdicionais,
            current.LegacyId,
            current.CriadoEm,
            DateTime.UtcNow,
            current.SchemaVersion);

        try
        {
            var changed = await _fornecedorRepository.UpdateAsync(
                objectId.ToString(),
                updated,
                cancellationToken);

            if (!changed)
            {
                throw new NotFoundException("Fornecedor não encontrado.");
            }

            return updated;
        }
        catch (MongoDuplicateKeyException exception)
        {
            throw new ConflictException(
                "Já existe um fornecedor com o CNPJ informado.",
                exception);
        }
    }

    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var document = await GetByIdAsync(id, cancellationToken);

        if (await _emissaoRepository.ExistsByFornecedorIdAsync(
                document.Id.ToString(),
                cancellationToken))
        {
            throw new ConflictException(
                "O fornecedor não pode ser excluído porque possui emissões.");
        }

        if (MongoServiceRules.IsTemporaryRecord(document.Codigo))
        {
            var deleted = await _fornecedorRepository.DeleteAsync(
                document.Id.ToString(),
                cancellationToken);

            if (!deleted)
            {
                throw new NotFoundException("Fornecedor não encontrado.");
            }

            return;
        }

        var deactivated = new FornecedorDocument
        {
            Id = document.Id,
            LegacyId = document.LegacyId,
            Codigo = document.Codigo,
            RazaoSocial = document.RazaoSocial,
            NomeFantasia = document.NomeFantasia,
            Cnpj = document.Cnpj,
            Ativo = false,
            CategoriasAtuacao = document.CategoriasAtuacao,
            Certificacoes = document.Certificacoes,
            IndicadoresSociais = document.IndicadoresSociais,
            ConformidadeAmbiental = document.ConformidadeAmbiental,
            StatusAuditoria = document.StatusAuditoria,
            NivelRiscoEsg = document.NivelRiscoEsg,
            CriadoEm = document.CriadoEm,
            AtualizadoEm = DateTime.UtcNow,
            SchemaVersion = document.SchemaVersion
        };

        var updated = await _fornecedorRepository.UpdateAsync(
            document.Id.ToString(),
            deactivated,
            cancellationToken);

        if (!updated)
        {
            throw new NotFoundException("Fornecedor não encontrado.");
        }
    }

    private static FornecedorDocument BuildDocument(
        ObjectId id,
        string? codigo,
        string? razaoSocial,
        string? nomeFantasia,
        string? cnpj,
        bool ativo,
        IEnumerable<string>? categoriasAtuacao,
        IEnumerable<CertificacaoDto>? certificacoes,
        IndicadoresSociaisDto? indicadoresSociais,
        ConformidadeAmbientalDto? conformidadeAmbiental,
        string? statusAuditoria,
        string? nivelRiscoEsg,
        BsonDocument? camposAmbientaisAdicionais,
        int? legacyId,
        DateTime criadoEm,
        DateTime atualizadoEm,
        int schemaVersion)
    {
        return new FornecedorDocument
        {
            Id = id,
            Codigo = MongoServiceRules.Required(
                codigo,
                "codigo",
                50),
            RazaoSocial = MongoServiceRules.Required(
                razaoSocial,
                "razaoSocial"),
            NomeFantasia = MongoServiceRules.Optional(
                nomeFantasia,
                "nomeFantasia"),
            Cnpj = MongoServiceRules.ValidateCnpj(
                cnpj),
            Ativo = ativo,
            CategoriasAtuacao =
                categoriasAtuacao?
                .Select(categoria =>
                    MongoServiceRules.Required(
                        categoria,
                        "categoriaAtuacao",
                        100))
                .ToList() ?? [],
            Certificacoes = certificacoes?
                    .Select(certificacao => new Certificacao
                    {
                        Nome = MongoServiceRules.Required(
                            certificacao.Nome,
                            "nomeCertificacao",
                            150),
                        Emissor = MongoServiceRules.Required(
                            certificacao.Emissor,
                            "emissorCertificacao",
                            150),
                        ValidaAte = certificacao.ValidaAte.HasValue
                            ? MongoServiceRules.AsUtc(
                                certificacao.ValidaAte.Value)
                            : null
                    })
                    .ToList() ?? [],
            IndicadoresSociais = indicadoresSociais is null
                ? null
                : new IndicadoresSociais
                {
                    AcidentesUltimos12Meses =
                        indicadoresSociais.AcidentesUltimos12Meses,
                    PercentualMulheresLideranca =
                        MongoServiceRules.OptionalPercentage(
                            indicadoresSociais
                                .PercentualMulheresLideranca,
                            "percentualMulheresLideranca"),
                    PossuiProgramaDiversidade =
                        indicadoresSociais.PossuiProgramaDiversidade
                },
            ConformidadeAmbiental =
                conformidadeAmbiental is null
                    ? null
                    : new ConformidadeAmbiental
                    {
                        PossuiLicenca = conformidadeAmbiental.PossuiLicenca,
                        OcorrenciasUltimos12Meses =
                            conformidadeAmbiental
                                .OcorrenciasUltimos12Meses,
                        DescarteMonitorado =
                            conformidadeAmbiental.DescarteMonitorado,
                        PercentualMaterialReciclado =
                            MongoServiceRules.OptionalPercentage(
                                conformidadeAmbiental
                                    .PercentualMaterialReciclado,
                                "percentualMaterialReciclado"),
                        DataUltimaAuditoria =
                            conformidadeAmbiental.DataUltimaAuditoria.HasValue
                                ? MongoServiceRules.AsUtc(
                                    conformidadeAmbiental
                                        .DataUltimaAuditoria.Value)
                                : null,
                        CamposAdicionais = camposAmbientaisAdicionais
                    },
            StatusAuditoria = MongoServiceRules.Optional(
                statusAuditoria,
                "statusAuditoria",
                50),
            NivelRiscoEsg = MongoServiceRules.Optional(
                nivelRiscoEsg,
                "nivelRiscoEsg",
                30),
            LegacyId = legacyId,
            CriadoEm = criadoEm,
            AtualizadoEm = atualizadoEm,
            SchemaVersion = schemaVersion
        };
    }
}
