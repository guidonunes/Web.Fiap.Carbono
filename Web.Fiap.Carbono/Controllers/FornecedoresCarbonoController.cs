using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Services.Interfaces;
using Web.Fiap.Carbono.ViewModel;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/fornecedores-carbono")]
public class FornecedoresCarbonoController : ControllerBase
{
    private readonly IFornecedorCarbonoService _fornecedorCarbonoService;
    private readonly IMapper _mapper;

    public FornecedoresCarbonoController(
        IFornecedorCarbonoService fornecedorCarbonoService,
        IMapper mapper)
    {
        _fornecedorCarbonoService = fornecedorCarbonoService;
        _mapper = mapper;
    }

    [HttpGet("ranking")]
    public async Task<ActionResult<PaginationViewModel<FornecedorRankingCarbonoViewModel>>> GetRanking(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var fornecedores = await _fornecedorCarbonoService
                .GetFornecedoresComEmissoesPagedAsync(pageNumber, pageSize);

            var totalItems = await _fornecedorCarbonoService
                .CountFornecedoresComEmissoesAsync();

            var fornecedoresViewModel = _mapper
                .Map<IEnumerable<FornecedorRankingCarbonoViewModel>>(fornecedores)
                .OrderByDescending(f => f.TotalCo2e)
                .ToList();

            var result = new PaginationViewModel<FornecedorRankingCarbonoViewModel>
            {
                Items = fornecedoresViewModel,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            return Ok(result);
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