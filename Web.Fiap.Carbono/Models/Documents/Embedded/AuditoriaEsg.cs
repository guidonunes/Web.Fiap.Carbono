using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents.Embedded;

[BsonIgnoreExtraElements]
public sealed class AuditoriaEsg
{
    [BsonElement("status")]
    public string Status { get; init; } = string.Empty;

    [BsonElement("nivelRisco")]
    public string NivelRisco { get; init; } = string.Empty;

    [BsonElement("dataUltimaAuditoria")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DataUltimaAuditoria { get; init; }
}