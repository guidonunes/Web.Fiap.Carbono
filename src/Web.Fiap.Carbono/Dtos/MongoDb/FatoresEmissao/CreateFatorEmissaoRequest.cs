using System.ComponentModel.DataAnnotations;

namespace Web.Fiap.Carbono.Dtos.MongoDb.FatoresEmissao;

public sealed class CreateFatorEmissaoRequest
{
    [Required]
    [StringLength(80)]
    [RegularExpression("^[A-Za-z0-9-]+$")]
    public string Codigo { get; init; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Nome { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Categoria { get; init; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999999999999")]
    public decimal Valor { get; init; }

    [Required]
    [StringLength(50)]
    public string UnidadeBase { get; init; } = string.Empty;

    [Required]
    [RegularExpression(
        "^ESCOPO_[123]$",
        ErrorMessage =
            "Escopo must be ESCOPO_1, ESCOPO_2, or ESCOPO_3."
    )]
    public string Escopo { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Versao { get; init; }

    [StringLength(300)]
    public string? FonteReferencia { get; init; }

    [StringLength(500)]
    public string? Metodologia { get; init; }

    public DateTime ValidoDe { get; init; }

    public DateTime? ValidoAte { get; init; }

    public bool Ativo { get; init; } = true;
}
