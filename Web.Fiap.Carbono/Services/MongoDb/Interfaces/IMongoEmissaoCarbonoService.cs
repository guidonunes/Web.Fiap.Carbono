using Web.Fiap.Carbono.Data.MongoDb.Repositories;
using Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;
using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Services.MongoDb.Interfaces;

public interface IMongoEmissaoCarbonoService
{
    Task<EmissaoCarbonoDocument> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<MongoPagedResult<EmissaoCarbonoDocument>> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<EmissaoCarbonoDocument> UpdateAuditAsync(
        string id,
        string observacao,
        CancellationToken cancellationToken = default);

    Task DeleteTemporaryAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<EmissaoCarbonoDocument> CalculateAsync(
        CalcularEmissaoMongoRequest request,
        string calculatedBy,
        CancellationToken cancellationToken = default);
}
