using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Dtos.MongoDb.Analytics;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/dashboard-carbono")]
[Tags("Carbon Dashboard")]
[Produces("application/json")]
public sealed class DashboardCarbonoController : ControllerBase
{
    private readonly IMongoEmpresaService _empresaService;

    public DashboardCarbonoController(
        IMongoEmpresaService empresaService)
    {
        _empresaService = empresaService;
    }

    /// <summary>
    /// Retorna o resumo de emissões de uma empresa.
    /// </summary>
    [HttpGet("empresas/{idEmpresa}/resumo")]
    [ProducesResponseType(
        typeof(DashboardEmpresaMongoResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DashboardEmpresaMongoResponse>>
        GetResumoEmpresa(
            [FromRoute] string idEmpresa,
            CancellationToken cancellationToken)
    {
        var response = await _empresaService.GetDashboardAsync(
            idEmpresa,
            cancellationToken);

        return Ok(response);
    }
}