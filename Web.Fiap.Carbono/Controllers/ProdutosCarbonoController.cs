using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Dtos.MongoDb.Analytics;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/produtos-carbono")]
[Tags("Carbon Products")]
[Produces("application/json")]
public sealed class ProdutosCarbonoController : ControllerBase
{
    private readonly IMongoProdutoService _produtoService;

    public ProdutosCarbonoController(
        IMongoProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    /// <summary>
    /// Retorna a pegada de carbono agregada de um produto.
    /// </summary>
    [HttpGet("{idProduto}/pegada")]
    [ProducesResponseType(
        typeof(ProdutoPegadaMongoResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoPegadaMongoResponse>>
        GetPegadaCarbono(
            [FromRoute] string idProduto,
            CancellationToken cancellationToken)
    {
        var response = await _produtoService.GetFootprintAsync(
            idProduto,
            cancellationToken);

        return Ok(response);
    }
}