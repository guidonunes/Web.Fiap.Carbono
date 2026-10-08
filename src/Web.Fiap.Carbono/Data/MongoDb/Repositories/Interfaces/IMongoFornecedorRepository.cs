using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;

public interface IMongoFornecedorRepository
{
    Task<FornecedorDocument> CreateAsync(
        FornecedorDocument document,
        CancellationToken cancellationToken
    );

    Task<FornecedorDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken
    );

    Task<FornecedorDocument?> GetByCodigoAsync(
        string codigo,
        CancellationToken cancellationToken
    );

    Task<FornecedorDocument?> GetByCnpjAsync(
        string cnpj,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<FornecedorDocument>> GetAllAsync(
        CancellationToken cancellationToken
    );

    Task<bool> UpdateAsync(
        string id,
        FornecedorDocument document,
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
