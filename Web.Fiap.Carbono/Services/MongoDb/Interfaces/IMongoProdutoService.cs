using Web.Fiap.Carbono.Dtos.MongoDb.Produtos;
using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Services.MongoDb.Interfaces;

public interface IMongoProdutoService
{
    Task<ProdutoDocument> CreateAsync(
        CreateProdutoRequest request,
        CancellationToken cancellationToken = default);

    Task<ProdutoDocument> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProdutoDocument>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<ProdutoDocument> UpdateAsync(
        string id,
        UpdateProdutoRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default);
}