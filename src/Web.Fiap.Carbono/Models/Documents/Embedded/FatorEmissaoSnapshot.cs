using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents.Embedded;

[BsonIgnoreExtraElements]
public sealed class FatorEmissaoSnapshot
{
    [BsonElement("codigo")]
    public string Codigo { get; init; } = string.Empty;

    [BsonElement("nome")]
    public string Nome { get; init; } = string.Empty;

    [BsonElement("valor")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Valor { get; init; }

    [BsonElement("unidadeBase")]
    public string UnidadeBase { get; init; } = string.Empty;

    [BsonElement("escopo")]
    public string Escopo { get; init; } = string.Empty;

    [BsonElement("versao")]
    public int Versao { get; init; }

    [BsonElement("fonteReferencia")]
    [BsonIgnoreIfNull]
    public string? FonteReferencia { get; init; }

    [BsonElement("metodologia")]
    [BsonIgnoreIfNull]
    public string? Metodologia { get; init; }
}
