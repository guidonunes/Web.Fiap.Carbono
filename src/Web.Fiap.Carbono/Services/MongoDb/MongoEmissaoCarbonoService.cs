using MongoDB.Bson;
using Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;
using Web.Fiap.Carbono.Data.MongoDb.Repositories;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Services.MongoDb;

public sealed class MongoEmissaoCarbonoService
    : IMongoEmissaoCarbonoService
{
    private const string TemporaryCode = "CRUD-TEMP-EMISSAO";

    private readonly IMongoEmissaoCarbonoRepository _repository;
    private readonly IMongoProdutoRepository _produtoRepository;
    private readonly IMongoEmpresaRepository _empresaRepository;
    private readonly IMongoFornecedorRepository _fornecedorRepository;
    private readonly IMongoFatorEmissaoRepository _fatorRepository;
    private readonly TimeProvider _timeProvider;

    public MongoEmissaoCarbonoService(
        IMongoEmissaoCarbonoRepository repository,
        IMongoProdutoRepository produtoRepository,
        IMongoEmpresaRepository empresaRepository,
        IMongoFornecedorRepository fornecedorRepository,
        IMongoFatorEmissaoRepository fatorRepository,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _produtoRepository = produtoRepository;
        _empresaRepository = empresaRepository;
        _fornecedorRepository = fornecedorRepository;
        _fatorRepository = fatorRepository;
        _timeProvider = timeProvider
            ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    internal DateTime GetCalculationTimestampUtc()
    {
        return _timeProvider.GetUtcNow().UtcDateTime;
    }

    internal async Task<CalculationReferences> LoadReferencesAsync(
        CalcularEmissaoMongoRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var produtoId = MongoServiceRules.ParseObjectId(
            request.ProdutoId,
            "produtoId");

        var fornecedorId = MongoServiceRules.ParseObjectId(
            request.FornecedorId,
            "fornecedorId");

        var fatorId = MongoServiceRules.ParseObjectId(
            request.FatorEmissaoId,
            "fatorEmissaoId");

        // Validate all public IDs before accessing MongoDB so malformed IDs
        // consistently produce a 400 response.
        var produtoTask = _produtoRepository.GetByIdAsync(
            produtoId.ToString(),
            cancellationToken);

        var fornecedorTask = _fornecedorRepository.GetByIdAsync(
            fornecedorId.ToString(),
            cancellationToken);

        var fatorTask = _fatorRepository.GetByIdAsync(
            fatorId.ToString(),
            cancellationToken);

        await Task.WhenAll(
            produtoTask,
            fornecedorTask,
            fatorTask);

        var produto = await produtoTask
            ?? throw new NotFoundException(
                "Produto não encontrado.");

        var fornecedor = await fornecedorTask
            ?? throw new NotFoundException(
                "Fornecedor não encontrado.");

        var fator = await fatorTask
            ?? throw new NotFoundException(
                "Fator de emissão não encontrado.");

        // empresaId comes from the product so the caller cannot submit an
        // inconsistent product/company pair.
        var empresa = await _empresaRepository.GetByIdAsync(
            produto.EmpresaId.ToString(),
            cancellationToken)
            ?? throw new NotFoundException(
                "A empresa associada ao produto não foi encontrada.");

        return new CalculationReferences(
            empresa,
            produto,
            fornecedor,
            fator);
    }

    public async Task<EmissaoCarbonoDocument> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var objectId = MongoServiceRules.ParseObjectId(id);

        return await _repository.GetByIdAsync(
                   objectId.ToString(),
                   cancellationToken)
               ?? throw new NotFoundException(
                   "Emissão de carbono não encontrada.");
    }

    public async Task<EmissaoCarbonoDocument> GetByLegacyIdAsync(
        int legacyId,
        CancellationToken cancellationToken = default)
    {
        if (legacyId <= 0)
        {
            throw new BadRequestException(
                "idEmissao deve ser maior que zero.");
        }

        return await _repository.GetByLegacyIdAsync(
                   legacyId,
                   cancellationToken)
               ?? throw new NotFoundException(
                   "Emissão de carbono não encontrada.");
    }

    public Task<MongoPagedResult<EmissaoCarbonoDocument>>
        GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
        {
            throw new BadRequestException(
                "pageNumber deve ser maior ou igual a 1.");
        }

        if (pageSize is < 1 or > 50)
        {
            throw new BadRequestException(
                "pageSize deve estar entre 1 e 50.");
        }

        return _repository.GetPaginatedAsync(
            pageNumber,
            pageSize,
            cancellationToken);
    }

    public async Task<EmissaoCarbonoDocument> UpdateAuditAsync(
        string id,
        string observacao,
        string revisadoPor,
        CancellationToken cancellationToken = default)
    {
        var document = await GetByIdAsync(id, cancellationToken);

        if (!string.Equals(
                document.Codigo,
                TemporaryCode,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                "Emissões calculadas são registros históricos imutáveis.");
        }

        var revisionTimestamp = GetCalculationTimestampUtc();
        var revised = new EmissaoCarbonoDocument
        {
            Id = document.Id,
            LegacyId = document.LegacyId,
            Codigo = document.Codigo,
            EmpresaId = document.EmpresaId,
            ProdutoId = document.ProdutoId,
            FornecedorId = document.FornecedorId,
            FatorEmissaoId = document.FatorEmissaoId,
            Lote = document.Lote,
            Etapa = document.Etapa,
            QuantidadeAtividade = document.QuantidadeAtividade,
            DadosAtividade = document.DadosAtividade,
            FatorAplicado = document.FatorAplicado,
            QuantidadeEmitidaKgCO2e =
                document.QuantidadeEmitidaKgCO2e,
            MetodoCalculo = document.MetodoCalculo,
            FonteEmissao = document.FonteEmissao,
            Observacao = MongoServiceRules.Required(
                observacao,
                "observacao",
                1000),
            CalculadoPor = document.CalculadoPor,
            DataEmissao = document.DataEmissao,
            RevisadoEm = revisionTimestamp,
            RevisadoPor = EmissaoCalculationRules.ValidateCalculatedBy(revisadoPor),
            CriadoEm = document.CriadoEm,
            AtualizadoEm = revisionTimestamp,
            SchemaVersion = document.SchemaVersion
        };

        var updated = await _repository.UpdateAsync(
            document.Id.ToString(),
            revised,
            cancellationToken);

        if (!updated)
        {
            throw new NotFoundException(
                "Emissão de carbono não encontrada.");
        }

        return revised;
    }

    public async Task DeleteTemporaryAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var document = await GetByIdAsync(id, cancellationToken);

        if (!string.Equals(
                document.Codigo,
                TemporaryCode,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException(
                "Somente a emissão acadêmica CRUD-TEMP-EMISSAO pode ser removida.");
        }

        var deleted = await _repository.DeleteAsync(
            document.Id.ToString(),
            cancellationToken);

        if (!deleted)
        {
            throw new NotFoundException(
                "Emissão de carbono não encontrada.");
        }
    }

    public async Task<EmissaoCarbonoDocument> CalculateAsync(
        CalcularEmissaoMongoRequest request,
        string calculatedBy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var authenticatedUser =
            EmissaoCalculationRules.ValidateCalculatedBy(
                calculatedBy);

        var calculationTimestampUtc =
            GetCalculationTimestampUtc();

        var references = await LoadReferencesAsync(
            request,
            cancellationToken);

        EmissaoCalculationRules.ValidateFactorAvailability(
            references.Fator,
            calculationTimestampUtc);

        EmissaoCalculationRules.ValidateActivity(
            request,
            references.Fator);

        var factorSnapshot =
            EmissaoSnapshotFactory.CreateFactorSnapshot(
                references.Fator);

        var emittedQuantityKgCO2e =
            EmissaoCalculationRules.CalculateKgCO2e(
                request.QuantidadeAtividade,
                factorSnapshot.Valor);

        var documentId = ObjectId.GenerateNewId();

        var document = new EmissaoCarbonoDocument
        {
            Id = documentId,
            LegacyId = null,

            // The generated ObjectId makes this business/audit code unique.
            Codigo =
                $"EMI-{documentId.ToString().ToUpperInvariant()}",

            EmpresaId = references.Empresa.Id,
            ProdutoId = references.Produto.Id,
            FornecedorId = references.Fornecedor.Id,
            FatorEmissaoId = references.Fator.Id,

            Lote = EmissaoSnapshotFactory.CreateLoteSnapshot(
                request.Lote),

            Etapa = EmissaoSnapshotFactory.CreateEtapaSnapshot(
                request.Etapa),

            QuantidadeAtividade = request.QuantidadeAtividade,

            DadosAtividade =
                EmissaoSnapshotFactory.CreateActivityData(
                    request.DadosAtividade),

            FatorAplicado = factorSnapshot,

            QuantidadeEmitidaKgCO2e =
                emittedQuantityKgCO2e,

            MetodoCalculo =
                "quantidadeAtividade × fatorAplicado.valor",

            FonteEmissao = MongoServiceRules.Optional(
                request.FonteEmissao,
                "fonteEmissao",
                200),

            Observacao = MongoServiceRules.Optional(
                request.Observacao,
                "observacao",
                1000),

            CalculadoPor = authenticatedUser,

            DataEmissao = calculationTimestampUtc,
            CriadoEm = calculationTimestampUtc,
            AtualizadoEm = calculationTimestampUtc,

            RevisadoEm = null,
            RevisadoPor = null,

            SchemaVersion = 1
        };

        return await _repository.CreateAsync(
            document,
            cancellationToken);
    }

    internal sealed record CalculationReferences(
        EmpresaDocument Empresa,
        ProdutoDocument Produto,
        FornecedorDocument Fornecedor,
        FatorEmissaoDocument Fator);
}
