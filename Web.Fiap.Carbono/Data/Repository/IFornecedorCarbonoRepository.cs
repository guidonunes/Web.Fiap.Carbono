using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Data.Repository;

public interface IFornecedorCarbonoRepository
{
    Task<IReadOnlyList<FornecedorModel>> GetFornecedoresComEmissoesPagedAsync(int pageNumber, int pageSize);

    Task<int> CountFornecedoresComEmissoesAsync();
}