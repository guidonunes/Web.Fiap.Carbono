namespace Web.Fiap.Carbono.Models;

public class LoteProducaoModel
{
    public int IdLote { get; set; }

    public int IdProduto { get; set; }

    public string CodigoLote { get; set; } = string.Empty;

    public decimal Quantidade { get; set; }

    public DateTime DataProducao { get; set; }

    public DateTime DataValidade { get; set; }

    public string StatusLote { get; set; } = string.Empty;

    public ProdutoModel? Produto { get; set; }

    public ICollection<EtapaCadeiaModel> EtapasCadeia { get; set; } = new List<EtapaCadeiaModel>();
}