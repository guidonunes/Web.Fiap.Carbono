using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Data.MongoDb.Repositories;
using Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;
using Web.Fiap.Carbono.Exceptions;
using Web.Fiap.Carbono.Mapping;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;
using Web.Fiap.Carbono.ViewModel.MongoDb;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/emissoes-carbono")]
[Tags("Carbon Emissions")]
public class EmissoesCarbonoController : ControllerBase
{
    private readonly IMongoEmissaoCarbonoService _mongoEmissaoCarbonoService;

    public EmissoesCarbonoController(
        IMongoEmissaoCarbonoService mongoEmissaoCarbonoService)
    {
        _mongoEmissaoCarbonoService = mongoEmissaoCarbonoService;
    }

    [HttpGet]
    public async Task<ActionResult<MongoPagedResult<EmissaoCarbonoMongoResponseViewModel>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _mongoEmissaoCarbonoService.GetPaginatedAsync(
            pageNumber,
            pageSize,
            cancellationToken);

        return Ok(new MongoPagedResult<EmissaoCarbonoMongoResponseViewModel>(
            result.Items.Select(item => item.ToResponse()).ToList(),
            result.TotalItems,
            result.PageNumber,
            result.PageSize));
    }

    [HttpGet("{idEmissao:int}")]
    public async Task<ActionResult<EmissaoCarbonoMongoResponseViewModel>> GetById(
        int idEmissao,
        CancellationToken cancellationToken)
    {
        var emission = await _mongoEmissaoCarbonoService.GetByLegacyIdAsync(
            idEmissao,
            cancellationToken);

        return Ok(emission.ToResponse());
    }
    
    [HttpGet("mongodb/{id}")]
    [ProducesResponseType(
        typeof(EmissaoCarbonoMongoResponseViewModel),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmissaoCarbonoMongoResponseViewModel>>
        GetMongoDbById(
            [FromRoute] string id,
            CancellationToken cancellationToken)
    {
        var document =
            await _mongoEmissaoCarbonoService.GetByIdAsync(
                id,
                cancellationToken);

        return Ok(document.ToResponse());
    }

    [Authorize(Roles = "ADMIN,ANALISTA_ESG")]
    [HttpPut("mongodb/{id}")]
    [ProducesResponseType(
        typeof(EmissaoCarbonoMongoResponseViewModel),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmissaoCarbonoMongoResponseViewModel>>
        UpdateMongoDbAudit(
            [FromRoute] string id,
            [FromBody] AtualizarEmissaoMongoRequest request,
            CancellationToken cancellationToken)
    {
        var document = await _mongoEmissaoCarbonoService.UpdateAuditAsync(
            id,
            request.Observacao,
            User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
            cancellationToken);

        return Ok(document.ToResponse());
    }

    [Authorize(Roles = "ADMIN")]
    [HttpDelete("mongodb/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteMongoDbTemporary(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        await _mongoEmissaoCarbonoService.DeleteTemporaryAsync(
            id,
            cancellationToken);

        return NoContent();
    }

    [Authorize(Roles = "ADMIN,ANALISTA_ESG")]
    [HttpPost("calcular")]
    [ProducesResponseType(
        typeof(EmissaoCarbonoMongoResponseViewModel),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<EmissaoCarbonoMongoResponseViewModel>>
        CalcularEmissao(
            [FromBody] CalcularEmissaoMongoRequest request,
            CancellationToken cancellationToken)
    {
        var calculatedBy =
            User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrWhiteSpace(calculatedBy))
        {
            throw new UnauthorizedAccessDomainException(
                "O usuário autenticado não possui um e-mail válido.");
        }

        var document =
            await _mongoEmissaoCarbonoService.CalculateAsync(
                request,
                calculatedBy,
                cancellationToken);

        var response = document.ToResponse();

        return CreatedAtAction(
            nameof(GetMongoDbById),
            new { id = response.Id },
            response);
    }
}
