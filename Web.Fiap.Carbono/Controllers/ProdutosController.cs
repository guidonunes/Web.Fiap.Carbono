using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Dtos.MongoDb.Produtos;
using Web.Fiap.Carbono.Mapping.MongoDb;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/produtos")]
[Tags("MongoDB Products")]
[Produces("application/json")]
public sealed class ProdutosController : ControllerBase
{
    private readonly IMongoProdutoService _service;

    public ProdutosController(IMongoProdutoService service)
    {
        _service = service;
    }

    /// <summary>
    /// Cadastra um produto vinculado a uma empresa existente.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "ADMIN,ANALISTA_ESG")]
    [ProducesResponseType(
        typeof(ProdutoResponse),
        StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProdutoResponse>> Create(
        [FromBody] CreateProdutoRequest request,
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
    /// Retorna todos os produtos cadastrados no MongoDB.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ProdutoResponse>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProdutoResponse>>>
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
    /// Consulta um produto pelo ObjectId.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(
        typeof(ProdutoResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoResponse>> GetById(
        [FromRoute] string id,
        CancellationToken cancellationToken)
    {
        var document = await _service.GetByIdAsync(
            id,
            cancellationToken);

        return Ok(document.ToResponse());
    }

    /// <summary>
    /// Atualiza integralmente um produto.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "ADMIN,ANALISTA_ESG")]
    [ProducesResponseType(
        typeof(ProdutoResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProdutoResponse>> Update(
        [FromRoute] string id,
        [FromBody] UpdateProdutoRequest request,
        CancellationToken cancellationToken)
    {
        var document = await _service.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(document.ToResponse());
    }

    /// <summary>
    /// Remove um produto temporário ou desativa um produto permanente.
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
