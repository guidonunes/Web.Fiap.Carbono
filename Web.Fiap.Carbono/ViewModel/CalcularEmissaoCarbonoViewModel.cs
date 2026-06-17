using System.ComponentModel.DataAnnotations;

namespace Web.Fiap.Carbono.ViewModel;

public class CalcularEmissaoCarbonoViewModel
{
    [Required(ErrorMessage = "O ID da etapa é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "O ID da etapa deve ser maior que zero.")]
    public int IdEtapa { get; set; }

    [Required(ErrorMessage = "O ID do fator de emissão é obrigatório.")]
    [Range(1, int.MaxValue, ErrorMessage = "O ID do fator de emissão deve ser maior que zero.")]
    public int IdFator { get; set; }

    [Required(ErrorMessage = "A quantidade da atividade é obrigatória.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "A quantidade da atividade deve ser maior que zero.")]
    public decimal QuantidadeAtividade { get; set; }

    [Required(ErrorMessage = "A fonte da emissão é obrigatória.")]
    [StringLength(50, ErrorMessage = "A fonte da emissão deve ter no máximo 50 caracteres.")]
    public string FonteEmissao { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A observação deve ter no máximo 500 caracteres.")]
    public string? Observacao { get; set; }
}