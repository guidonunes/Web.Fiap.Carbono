using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Data.Repository;

public interface IProdutoCarbonoRepository
{
    Task<ProdutoModel?> GetProdutoComEmissoesAsync(int idProduto);
}