using System.ComponentModel.DataAnnotations;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;

public sealed class DadosEnergiaRequest : DadosAtividadeRequest
{
    public override string Categoria => "ENERGIA";

    [Range(
        typeof(decimal),
        "0.00000001",
        "79228162514264337593543950335")]
    public decimal ConsumoKwh { get; init; }

    [Required]
    [StringLength(100)]
    public string FonteEnergia { get; init; } = string.Empty;

    [Range(typeof(decimal), "0", "100")]
    public decimal PercentualRenovavel { get; init; }
}