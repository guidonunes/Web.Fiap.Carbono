using System.ComponentModel.DataAnnotations;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;

public sealed class LoteEmissaoRequest
{
    [Required]
    [StringLength(100)]
    public string Codigo { get; init; } = string.Empty;

    [Range(
        typeof(decimal),
        "0.0000000000000000000000000001",
        "79228162514264337593543950335",
        ErrorMessage = "A quantidade produzida deve ser maior que zero."
    )]
    public decimal QuantidadeProduzida { get; init; }

    [Required]
    [StringLength(50)]
    public string Unidade { get; init; } = string.Empty;

    public DateTime DataProducao { get; init; }
}