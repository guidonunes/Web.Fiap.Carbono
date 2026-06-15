namespace Web.Fiap.Carbono.Models;

public class EmissaoCarbonoModel
{
    public int IdEmissao { get; set; }

    public int IdFator { get; set; }

    public int IdEtapa { get; set; }

    public string FonteEmissao { get; set; } = string.Empty;

    public decimal QuantidadeEmitida { get; set; }

    public decimal QuantidadeAtividade { get; set; }

    public string Unidade { get; set; } = string.Empty;

    public string? MetodoCalculo { get; set; }

    public string? Observacao { get; set; }

    public DateTime DataRegistro { get; set; }

    public EtapaCadeiaModel? EtapaCadeia { get; set; }

    public FatorEmissaoModel? FatorEmissao { get; set; }
}