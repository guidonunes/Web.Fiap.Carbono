using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Dtos.MongoDb.Fornecedores;
using Web.Fiap.Carbono.Mapping.MongoDb;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/fornecedores")]
[Tags("MongoDB Suppliers")]
[Produces("application/json")]
public sealed class FornecedoresController : ControllerBase
{
    private readonly IMongoFornecedorService _service;

    public FornecedoresController(IMongoFornecedorService service)
    {
        _service = service;
    }

    /// <summary>
    /// Cadastra um novo fornecedor com informações ESG.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN,ANALISTA_ESG")]
    [ProducesResponseType(
        typeof(FornecedorResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FornecedorResponse>> Create(
        [FromBody] CreateFornecedorRequest request,
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
    /// Retorna todos os fornecedores cadastrados no MongoDB.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<FornecedorResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FornecedorResponse>>>
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
    /// Consulta um fornecedor pelo ObjectId.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(
        typeof(FornecedorResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FornecedorResponse>> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var document = await _service.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(document.ToResponse());
    }

    /// <summary>
    /// Atualiza integralmente um fornecedor.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,ANALISTA_ESG")]
    [ProducesResponseType(
        typeof(FornecedorResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FornecedorResponse>> Update(
        [FromRoute] string id,
        [FromBody] UpdateFornecedorRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _service.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(document.ToResponse());
    }

    /// <summary>
    /// Remove um fornecedor temporário ou desativa um fornecedor permanente.
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
