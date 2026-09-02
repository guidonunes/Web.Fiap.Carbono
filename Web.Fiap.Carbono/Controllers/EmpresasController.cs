using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Dtos.MongoDb.Empresas;
using Web.Fiap.Carbono.Mapping.MongoDb;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/empresas")]
[Tags("MongoDB Companies")]
[Produces("application/json")]
public sealed class EmpresasController : ControllerBase
{
    private readonly IMongoEmpresaService _service;

    public EmpresasController(IMongoEmpresaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Cadastra uma nova empresa ESG no MongoDB.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN,ANALISTA_ESG")]
    [ProducesResponseType(
        typeof(EmpresaResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmpresaResponse>> Create(
        [FromBody] CreateEmpresaRequest request,
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
    /// Retorna todas as empresas cadastradas no MongoDB.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<EmpresaResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<EmpresaResponse>>>
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
    /// Consulta uma empresa pelo ObjectId.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(
        typeof(EmpresaResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmpresaResponse>> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var document = await _service.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(document.ToResponse());
    }

    /// <summary>
    /// Atualiza integralmente uma empresa.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,ANALISTA_ESG")]
    [ProducesResponseType(
        typeof(EmpresaResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmpresaResponse>> Update(
        [FromRoute] string id,
        [FromBody] UpdateEmpresaRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _service.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(document.ToResponse());
    }

    /// <summary>
    /// Remove um registro temporário ou desativa uma empresa permanente.
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
