using System.ComponentModel.DataAnnotations;

namespace Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;

public sealed class CalcularEmissaoMongoRequest
{
    [Required]
    public string ProdutoId { get; init; } = string.Empty;

    [Required]
    public string FornecedorId { get; init; } = string.Empty;

    [Required]
    public string FatorEmissaoId { get; init; } = string.Empty;

    [Required]
    public LoteEmissaoRequest Lote { get; init; } = new();

    [Required]
    public EtapaEmissaoRequest Etapa { get; init; } = new();

    [Range(
        typeof(decimal),
        "0.0000000000000000000000000001",
        "79228162514264337593543950335",
        ErrorMessage = "A quantidade da atividade deve ser maior que zero."
    )]
    public decimal QuantidadeAtividade { get; init; }

    [Required]
    [StringLength(50)]
    public string UnidadeAtividade { get; init; } = string.Empty;

    [Required]
    public DadosAtividadeRequest DadosAtividade { get; init; } = null!;

    [StringLength(200)]
    public string? FonteEmissao { get; init; }

    [StringLength(1000)]
    public string? Observacao { get; init; }
}
