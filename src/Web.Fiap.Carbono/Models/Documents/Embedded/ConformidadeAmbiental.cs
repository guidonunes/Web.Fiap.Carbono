using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents.Embedded;

public sealed class ConformidadeAmbiental
{
    [BsonElement("possuiLicenca")]
    public bool PossuiLicenca { get; init; }

    [BsonElement("ocorrenciasUltimos12Meses")]
    public int OcorrenciasUltimos12Meses { get; init; }

    [BsonElement("descarteMonitorado")]
    public bool? DescarteMonitorado { get; init; }

    [BsonElement("percentualMaterialReciclado")]
    [BsonRepresentation(BsonType.Decimal128)]
    public decimal? PercentualMaterialReciclado { get; init; }

    [BsonElement("dataUltimaAuditoria")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? DataUltimaAuditoria { get; init; }

    // Suppliers may have different applicable environmental controls.
    [BsonExtraElements]
    public BsonDocument? CamposAdicionais { get; init; }
}
