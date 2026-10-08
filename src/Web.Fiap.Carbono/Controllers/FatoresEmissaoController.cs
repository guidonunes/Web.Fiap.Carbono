using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Dtos.MongoDb.FatoresEmissao;
using Web.Fiap.Carbono.Mapping.MongoDb;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/fatores-emissao")]
[Tags("MongoDB Emission Factors")]
[Produces("application/json")]
public sealed class FatoresEmissaoController : ControllerBase
{
    private readonly IMongoFatorEmissaoService _service;

    public FatoresEmissaoController(
        IMongoFatorEmissaoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Cadastra um fator de emissão versionado.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(
        typeof(FatorEmissaoResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<FatorEmissaoResponse>> Create(
        [FromBody] CreateFatorEmissaoRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _service.CreateAsync(
            request,
            cancellationToken);

        var response = document.ToResponse();

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response);
    }

    /// <summary>
    /// Retorna todos os fatores de emissão cadastrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<FatorEmissaoResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FatorEmissaoResponse>>>
        GetAll(CancellationToken cancellationToken)
    {
        var documents = await _service.GetAllAsync(
            cancellationToken);

        var response = documents
            .Select(document => document.ToResponse())
            .ToList();

        return Ok(response);
    }

    /// <summary>
    /// Consulta um fator de emissão pelo ObjectId.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(
        typeof(FatorEmissaoResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FatorEmissaoResponse>> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var document = await _service.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(document.ToResponse());
    }

    /// <summary>
    /// Atualiza um fator de emissão preservando sua identidade e auditoria.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(
        typeof(FatorEmissaoResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<FatorEmissaoResponse>> Update(
        [FromRoute] string id,
        [FromBody] UpdateFatorEmissaoRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _service.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(document.ToResponse());
    }

    /// <summary>
    /// Remove um fator temporário ou desativa um fator permanente.
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "ADMIN")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}
