using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Services.Interfaces;

public interface IFornecedorCarbonoService
{
    Task<IReadOnlyList<FornecedorModel>> GetFornecedoresComEmissoesPagedAsync(int pageNumber, int pageSize);

    Task<int> CountFornecedoresComEmissoesAsync();
}