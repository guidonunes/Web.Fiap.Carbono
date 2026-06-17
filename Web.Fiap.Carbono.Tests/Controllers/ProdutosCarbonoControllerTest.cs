using Web.Fiap.Carbono.Tests.Config;

namespace Web.Fiap.Carbono.Tests.Controllers;

public class ProdutosCarbonoControllerTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProdutosCarbonoControllerTest(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetPegadaCarbono_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/produtos-carbono/1/pegada";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }
}