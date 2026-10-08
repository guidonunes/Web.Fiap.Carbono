using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents.Embedded;

[BsonIgnoreExtraElements]
public sealed class MaterialProduto
{
    [BsonElement("nome")]
    public string Nome { get; init; } = string.Empty;

    [BsonElement("percentualComposicao")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal PercentualComposicao { get; init; }

    [BsonElement("origemRenovavel")]
    public bool? OrigemRenovavel { get; init; }

    [BsonElement("origemReciclada")]
    public bool? OrigemReciclada { get; init; }
}