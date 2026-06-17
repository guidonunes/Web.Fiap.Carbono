using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Web.Fiap.Carbono.Config.Security;
using Web.Fiap.Carbono.Exceptions;
using Web.Fiap.Carbono.Models;
using Web.Fiap.Carbono.Services.Interfaces;
using Web.Fiap.Carbono.ViewModel;

namespace Web.Fiap.Carbono.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;

    private readonly List<AuthUserModel> _users = new()
    {
        new AuthUserModel
        {
            Id = 1,
            Nome = "Administrador ESG",
            Email = "admin@carbono.com",
            Senha = "Carbono@123",
            Role = "ADMIN"
        },
        new AuthUserModel
        {
            Id = 2,
            Nome = "Analista ESG",
            Email = "analista@carbono.com",
            Senha = "Carbono@123",
            Role = "ANALISTA_ESG"
        }
    };

    public AuthService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public TokenViewModel Login(LoginViewModel loginViewModel)
    {
        var user = _users.FirstOrDefault(u =>
            u.Email.Equals(loginViewModel.Email, StringComparison.OrdinalIgnoreCase)
            && u.Senha == loginViewModel.Senha);

        if (user is null)
            throw new UnauthorizedAccessDomainException("E-mail ou senha inválidos.");

        var expiration = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Nome),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return new TokenViewModel
        {
            Token = token,
            ExpiraEm = expiration,
            Email = user.Email,
            Role = user.Role
        };
    }
}