using System.ComponentModel.DataAnnotations;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;

public sealed class DadosTransporteRequest : DadosAtividadeRequest
{
    public override string Categoria => "TRANSPORTE";

    [Range(
        typeof(decimal),
        "0.00000001",
        "79228162514264337593543950335")]
    public decimal DistanciaKm { get; init; }

    [Range(
        typeof(decimal),
        "0.00000001",
        "79228162514264337593543950335")]
    public decimal CargaToneladas { get; init; }

    [Required]
    [StringLength(100)]
    public string Combustivel { get; init; } = string.Empty;

    [StringLength(100)]
    public string? Modal { get; init; }
}