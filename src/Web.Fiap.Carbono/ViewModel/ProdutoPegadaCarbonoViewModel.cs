namespace Web.Fiap.Carbono.ViewModel;

public class ProdutoPegadaCarbonoViewModel
{
    public int IdProduto { get; set; }

    public string NomeProduto { get; set; } = string.Empty;

    public string NomeEmpresa { get; set; } = string.Empty;

    public decimal TotalCo2e { get; set; }

    public string Unidade { get; set; } = "kgCO2e";

    public IEnumerable<EmissaoPorEtapaViewModel> EmissoesPorEtapa { get; set; } = new List<EmissaoPorEtapaViewModel>();
}