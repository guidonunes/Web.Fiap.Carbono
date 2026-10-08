using Web.Fiap.Carbono.Exceptions;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;

public sealed class MongoDuplicateKeyException : ConflictException
{
    public MongoDuplicateKeyException(string message)
        : base(message)
    {
    }
}
