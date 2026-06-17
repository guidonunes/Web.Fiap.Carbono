using Web.Fiap.Carbono.Tests.Config;

namespace Web.Fiap.Carbono.Tests.Controllers;

public class DashboardCarbonoControllerTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DashboardCarbonoControllerTest(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetResumoEmpresa_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/dashboard-carbono/empresas/1/resumo";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }
}