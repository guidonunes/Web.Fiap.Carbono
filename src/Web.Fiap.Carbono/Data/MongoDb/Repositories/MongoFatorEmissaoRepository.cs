using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;
using Web.Fiap.Carbono.Models.Documents;
using MongoDuplicateKeyException = Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions.MongoDuplicateKeyException;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories;

public sealed class MongoFatorEmissaoRepository
    : IMongoFatorEmissaoRepository
{
    private readonly IMongoCollection<FatorEmissaoDocument>
        _fatoresEmissao;

    public MongoFatorEmissaoRepository(
        MongoDbContext context
    )
    {
        _fatoresEmissao = context.FatoresEmissao;
    }

    public async Task<FatorEmissaoDocument> CreateAsync(
        FatorEmissaoDocument document,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _fatoresEmissao.InsertOneAsync(
                document,
                cancellationToken: cancellationToken
            );

            return document;
        }
        catch (MongoWriteException exception)
            when (MongoRepositoryRules.IsDuplicateKey(exception))
        {
            throw new MongoDuplicateKeyException(
                "Já existe um fator de emissão com o mesmo código e versão."
            );
        }
    }

    public async Task<FatorEmissaoDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var fator = await _fatoresEmissao
            .Find(item => item.Id == objectId)
            .FirstOrDefaultAsync(cancellationToken);

        return fator;
    }

    public async Task<FatorEmissaoDocument?> GetByCodigoAndVersaoAsync(
        string codigo,
        int versao,
        CancellationToken cancellationToken
    )
    {
        var fator = await _fatoresEmissao
            .Find(fator =>
                fator.Codigo == codigo &&
                fator.Versao == versao
            )
            .FirstOrDefaultAsync(cancellationToken);

        return fator;
    }

    public Task<List<FatorEmissaoDocument>> GetAllAsync(
        CancellationToken cancellationToken
    )
    {
        var sort = Builders<FatorEmissaoDocument>.Sort.Combine(
            Builders<FatorEmissaoDocument>.Sort.Ascending(
                fator => fator.Codigo
            ),
            Builders<FatorEmissaoDocument>.Sort.Ascending(
                fator => fator.Versao
            ),
            Builders<FatorEmissaoDocument>.Sort.Ascending(
                fator => fator.Id
            )
        );

        return _fatoresEmissao
            .Find(Builders<FatorEmissaoDocument>.Filter.Empty)
            .Sort(sort)
            .ToListAsync(cancellationToken);
    }

    async Task<IReadOnlyList<FatorEmissaoDocument>>
        IMongoFatorEmissaoRepository.GetAllAsync(
            CancellationToken cancellationToken
        )
    {
        return await GetAllAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        string id,
        FatorEmissaoDocument document,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));
        EnsureMatchingId(objectId, document.Id);

        try
        {
            var result = await _fatoresEmissao.ReplaceOneAsync(
                fator => fator.Id == objectId,
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
                "Já existe um fator de emissão com o mesmo código e versão."
            );
        }
    }

    public async Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var result = await _fatoresEmissao.DeleteOneAsync(
            fator => fator.Id == objectId,
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

        var count = await _fatoresEmissao.CountDocumentsAsync(
            fator => fator.Id == objectId,
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
