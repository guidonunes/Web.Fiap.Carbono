namespace Web.Fiap.Carbono.Models;

public class EmpresaModel
{
    public int IdEmpresa { get; set; }

    public string NomeEmpresa { get; set; } = string.Empty;

    public string Cnpj { get; set; } = string.Empty;

    public string? SetorAtuacao { get; set; }

    public string? Cidade { get; set; }

    public string? Estado { get; set; }

    public string Pais { get; set; } = string.Empty;

    public DateTime DataCadastro { get; set; }

    public ICollection<ProdutoModel> Produtos { get; set; } = new List<ProdutoModel>();
}