namespace Web.Fiap.Carbono.ViewModel;

public class TokenViewModel
{
    public string Token { get; set; } = string.Empty;

    public string Tipo { get; set; } = "Bearer";

    public DateTime ExpiraEm { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}