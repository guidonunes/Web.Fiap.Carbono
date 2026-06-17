using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Web.Fiap.Carbono.Services.Interfaces;
using Web.Fiap.Carbono.ViewModel;

namespace Web.Fiap.Carbono.Controllers;

[ApiController]
[Route("api/dashboard-carbono")]
[Tags("Carbon Dashboard")]
public class DashboardCarbonoController : ControllerBase
{
    private readonly IDashboardCarbonoService _dashboardCarbonoService;
    private readonly IMapper _mapper;

    public DashboardCarbonoController(
        IDashboardCarbonoService dashboardCarbonoService,
        IMapper mapper)
    {
        _dashboardCarbonoService = dashboardCarbonoService;
        _mapper = mapper;
    }

    [HttpGet("empresas/{idEmpresa:int}/resumo")]
    public async Task<ActionResult<DashboardCarbonoViewModel>> GetResumoEmpresa(int idEmpresa)
    {
        try
        {
            var empresa = await _dashboardCarbonoService.GetEmpresaComDadosCarbonoAsync(idEmpresa);

            if (empresa is null)
            {
                return NotFound(new
                {
                    message = "Empresa não encontrada."
                });
            }

            var dashboardViewModel = _mapper.Map<DashboardCarbonoViewModel>(empresa);

            return Ok(dashboardViewModel);
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