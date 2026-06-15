namespace Web.Fiap.Carbono.Models;

public class ProdutoModel
{
    public int IdProduto { get; set; }

    public int IdEmpresa { get; set; }

    public string NomeProduto { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public string TipoCategoria { get; set; } = string.Empty;

    public string UnidadeMedida { get; set; } = string.Empty;

    public string Ativo { get; set; } = "S";

    public DateTime DataCadastro { get; set; }

    public EmpresaModel? Empresa { get; set; }

    public ICollection<LoteProducaoModel> LotesProducao { get; set; } = new List<LoteProducaoModel>();
}