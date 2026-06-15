namespace Web.Fiap.Carbono.Models;

public class EtapaCadeiaModel
{
    public int IdEtapa { get; set; }

    public string TipoEtapa { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public string? Origem { get; set; }

    public string? Destino { get; set; }

    public DateTime DataInicio { get; set; }

    public DateTime? DataFim { get; set; }

    public string? OrdemEtapa { get; set; }

    public int IdFornecedor { get; set; }

    public int IdLote { get; set; }

    public FornecedorModel? Fornecedor { get; set; }

    public LoteProducaoModel? LoteProducao { get; set; }

    public ICollection<EmissaoCarbonoModel> EmissoesCarbono { get; set; } = new List<EmissaoCarbonoModel>();
}