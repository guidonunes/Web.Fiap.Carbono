using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Web.Fiap.Carbono.Models.Documents.Embedded;

namespace Web.Fiap.Carbono.Models.Documents;

[BsonIgnoreExtraElements]
public sealed class ProdutoDocument
{
    [BsonId]
    public ObjectId Id { get; init; }

    [BsonElement("legacyId")]
    public int? LegacyId { get; init; }

    [BsonElement("empresaId")]
    public ObjectId EmpresaId { get; init; }

    [BsonElement("codigo")]
    public string Codigo { get; init; } = string.Empty;

    [BsonElement("nome")]
    public string Nome { get; init; } = string.Empty;

    [BsonElement("categoria")]
    public string? Categoria { get; init; }

    [BsonElement("unidadeFuncional")]
    public string UnidadeFuncional { get; init; } = string.Empty;

    [BsonElement("ativo")]
    public bool Ativo { get; init; }

    [BsonElement("atributosAmbientais")]
    public AtributosAmbientais? AtributosAmbientais { get; init; }

    [BsonElement("criadoEm")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CriadoEm { get; init; }

    [BsonElement("atualizadoEm")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AtualizadoEm { get; init; }

    [BsonElement("schemaVersion")]
    public int SchemaVersion { get; init; }
}
