using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;
using Web.Fiap.Carbono.Models.Documents;
using MongoDuplicateKeyException = Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions.MongoDuplicateKeyException;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories;

public sealed class MongoProdutoRepository
    : IMongoProdutoRepository
{
    private readonly IMongoCollection<ProdutoDocument>
        _produtos;

    public MongoProdutoRepository(MongoDbContext context)
    {
        _produtos = context.Produtos;
    }

    public async Task<ProdutoDocument> CreateAsync(
        ProdutoDocument document,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _produtos.InsertOneAsync(
                document,
                cancellationToken: cancellationToken
            );

            return document;
        }
        catch (MongoWriteException exception)
            when (MongoRepositoryRules.IsDuplicateKey(exception))
        {
            throw new MongoDuplicateKeyException(
                "Já existe um produto com o mesmo código nesta empresa."
            );
        }
    }

    public async Task<ProdutoDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var produto = await _produtos
            .Find(item => item.Id == objectId)
            .FirstOrDefaultAsync(cancellationToken);

        return produto;
    }

    public async Task<ProdutoDocument?> GetByEmpresaIdAndCodigoAsync(
        string empresaId,
        string codigo,
        CancellationToken cancellationToken
    )
    {
        var empresaObjectId = MongoRepositoryRules.ParseObjectId(
            empresaId,
            nameof(empresaId)
        );

        var produto = await _produtos
            .Find(produto =>
                produto.EmpresaId == empresaObjectId &&
                produto.Codigo == codigo
            )
            .FirstOrDefaultAsync(cancellationToken);

        return produto;
    }

    public Task<List<ProdutoDocument>> GetAllAsync(
        CancellationToken cancellationToken
    )
    {
        var sort = Builders<ProdutoDocument>.Sort.Combine(
            Builders<ProdutoDocument>.Sort.Ascending(
                produto => produto.EmpresaId
            ),
            Builders<ProdutoDocument>.Sort.Ascending(
                produto => produto.Codigo
            ),
            Builders<ProdutoDocument>.Sort.Ascending(
                produto => produto.Id
            )
        );

        return _produtos
            .Find(Builders<ProdutoDocument>.Filter.Empty)
            .Sort(sort)
            .ToListAsync(cancellationToken);
    }

    async Task<IReadOnlyList<ProdutoDocument>>
        IMongoProdutoRepository.GetAllAsync(
            CancellationToken cancellationToken
        )
    {
        return await GetAllAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        string id,
        ProdutoDocument document,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));
        EnsureMatchingId(objectId, document.Id);

        try
        {
            var result = await _produtos.ReplaceOneAsync(
                produto => produto.Id == objectId,
                document,
                new ReplaceOptions { IsUpsert = false },
                cancellationToken
            );

            return result.IsAcknowledged &&
                result.MatchedCount == 1;
        }
        catch (MongoWriteException exception)
            when (MongoRepositoryRules.IsDuplicateKey(exception))
        {
            throw new MongoDuplicateKeyException(
                "Já existe um produto com o mesmo código nesta empresa."
            );
        }
    }

    public async Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var result = await _produtos.DeleteOneAsync(
            produto => produto.Id == objectId,
            cancellationToken
        );

        return result.IsAcknowledged &&
            result.DeletedCount == 1;
    }

    public async Task<bool> ExistsAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var count = await _produtos.CountDocumentsAsync(
            produto => produto.Id == objectId,
            new CountOptions { Limit = 1 },
            cancellationToken
        );

        return count == 1;
    }

    private static void EnsureMatchingId(
        ObjectId requestedId,
        ObjectId documentId
    )
    {
        if (requestedId != documentId)
        {
            throw new ArgumentException(
                "O Id do documento deve corresponder ao Id da atualização."
            );
        }
    }
}
