using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Web.Fiap.Carbono.Models.Documents.Embedded;

namespace Web.Fiap.Carbono.Models.Documents;

[BsonIgnoreExtraElements]
public sealed class EmpresaDocument
{
    [BsonId]
    public ObjectId Id { get; init; }

    [BsonElement("legacyId")]
    public int? LegacyId { get; init; }

    [BsonElement("codigo")]
    public string Codigo { get; init; } = string.Empty;

    [BsonElement("razaoSocial")]
    public string RazaoSocial { get; init; } = string.Empty;

    [BsonElement("nomeFantasia")]
    public string? NomeFantasia { get; init; }

    [BsonElement("cnpj")]
    public string Cnpj { get; init; } = string.Empty;

    [BsonElement("setor")]
    public string? Setor { get; init; }

    [BsonElement("ativa")]
    public bool Ativa { get; init; }

    [BsonElement("metasReducao")]
    public List<MetaReducao> MetasReducao { get; init; } = [];

    [BsonElement("governanca")]
    public GovernancaEsg? Governanca { get; init; }

    [BsonElement("criadoEm")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CriadoEm { get; init; }

    [BsonElement("atualizadoEm")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AtualizadoEm { get; init; }

    [BsonElement("schemaVersion")]
    public int SchemaVersion { get; init; }
}