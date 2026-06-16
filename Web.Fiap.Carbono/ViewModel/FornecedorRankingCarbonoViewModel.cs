namespace Web.Fiap.Carbono.ViewModel;

public class FornecedorRankingCarbonoViewModel
{
    public int IdFornecedor { get; set; }

    public string NomeFornecedor { get; set; } = string.Empty;

    public string TipoFornecedor { get; set; } = string.Empty;

    public string? CertificacaoEsg { get; set; }

    public decimal TotalCo2e { get; set; }

    public int QuantidadeEtapas { get; set; }

    public int QuantidadeEmissoes { get; set; }
}