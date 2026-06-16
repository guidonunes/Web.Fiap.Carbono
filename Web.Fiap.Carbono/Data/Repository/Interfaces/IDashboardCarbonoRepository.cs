using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Data.Repository;

public interface IDashboardCarbonoRepository
{
    Task<EmpresaModel?> GetEmpresaComDadosCarbonoAsync(int idEmpresa);
}