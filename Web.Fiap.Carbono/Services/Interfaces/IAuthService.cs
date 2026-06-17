using Web.Fiap.Carbono.ViewModel;

namespace Web.Fiap.Carbono.Services.Interfaces;

public interface IAuthService
{
    TokenViewModel Login(LoginViewModel loginViewModel);
}