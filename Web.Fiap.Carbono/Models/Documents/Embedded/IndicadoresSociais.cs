using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents.Embedded;

[BsonIgnoreExtraElements]
public sealed class IndicadoresSociais
{
    [BsonElement("acidentesUltimos12Meses")]
    public int AcidentesUltimos12Meses { get; init; }

    [BsonElement("percentualMulheresLideranca")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? PercentualMulheresLideranca { get; init; }

    [BsonElement("possuiProgramaDiversidade")]
    public bool? PossuiProgramaDiversidade { get; init; }
}