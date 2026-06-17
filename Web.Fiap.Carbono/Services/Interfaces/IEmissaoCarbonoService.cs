using Web.Fiap.Carbono.Models;
using Web.Fiap.Carbono.ViewModel;

namespace Web.Fiap.Carbono.Services.Interfaces;

public interface IEmissaoCarbonoService
{
    Task<IReadOnlyList<EmissaoCarbonoModel>> GetPagedAsync(int pageNumber, int pageSize);

    Task<int> CountAsync();

    Task<EmissaoCarbonoModel?> GetByIdAsync(int idEmissao);

    Task<EmissaoCarbonoModel> CalcularEmissaoAsync(CalcularEmissaoCarbonoViewModel viewModel);
}