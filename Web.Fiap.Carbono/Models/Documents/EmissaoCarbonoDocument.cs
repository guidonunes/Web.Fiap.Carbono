using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Web.Fiap.Carbono.Models.Documents.Embedded;

namespace Web.Fiap.Carbono.Models.Documents;

[BsonIgnoreExtraElements]
public sealed class EmissaoCarbonoDocument
{
    [BsonId]
    public ObjectId Id { get; init; }

    [BsonElement("legacyId")]
    [BsonIgnoreIfNull]
    public int? LegacyId { get; init; }

    // Deterministic business key used by the seed and CRUD demo.
    [BsonElement("codigo")]
    public string Codigo { get; init; } = string.Empty;

    [BsonElement("empresaId")]
    public ObjectId EmpresaId { get; init; }

    [BsonElement("produtoId")]
    public ObjectId ProdutoId { get; init; }

    [BsonElement("fornecedorId")]
    public ObjectId FornecedorId { get; init; }

    [BsonElement("fatorEmissaoId")]
    public ObjectId FatorEmissaoId { get; init; }

    [BsonElement("lote")]
    public LoteSnapshot Lote { get; init; } = new();

    [BsonElement("etapa")]
    public EtapaSnapshot Etapa { get; init; } = new();

    [BsonElement("quantidadeAtividade")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal QuantidadeAtividade { get; init; }

    // This is intentionally flexible: TRANSPORTE, ENERGIA,
    // MATERIA_PRIMA, and RESIDUO can have different fields.
    [BsonElement("dadosAtividade")]
    public BsonDocument DadosAtividade { get; init; } = new();

    // Immutable embedded snapshot of the factor applied at calculation time.
    [BsonElement("fatorAplicado")]
    public FatorEmissaoSnapshot FatorAplicado { get; init; } = new();

    [BsonElement("quantidadeEmitidaKgCO2e")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal QuantidadeEmitidaKgCO2e { get; init; }

    [BsonElement("metodoCalculo")]
    public string MetodoCalculo { get; init; } = string.Empty;

    [BsonElement("fonteEmissao")]
    [BsonIgnoreIfNull]
    public string? FonteEmissao { get; init; }

    [BsonElement("observacao")]
    [BsonIgnoreIfNull]
    public string? Observacao { get; init; }

    [BsonElement("calculadoPor")]
    public string CalculadoPor { get; init; } = string.Empty;

    [BsonElement("dataEmissao")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DataEmissao { get; init; }

    [BsonElement("revisadoEm")]
    [BsonIgnoreIfNull]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? RevisadoEm { get; init; }

    [BsonElement("revisadoPor")]
    [BsonIgnoreIfNull]
    public string? RevisadoPor { get; init; }

    [BsonElement("criadoEm")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CriadoEm { get; init; }

    [BsonElement("atualizadoEm")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime AtualizadoEm { get; init; }

    [BsonElement("schemaVersion")]
    public int SchemaVersion { get; init; }
}
