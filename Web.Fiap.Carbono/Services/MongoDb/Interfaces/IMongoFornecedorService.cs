using Web.Fiap.Carbono.Dtos.MongoDb.Fornecedores;
using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Services.MongoDb.Interfaces;

public interface IMongoFornecedorService
{
    Task<FornecedorDocument> CreateAsync(
        CreateFornecedorRequest request,
        CancellationToken cancellationToken = default);

    Task<FornecedorDocument> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FornecedorDocument>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<FornecedorDocument> UpdateAsync(
        string id,
        UpdateFornecedorRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);
}