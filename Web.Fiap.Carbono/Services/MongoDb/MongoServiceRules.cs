using MongoDB.Bson;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;

namespace Web.Fiap.Carbono.Services.MongoDb;

internal static class MongoServiceRules
{
    public static ObjectId ParseObjectId(string? value, string fieldName = "id")
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !ObjectId.TryParse(value, out var objectId))
        {
            throw new BadRequestException(
                $"O campo '{fieldName}' não contém um ObjectId válido.");
        }

        return objectId;
    }

    public static string Required(
        string? value,
        string fieldName,
        int maximumLength = 200)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new BadRequestException(
                $"O campo '{fieldName}' é obrigatório.");
        }

        var normalized = value.Trim();

        if (normalized.Length > maximumLength)
        {
            throw new BadRequestException(
                $"O campo '{fieldName}' deve possuir no máximo {maximumLength} caracteres.");
        }

        return normalized;
    }

    public static string? Optional(
        string? value,
        string fieldName,
        int maximumLength = 200)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();

        if (normalized.Length > maximumLength)
        {
            throw new BadRequestException(
                $"O campo '{fieldName}' deve possuir no máximo {maximumLength} caracteres.");
        }

        return normalized;
    }

    public static string ValidateCnpj(string? cnpj)
    {
        var normalized = new string(
            (cnpj ?? string.Empty)
            .Where(char.IsDigit)
            .ToArray());

        if (normalized.Length != 14)
        {
            throw new BadRequestException(
                "O CNPJ deve possuir exatamente 14 dígitos.");
        }

        return normalized;
    }

    public static decimal Percentage(
        decimal value,
        string fieldName)
    {
        if (value is < 0 or > 100)
        {
            throw new BadRequestException(
                $"O campo '{fieldName}' deve estar entre 0 e 100.");
        }

        return value;
    }

    public static decimal? OptionalPercentage(
        decimal? value,
        string fieldName)
    {
        return value.HasValue
            ? Percentage(value.Value, fieldName)
            : null;
    }

    public static decimal NonNegative(
        decimal value,
        string fieldName)
    {
        if (value < 0)
        {
            throw new BadRequestException(
                $"O campo '{fieldName}' não pode ser negativo.");
        }

        return value;
    }

    public static DateTime AsUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    public static string ValidateScope(string? scope)
    {
        var normalized = Required(scope, "escopo", 20).ToUpperInvariant();

        if (normalized is not
            ("ESCOPO_1" or "ESCOPO_2" or "ESCOPO_3"))
        {
            throw new BadRequestException(
                "O escopo deve ser ESCOPO_1, ESCOPO_2 ou ESCOPO_3.");
        }

        return normalized;
    }

    public static bool IsTemporaryRecord(string codigo)
    {
        return codigo.StartsWith(
            "CRUD-TEMP",
            StringComparison.OrdinalIgnoreCase);
    }
}
