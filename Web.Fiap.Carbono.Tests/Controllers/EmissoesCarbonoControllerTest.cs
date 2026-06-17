using System.Net;
using Web.Fiap.Carbono.Tests.Config;

namespace Web.Fiap.Carbono.Tests.Controllers;

public class EmissoesCarbonoControllerTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EmissoesCarbonoControllerTest(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/emissoes-carbono?pageNumber=1&pageSize=10";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetById_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/emissoes-carbono/1";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Get_WithInvalidPageSize_ReturnsBadRequest()
    {
        // Arrange
        var request = "/api/emissoes-carbono?pageNumber=1&pageSize=100";

        // Act
        var response = await _client.GetAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}