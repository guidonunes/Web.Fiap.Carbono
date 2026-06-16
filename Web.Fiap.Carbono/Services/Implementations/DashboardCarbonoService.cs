using Web.Fiap.Carbono.Data.Repository;
using Web.Fiap.Carbono.Models;
using Web.Fiap.Carbono.Services.Interfaces;

namespace Web.Fiap.Carbono.Services.Implementations;

public class DashboardCarbonoService: IDashboardCarbonoService
{
    private readonly IDashboardCarbonoRepository _repository;

    public DashboardCarbonoService(IDashboardCarbonoRepository repository)
    {
        _repository = repository;
    }

    public async Task<EmpresaModel?> GetEmpresaComDadosCarbonoAsync(int idEmpresa)
    {
        if (idEmpresa <= 0)
            throw new ArgumentException("O ID da empresa deve ser maior que zero.");

        return await _repository.GetEmpresaComDadosCarbonoAsync(idEmpresa);
    }
}