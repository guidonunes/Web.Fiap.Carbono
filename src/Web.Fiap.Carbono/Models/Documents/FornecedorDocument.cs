using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Web.Fiap.Carbono.Models.Documents.Embedded;

namespace Web.Fiap.Carbono.Models.Documents;

[BsonIgnoreExtraElements]
public sealed class FornecedorDocument
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

    [BsonElement("ativo")]
    public bool Ativo { get; init; }

    [BsonElement("categoriasAtuacao")]
    public List<string> CategoriasAtuacao { get; init; } = new();

    [BsonElement("certificacoes")]
    public List<Certificacao> Certificacoes { get; init; } = new();

    [BsonElement("indicadoresSociais")]
    public IndicadoresSociais? IndicadoresSociais { get; init; }

    [BsonElement("conformidadeAmbiental")]
    public ConformidadeAmbiental? ConformidadeAmbiental { get; init; }

    [BsonElement("statusAuditoria")]
    public string? StatusAuditoria { get; init; }

    [BsonElement("nivelRiscoEsg")]
    public string? NivelRiscoEsg { get; init; }

    [BsonElement("criadoEm")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CriadoEm { get; init; }

    [BsonElement("atualizadoEm")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AtualizadoEm { get; init; }

    [BsonElement("schemaVersion")]
    public int SchemaVersion { get; init; }
}
