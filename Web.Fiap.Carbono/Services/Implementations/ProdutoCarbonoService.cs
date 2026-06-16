using Web.Fiap.Carbono.Data.Repository;
using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Services.Implementations;

public class ProdutoCarbonoService: IProdutoCarbonoRepository
{
    private readonly IProdutoCarbonoRepository _repository;

    public ProdutoCarbonoService(IProdutoCarbonoRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProdutoModel?> GetProdutoComEmissoesAsync(int idProduto)
    {
        if (idProduto <= 0)
            throw new ArgumentException("O ID do produto deve ser maior que zero.");

        return await _repository.GetProdutoComEmissoesAsync(idProduto);
    }
}