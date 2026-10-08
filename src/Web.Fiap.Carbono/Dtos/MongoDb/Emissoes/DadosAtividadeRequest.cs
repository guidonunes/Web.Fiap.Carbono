using System.Text.Json.Serialization;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "tipo")]
[JsonDerivedType(typeof(DadosTransporteRequest), "TRANSPORTE")]
[JsonDerivedType(typeof(DadosEnergiaRequest), "ENERGIA")]
[JsonDerivedType(typeof(DadosMateriaPrimaRequest), "MATERIA_PRIMA")]
[JsonDerivedType(typeof(DadosResiduoRequest), "RESIDUO")]
public abstract class DadosAtividadeRequest
{
    [JsonIgnore]
    public abstract string Categoria { get; }
}