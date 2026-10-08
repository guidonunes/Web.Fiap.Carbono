namespace Web.Fiap.Carbono.ViewModel;

public class DashboardCarbonoViewModel
{
    public int IdEmpresa { get; set; }

    public string NomeEmpresa { get; set; } = string.Empty;

    public decimal TotalCo2e { get; set; }

    public string Unidade { get; set; } = "kgCO2e";

    public int QuantidadeProdutos { get; set; }

    public int QuantidadeLotes { get; set; }

    public int QuantidadeEtapas { get; set; }

    public int QuantidadeEmissoes { get; set; }

    public string? ProdutoMaisEmissor { get; set; }

    public string? FornecedorMaisEmissor { get; set; }

    public decimal MediaEmissaoPorProduto { get; set; }

    public IEnumerable<EmissaoPorMesViewModel> EmissoesPorMes { get; set; } = new List<EmissaoPorMesViewModel>();
}