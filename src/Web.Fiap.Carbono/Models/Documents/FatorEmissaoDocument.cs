using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents;

[BsonIgnoreExtraElements]
public sealed class FatorEmissaoDocument
{
    [BsonId]
    public ObjectId Id { get; init; }

    [BsonElement("legacyId")]
    public int? LegacyId { get; init; }

    [BsonElement("codigo")]
    public string Codigo { get; init; } = string.Empty;

    [BsonElement("nome")]
    public string Nome { get; init; } = string.Empty;

    [BsonElement("categoria")]
    public string Categoria { get; init; } = string.Empty;

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
    public string? FonteReferencia { get; init; }

    [BsonElement("metodologia")]
    public string? Metodologia { get; init; }

    [BsonElement("validoDe")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime ValidoDe { get; init; }

    [BsonElement("validoAte")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? ValidoAte { get; init; }

    [BsonElement("ativo")]
    public bool Ativo { get; init; }

    [BsonElement("criadoEm")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CriadoEm { get; init; }

    [BsonElement("atualizadoEm")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AtualizadoEm { get; init; }

    [BsonElement("schemaVersion")]
    public int SchemaVersion { get; init; }
}
