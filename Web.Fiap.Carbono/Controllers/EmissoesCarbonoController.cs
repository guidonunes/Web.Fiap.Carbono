using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;
using Web.Fiap.Carbono.Exceptions;
using Web.Fiap.Carbono.Mapping;
using Web.Fiap.Carbono.Services.Interfaces;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;
using Web.Fiap.Carbono.ViewModel;
using Web.Fiap.Carbono.ViewModel.MongoDb;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/emissoes-carbono")]
[Tags("Carbon Emissions")]
public class EmissoesCarbonoController : ControllerBase
{
    private readonly IEmissaoCarbonoService _emissaoCarbonoService;
    private readonly IMapper _mapper;
    private readonly IMongoEmissaoCarbonoService _mongoEmissaoCarbonoService;

    public EmissoesCarbonoController(
        IEmissaoCarbonoService emissaoCarbonoService,
        IMongoEmissaoCarbonoService mongoEmissaoCarbonoService,
        IMapper mapper)
    {
        _emissaoCarbonoService = emissaoCarbonoService;
        _mongoEmissaoCarbonoService = mongoEmissaoCarbonoService;
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
