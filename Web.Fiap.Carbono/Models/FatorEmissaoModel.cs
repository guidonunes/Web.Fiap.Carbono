namespace Web.Fiap.Carbono.Models;

public class FatorEmissaoModel
{
    public int IdFator { get; set; }

    public string Fonte { get; set; } = string.Empty;

    public string Escopo { get; set; } = string.Empty;

    public string UnidadeBase { get; set; } = string.Empty;

    public decimal ValorFatorCo2e { get; set; }

    public string? Referencia { get; set; }

    public string Ativo { get; set; } = "S";

    public DateTime DataCadastro { get; set; }

    public ICollection<EmissaoCarbonoModel> EmissoesCarbono { get; set; } = new List<EmissaoCarbonoModel>();
}