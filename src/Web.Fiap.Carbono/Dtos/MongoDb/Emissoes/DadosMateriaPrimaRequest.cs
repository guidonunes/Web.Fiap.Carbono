using System.ComponentModel.DataAnnotations;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;

public sealed class DadosMateriaPrimaRequest : DadosAtividadeRequest
{
    public override string Categoria => "MATERIA_PRIMA";

    [Required]
    [StringLength(150)]
    public string Material { get; init; } = string.Empty;

    [Range(
        typeof(decimal),
        "0.00000001",
        "79228162514264337593543950335")]
    public decimal PesoKg { get; init; }

    [Range(typeof(decimal), "0", "100")]
    public decimal PercentualReciclado { get; init; }

    [StringLength(150)]
    public string? Origem { get; init; }
}