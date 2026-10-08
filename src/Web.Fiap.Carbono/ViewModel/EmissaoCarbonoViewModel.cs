namespace Web.Fiap.Carbono.ViewModel;

public class EmissaoCarbonoViewModel
{
    public int IdEmissao { get; set; }

    public int IdEtapa { get; set; }

    public string TipoEtapa { get; set; } = string.Empty;

    public int IdFator { get; set; }

    public string FonteFator { get; set; } = string.Empty;

    public string Escopo { get; set; } = string.Empty;

    public string UnidadeBase { get; set; } = string.Empty;

    public decimal ValorFatorCo2e { get; set; }

    public string FonteEmissao { get; set; } = string.Empty;

    public decimal QuantidadeAtividade { get; set; }

    public decimal QuantidadeEmitida { get; set; }

    public string Unidade { get; set; } = string.Empty;

    public string? MetodoCalculo { get; set; }

    public string? Observacao { get; set; }

    public DateTime DataRegistro { get; set; }
}