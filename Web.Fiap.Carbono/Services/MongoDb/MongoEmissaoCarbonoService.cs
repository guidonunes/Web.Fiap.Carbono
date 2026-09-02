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

    public MongoEmissaoCarbonoService(
        IMongoEmissaoCarbonoRepository repository)
    {
        _repository = repository;
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
            RevisadoEm = document.RevisadoEm,
            RevisadoPor = document.RevisadoPor,
            CriadoEm = document.CriadoEm,
            AtualizadoEm = DateTime.UtcNow,
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
}
