using Web.Fiap.Carbono.Dtos.MongoDb.FatoresEmissao;
using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Services.MongoDb.Interfaces;

public interface IMongoFatorEmissaoService
{
    Task<FatorEmissaoDocument> CreateAsync(
        CreateFatorEmissaoRequest request,
        CancellationToken cancellationToken = default);

    Task<FatorEmissaoDocument> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FatorEmissaoDocument>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<FatorEmissaoDocument> UpdateAsync(
        string id,
        UpdateFatorEmissaoRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);
}