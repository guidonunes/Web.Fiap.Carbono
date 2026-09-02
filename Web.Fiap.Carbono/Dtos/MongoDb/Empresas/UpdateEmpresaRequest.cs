using System.ComponentModel.DataAnnotations;
using Web.Fiap.Carbono.Dtos.MongoDb.Common;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Empresas;

public sealed class UpdateEmpresaRequest
{
    [Required]
    [StringLength(50)]
    [RegularExpression("^[A-Za-z0-9-]+$")]
    public string Codigo { get; init; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string RazaoSocial { get; init; } = string.Empty;

    [StringLength(200)]
    public string? NomeFantasia { get; init; }

    [Required]
    [RegularExpression(
        "^\\d{14}$",
        ErrorMessage = "CNPJ must contain exactly 14 digits."
    )]
    public string Cnpj { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Setor { get; init; } = string.Empty;

    public bool Ativa { get; init; }

    public List<MetaReducaoDto> MetasReducao { get; init; } =
        new();

    public GovernancaEsgDto? Governanca { get; init; }
}
