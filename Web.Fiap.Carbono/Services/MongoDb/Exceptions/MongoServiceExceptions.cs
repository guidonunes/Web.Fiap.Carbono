using Web.Fiap.Carbono.Exceptions;

namespace Web.Fiap.Carbono.Services.MongoDb.Exceptions;

public sealed class BadRequestException : DomainValidationException
{
    public BadRequestException(string message) : base(message)
    {
    }
}

public sealed class NotFoundException
    : Web.Fiap.Carbono.Exceptions.NotFoundException
{
    public NotFoundException(string message) : base(message)
    {
    }
}

public sealed class ConflictException
    : Web.Fiap.Carbono.Exceptions.ConflictException
{
    public ConflictException(string message, Exception? innerException = null)
        : base(message)
    {
    }
}

public sealed class UnprocessableEntityException : BusinessRuleException
{
    public UnprocessableEntityException(string message) : base(message)
    {
    }
}
