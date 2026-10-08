namespace Web.Fiap.Carbono.Models;

public class FornecedorModel
{
    public int IdFornecedor { get; set; }

    public string NomeFornecedor { get; set; } = string.Empty;

    public string Cnpj { get; set; } = string.Empty;

    public string TipoFornecedor { get; set; } = string.Empty;

    public string? Cidade { get; set; }

    public string? Estado { get; set; }

    public string Pais { get; set; } = string.Empty;

    public string? CertificacaoEsg { get; set; }

    public string Ativo { get; set; } = "S";

    public ICollection<EtapaCadeiaModel> EtapasCadeia { get; set; } = new List<EtapaCadeiaModel>();
}