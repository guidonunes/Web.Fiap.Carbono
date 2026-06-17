using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Services.Interfaces;
using Web.Fiap.Carbono.ViewModel;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/auth")]
[Tags("Authentication")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public ActionResult<TokenViewModel> Login([FromBody] LoginViewModel loginViewModel)
    {
        var token = _authService.Login(loginViewModel);

        return Ok(token);
    }
}