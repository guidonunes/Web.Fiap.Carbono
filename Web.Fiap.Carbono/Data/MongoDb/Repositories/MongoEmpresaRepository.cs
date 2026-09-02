using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;
using Web.Fiap.Carbono.Models.Documents;
using MongoDuplicateKeyException = Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions.MongoDuplicateKeyException;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories;

public sealed class MongoEmpresaRepository
    : IMongoEmpresaRepository
{
    private readonly IMongoCollection<EmpresaDocument>
        _empresas;

    public MongoEmpresaRepository(MongoDbContext context)
    {
        _empresas = context.Empresas;
    }

    public async Task<EmpresaDocument> CreateAsync(
        EmpresaDocument document,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await _empresas.InsertOneAsync(
                document,
                cancellationToken: cancellationToken
            );

            return document;
        }
        catch (MongoWriteException exception)
            when (MongoRepositoryRules.IsDuplicateKey(exception))
        {
            throw new MongoDuplicateKeyException(
                "Já existe uma empresa com o mesmo CNPJ."
            );
        }
    }

    public async Task<EmpresaDocument?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var empresa = await _empresas
            .Find(item => item.Id == objectId)
            .FirstOrDefaultAsync(cancellationToken);

        return empresa;
    }

    public async Task<EmpresaDocument?> GetByCnpjAsync(
        string cnpj,
        CancellationToken cancellationToken
    )
    {
        var empresa = await _empresas
            .Find(empresa => empresa.Cnpj == cnpj)
            .FirstOrDefaultAsync(cancellationToken);

        return empresa;
    }

    public Task<List<EmpresaDocument>> GetAllAsync(
        CancellationToken cancellationToken
    )
    {
        var sort = Builders<EmpresaDocument>.Sort.Combine(
            Builders<EmpresaDocument>.Sort.Ascending(
                empresa => empresa.Codigo
            ),
            Builders<EmpresaDocument>.Sort.Ascending(
                empresa => empresa.Id
            )
        );

        return _empresas
            .Find(Builders<EmpresaDocument>.Filter.Empty)
            .Sort(sort)
            .ToListAsync(cancellationToken);
    }

    async Task<IReadOnlyList<EmpresaDocument>>
        IMongoEmpresaRepository.GetAllAsync(
            CancellationToken cancellationToken
        )
    {
        return await GetAllAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(
        string id,
        EmpresaDocument document,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));
        EnsureMatchingId(objectId, document.Id);

        try
        {
            var result = await _empresas.ReplaceOneAsync(
                empresa => empresa.Id == objectId,
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
                "Já existe uma empresa com o mesmo CNPJ."
            );
        }
    }

    public async Task<bool> DeleteAsync(
        string id,
        CancellationToken cancellationToken
    )
    {
        var objectId = MongoRepositoryRules.ParseObjectId(id, nameof(id));

        var result = await _empresas.DeleteOneAsync(
            empresa => empresa.Id == objectId,
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

        var count = await _empresas.CountDocumentsAsync(
            empresa => empresa.Id == objectId,
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
