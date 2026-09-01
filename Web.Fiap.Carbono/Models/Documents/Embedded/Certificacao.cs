using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents.Embedded;

[BsonIgnoreExtraElements]
public sealed class Certificacao
{
    [BsonElement("nome")]
    public string Nome { get; init; } = string.Empty;

    [BsonElement("emissor")]
    public string Emissor { get; init; } = string.Empty;

    [BsonElement("validaAte")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ValidaAte { get; init; }
}
