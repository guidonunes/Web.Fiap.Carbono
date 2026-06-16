using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Services.Interfaces;

public interface IDashboardCarbonoService
{
    Task<EmpresaModel?> GetEmpresaComDadosCarbonoAsync(int idEmpresa);
}