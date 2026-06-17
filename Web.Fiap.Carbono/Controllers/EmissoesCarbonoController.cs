using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Services.Interfaces;
using Web.Fiap.Carbono.ViewModel;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/emissoes-carbono")]
public class EmissoesCarbonoController : ControllerBase
{
    private readonly IEmissaoCarbonoService _emissaoCarbonoService;
    private readonly IMapper _mapper;

    public EmissoesCarbonoController(
        IEmissaoCarbonoService emissaoCarbonoService,
        IMapper mapper)
    {
        _emissaoCarbonoService = emissaoCarbonoService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<PaginationViewModel<EmissaoCarbonoViewModel>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var emissoes = await _emissaoCarbonoService.GetPagedAsync(pageNumber, pageSize);
            var totalItems = await _emissaoCarbonoService.CountAsync();

            var emissoesViewModel = _mapper.Map<IEnumerable<EmissaoCarbonoViewModel>>(emissoes);

            var result = new PaginationViewModel<EmissaoCarbonoViewModel>
            {
                Items = emissoesViewModel,
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

    [HttpGet("{idEmissao:int}")]
    public async Task<ActionResult<EmissaoCarbonoViewModel>> GetById(int idEmissao)
    {
        try
        {
            var emissao = await _emissaoCarbonoService.GetByIdAsync(idEmissao);

            if (emissao is null)
            {
                return NotFound(new
                {
                    message = "Emissão de carbono não encontrada."
                });
            }

            var emissaoViewModel = _mapper.Map<EmissaoCarbonoViewModel>(emissao);

            return Ok(emissaoViewModel);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
    
    [HttpPost("calcular")]
    public async Task<ActionResult<EmissaoCarbonoViewModel>> CalcularEmissao(
        [FromBody] CalcularEmissaoCarbonoViewModel viewModel)
    {
        var emissaoCriada = await _emissaoCarbonoService.CalcularEmissaoAsync(viewModel);

        var emissaoViewModel = _mapper.Map<EmissaoCarbonoViewModel>(emissaoCriada);

        return CreatedAtAction(
            nameof(GetById),
            new { idEmissao = emissaoCriada.IdEmissao },
            emissaoViewModel
        );
    }
}