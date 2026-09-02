using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;
using Web.Fiap.Carbono.Exceptions;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories;

internal static class MongoRepositoryRules
{
    public static ObjectId ParseObjectId(
        string id,
        string parameterName
    )
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            throw new DomainValidationException(
                $"{parameterName} deve ser um ObjectId válido."
            );
        }

        return objectId;
    }

    public static void ValidatePagination(
        int pageNumber,
        int pageSize
    )
    {
        if (pageNumber < 1)
        {
            throw new MongoPaginationException(
                "pageNumber deve ser maior ou igual a 1."
            );
        }

        if (pageSize is < 1 or > 50)
        {
            throw new MongoPaginationException(
                "pageSize deve estar entre 1 e 50."
            );
        }
    }

    public static bool IsDuplicateKey(
        MongoWriteException exception
    )
    {
        return exception.WriteError?.Category ==
               ServerErrorCategory.DuplicateKey;
    }
}
