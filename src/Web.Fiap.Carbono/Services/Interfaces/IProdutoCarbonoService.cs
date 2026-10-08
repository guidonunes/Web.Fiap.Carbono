using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Services.Interfaces;

public interface IProdutoCarbonoService
{
    Task<ProdutoModel?> GetProdutoComEmissoesAsync(int idProduto);
}