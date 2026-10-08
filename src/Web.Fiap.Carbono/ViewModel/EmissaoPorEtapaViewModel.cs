namespace Web.Fiap.Carbono.ViewModel;

public class EmissaoPorEtapaViewModel
{
    public int IdEtapa { get; set; }

    public string TipoEtapa { get; set; } = string.Empty;

    public decimal TotalCo2e { get; set; }
}