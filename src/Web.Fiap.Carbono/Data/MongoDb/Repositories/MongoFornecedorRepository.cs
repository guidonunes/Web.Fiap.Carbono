using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;
using Web.Fiap.Carbono.Models.Documents;
using MongoDuplicateKeyException = Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions.MongoDuplicateKeyException;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories;

public sealed class MongoFornecedorRepository
    : IMongoFornecedorRepository
{
    private readonly IMongoCollection<FornecedorDocument>
        _fornecedores;

    public MongoFornecedorRepository(MongoDbContext context)
    {
        _fornecedores = context.Fornecedores;
    }

    public async Task<FornecedorDocument> CreateAsync(
        FornecedorDocument document,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _fornecedores.InsertOneAsync(
                document,
                cancellationToken: cancellationToken
            );

            return document;
        }
        catch (MongoWriteException exception)
            when (MongoRepositoryRules.IsDuplicateKey(exception))
        {
            throw new MongoDuplicateKeyException(
                "Já existe um fornecedor com o mesmo CNPJ."
            );
        }
    }

    public async Task<FornecedorDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var fornecedor = await _fornecedores
            .Find(item => item.Id == objectId)
            .FirstOrDefaultAsync(cancellationToken);

        return fornecedor;
    }

    public async Task<FornecedorDocument?> GetByCodigoAsync(
        string codigo,
        CancellationToken cancellationToken
    )
    {
        var fornecedor = await _fornecedores
            .Find(fornecedor => fornecedor.Codigo == codigo)
            .FirstOrDefaultAsync(cancellationToken);

        return fornecedor;
    }

    public async Task<FornecedorDocument?> GetByCnpjAsync(
        string cnpj,
        CancellationToken cancellationToken
    )
    {
        var fornecedor = await _fornecedores
            .Find(fornecedor => fornecedor.Cnpj == cnpj)
            .FirstOrDefaultAsync(cancellationToken);

        return fornecedor;
    }

    public Task<List<FornecedorDocument>> GetAllAsync(
        CancellationToken cancellationToken
    )
    {
        var sort = Builders<FornecedorDocument>.Sort.Combine(
            Builders<FornecedorDocument>.Sort.Ascending(
                fornecedor => fornecedor.Codigo
            ),
            Builders<FornecedorDocument>.Sort.Ascending(
                fornecedor => fornecedor.Id
            )
        );

        return _fornecedores
            .Find(Builders<FornecedorDocument>.Filter.Empty)
            .Sort(sort)
            .ToListAsync(cancellationToken);
    }

    async Task<IReadOnlyList<FornecedorDocument>>
        IMongoFornecedorRepository.GetAllAsync(
            CancellationToken cancellationToken
        )
    {
        return await GetAllAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        string id,
        FornecedorDocument document,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));
        EnsureMatchingId(objectId, document.Id);

        try
        {
            var result = await _fornecedores.ReplaceOneAsync(
                fornecedor => fornecedor.Id == objectId,
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
                "Já existe um fornecedor com o mesmo CNPJ."
            );
        }
    }

    public async Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var result = await _fornecedores.DeleteOneAsync(
            fornecedor => fornecedor.Id == objectId,
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

        var count = await _fornecedores.CountDocumentsAsync(
            fornecedor => fornecedor.Id == objectId,
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
