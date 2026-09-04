using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Data.MongoDb.Repositories;
using Web.Fiap.Carbono.Dtos.MongoDb.Analytics;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/fornecedores-carbono")]
[Tags("Carbon Suppliers")]
[Produces("application/json")]
public sealed class FornecedoresCarbonoController : ControllerBase
{
    private readonly IMongoFornecedorService _fornecedorService;

    public FornecedoresCarbonoController(
        IMongoFornecedorService fornecedorService)
    {
        _fornecedorService = fornecedorService;
    }

    /// <summary>
    /// Retorna o ranking de fornecedores por emissões.
    /// </summary>
    [HttpGet("ranking")]
    [ProducesResponseType(
        typeof(MongoPagedResult<FornecedorRankingMongoResponse>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<
            MongoPagedResult<FornecedorRankingMongoResponse>>>
        GetRanking(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
    {
        var response = await _fornecedorService.GetRankingAsync(
            pageNumber,
            pageSize,
            cancellationToken);

        return Ok(response);
    }
}