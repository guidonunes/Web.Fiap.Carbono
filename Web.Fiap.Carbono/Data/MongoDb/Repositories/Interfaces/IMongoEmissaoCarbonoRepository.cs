using Web.Fiap.Carbono.Data.MongoDb.Repositories;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Projections;
using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;

public interface IMongoEmissaoCarbonoRepository
{
    Task<EmissaoCarbonoDocument> CreateAsync(
        EmissaoCarbonoDocument document,
        CancellationToken cancellationToken
    );

    Task<EmissaoCarbonoDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken
    );

    Task<EmissaoCarbonoDocument?> GetByLegacyIdAsync(
        int legacyId,
        CancellationToken cancellationToken
    );

    Task<EmissaoCarbonoDocument?> GetByCodigoAsync(
        string codigo,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<EmissaoCarbonoDocument>> GetAllAsync(
        CancellationToken cancellationToken
    );

    Task<bool> UpdateAsync(
        string id,
        EmissaoCarbonoDocument document,
        CancellationToken cancellationToken
    );

    Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    );

    Task<bool> ExistsAsync(
        string id,
        CancellationToken cancellationToken
    );

    Task<MongoPagedResult<EmissaoCarbonoDocument>>
        GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken
        );

    Task<ProdutoPegadaMongoResult?> GetProductFootprintAsync(
        string produtoId,
        CancellationToken cancellationToken
    );

    Task<MongoPagedResult<FornecedorRankingMongoResult>>
        GetSupplierRankingAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken
        );

    Task<DashboardEmpresaMongoResult?>
        GetCompanyDashboardAsync(
            string empresaId,
            CancellationToken cancellationToken
        );

    Task<bool> ExistsByEmpresaIdAsync(
        string empresaId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByProdutoIdAsync(
        string produtoId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByFornecedorIdAsync(
        string fornecedorId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByFatorEmissaoIdAsync(
        string fatorEmissaoId,
        CancellationToken cancellationToken = default);
}
