using System.ComponentModel.DataAnnotations;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;

public sealed class EtapaEmissaoRequest
{
    [Required]
    [StringLength(150)]
    public string Nome { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Ordem { get; init; }

    [Required]
    [RegularExpression(
        "^(TRANSPORTE|ENERGIA|MATERIA_PRIMA|RESIDUO)$",
        ErrorMessage =
            "A categoria deve ser TRANSPORTE, ENERGIA, MATERIA_PRIMA ou RESIDUO."
    )]
    public string Categoria { get; init; } = string.Empty;

    [StringLength(200)]
    public string? Local { get; init; }
}