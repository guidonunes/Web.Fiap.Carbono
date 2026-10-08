using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents.Embedded;

public sealed class AtributosAmbientais
{
    [BsonElement("materiais")]
    public List<MaterialProduto> Materiais { get; init; } = [];

    [BsonElement("percentualReciclavel")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? PercentualReciclavel { get; init; }

    [BsonElement("percentualMaterialReciclado")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? PercentualMaterialReciclado { get; init; }

    [BsonElement("embalagem")]
    public string? Embalagem { get; init; }

    [BsonElement("seloSustentabilidade")]
    public string? SeloSustentabilidade { get; init; }

    [BsonElement("compensacaoCarbono")]
    public bool? CompensacaoCarbono { get; init; }

    [BsonElement("origemAgriculturaOrganica")]
    public bool? OrigemAgriculturaOrganica { get; init; }

    [BsonElement("vidaUtilAnos")]
    public int? VidaUtilAnos { get; init; }

    // Product environmental attributes are intentionally extensible. Preserve
    // fields not yet represented by the stable part of this C# model.
    [BsonExtraElements]
    public BsonDocument? CamposAdicionais { get; init; }
}
