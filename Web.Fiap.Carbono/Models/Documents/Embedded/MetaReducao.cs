using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents.Embedded;

[BsonIgnoreExtraElements]
public sealed class MetaReducao
{
    [BsonElement("tipo")]
    public string Tipo { get; init; } = string.Empty;

    [BsonElement("anoBase")]
    public int AnoBase { get; init; }

    [BsonElement("anoMeta")]
    public int AnoMeta { get; init; }

    [BsonElement("percentualReducao")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal PercentualReducao { get; init; }
}