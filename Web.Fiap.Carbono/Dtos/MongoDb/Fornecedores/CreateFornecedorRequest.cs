using System.ComponentModel.DataAnnotations;
using Web.Fiap.Carbono.Dtos.MongoDb.Common;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Fornecedores;

public sealed class CreateFornecedorRequest
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

    public bool Ativo { get; init; } = true;

    public List<string> CategoriasAtuacao { get; init; } =
        new();

    public List<CertificacaoDto> Certificacoes { get; init; } =
        new();

    public IndicadoresSociaisDto? IndicadoresSociais {
        get;
        init;
    }

    public ConformidadeAmbientalDto? ConformidadeAmbiental {
        get;
        init;
    }

    [StringLength(50)]
    public string? StatusAuditoria { get; init; }

    [StringLength(30)]
    public string? NivelRiscoEsg { get; init; }
}
