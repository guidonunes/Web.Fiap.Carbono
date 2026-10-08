using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents.Embedded;

[BsonIgnoreExtraElements]
public sealed class LoteSnapshot
{
    [BsonElement("codigo")]
    public string Codigo { get; init; } = string.Empty;

    [BsonElement("quantidadeProduzida")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal QuantidadeProduzida { get; init; }

    [BsonElement("unidade")]
    public string Unidade { get; init; } = string.Empty;

    [BsonElement("dataProducao")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DataProducao { get; init; }
}