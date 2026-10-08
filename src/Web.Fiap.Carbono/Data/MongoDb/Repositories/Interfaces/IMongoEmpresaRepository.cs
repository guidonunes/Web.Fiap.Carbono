using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;

public interface IMongoEmpresaRepository
{
    Task<EmpresaDocument> CreateAsync(
        EmpresaDocument document,
        CancellationToken cancellationToken
    );

    Task<EmpresaDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken
    );

    Task<EmpresaDocument?> GetByCnpjAsync(
        string cnpj,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<EmpresaDocument>> GetAllAsync(
        CancellationToken cancellationToken
    );

    Task<bool> UpdateAsync(
        string id,
        EmpresaDocument document,
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
