using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;

public interface IMongoFatorEmissaoRepository
{
    Task<FatorEmissaoDocument> CreateAsync(
        FatorEmissaoDocument document,
        CancellationToken cancellationToken
    );

    Task<FatorEmissaoDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken
    );

    Task<FatorEmissaoDocument?> GetByCodigoAndVersaoAsync(
        string codigo,
        int versao,
        CancellationToken cancellationToken
    );

    Task<IReadOnlyList<FatorEmissaoDocument>> GetAllAsync(
        CancellationToken cancellationToken
    );

    Task<bool> UpdateAsync(
        string id,
        FatorEmissaoDocument document,
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
