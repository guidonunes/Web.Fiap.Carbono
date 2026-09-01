using MongoDB.Bson.Serialization.Attributes;

namespace Web.Fiap.Carbono.Models.Documents.Embedded;

[BsonIgnoreExtraElements]
public sealed class GovernancaEsg
{
    [BsonElement("responsavelEsg")]
    public string ResponsavelEsg { get; init; } = string.Empty;

    [BsonElement("comiteEsg")]
    public bool ComiteEsg { get; init; }

    [BsonElement("frequenciaAuditoria")]
    public string FrequenciaAuditoria { get; init; } = string.Empty;

    [BsonElement("relatorioPublico")]
    public bool? RelatorioPublico { get; init; }

    [BsonElement("canalDenuncias")]
    public bool? CanalDenuncias { get; init; }

    [BsonElement("conselhoSupervisao")]
    public bool? ConselhoSupervisao { get; init; }
}