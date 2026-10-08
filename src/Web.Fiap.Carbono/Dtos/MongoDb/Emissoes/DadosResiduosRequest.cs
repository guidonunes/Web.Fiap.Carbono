using System.ComponentModel.DataAnnotations;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;

public sealed class DadosResiduoRequest : DadosAtividadeRequest
{
    public override string Categoria => "RESIDUO";

    [Required]
    [StringLength(100)]
    public string Classe { get; init; } = string.Empty;

    [Range(
        typeof(decimal),
        "0.00000001",
        "79228162514264337593543950335")]
    public decimal PesoKg { get; init; }

    [StringLength(150)]
    public string? TipoResiduo { get; init; }

    [Required]
    [StringLength(150)]
    public string Tratamento { get; init; } = string.Empty;

    [Range(
        typeof(decimal),
        "0",
        "79228162514264337593543950335")]
    public decimal DistanciaDestinoKm { get; init; }

    [Range(typeof(decimal), "0", "100")]
    public decimal? PercentualReciclavel { get; init; }
}