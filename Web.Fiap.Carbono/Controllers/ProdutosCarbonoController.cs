using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Services.Interfaces;
using Web.Fiap.Carbono.ViewModel;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/produtos-carbono")]
public class ProdutosCarbonoController : ControllerBase
{
    private readonly IProdutoCarbonoService _produtoCarbonoService;
    private readonly IMapper _mapper;

    public ProdutosCarbonoController(
        IProdutoCarbonoService produtoCarbonoService,
        IMapper mapper)
    {
        _produtoCarbonoService = produtoCarbonoService;
        _mapper = mapper;
    }

    [HttpGet("{idProduto:int}/pegada")]
    public async Task<ActionResult<ProdutoPegadaCarbonoViewModel>> GetPegadaCarbono(int idProduto)
    {
        try
        {
            var produto = await _produtoCarbonoService.GetProdutoComEmissoesAsync(idProduto);

            if (produto is null)
            {
                return NotFound(new
                {
                    message = "Produto não encontrado."
                });
            }

            var produtoViewModel = _mapper.Map<ProdutoPegadaCarbonoViewModel>(produto);

            return Ok(produtoViewModel);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}