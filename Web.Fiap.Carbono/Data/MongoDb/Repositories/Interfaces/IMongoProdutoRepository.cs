using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;

public interface IMongoProdutoRepository
{
    Task<ProdutoDocument> CreateAsync(
        ProdutoDocument document,
        CancellationToken cancellationToken
    );

    Task<ProdutoDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken
    );

    Task<ProdutoDocument?> GetByEmpresaIdAndCodigoAsync(
        string empresaId,
        string codigo,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<ProdutoDocument>> GetAllAsync(
        CancellationToken cancellationToken
    );

    Task<bool> UpdateAsync(
        string id,
        ProdutoDocument document,
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
}
