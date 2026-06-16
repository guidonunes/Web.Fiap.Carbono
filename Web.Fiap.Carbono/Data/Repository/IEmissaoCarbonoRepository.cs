using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Data.Repository;

public interface IEmissaoCarbonoRepository
{
    Task<IReadOnlyList<EmissaoCarbonoModel>> GetPagedAsync(int pageNumber, int pageSize);

    Task<int> CountAsync();

    Task<EmissaoCarbonoModel?> GetByIdAsync(int idEmissao);
}