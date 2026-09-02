using Web.Fiap.Carbono.Exceptions;

namespace Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;

public sealed class MongoPaginationException : DomainValidationException
{
    public MongoPaginationException(string message)
        : base(message)
    {
    }
}
