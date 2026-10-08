using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents.Embedded;

[BsonIgnoreExtraElements]
public sealed class EtapaSnapshot
{
    [BsonElement("nome")]
    public string Nome { get; init; } = string.Empty;

    [BsonElement("ordem")]
    public int Ordem { get; init; }

    [BsonElement("categoria")]
    public string Categoria { get; init; } = string.Empty;

    [BsonElement("local")]
    [BsonIgnoreIfNull]
    public string? Local { get; init; }
}
