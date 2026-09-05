using Web.Fiap.Carbono.Data.Repository;
using Web.Fiap.Carbono.Models;
using Web.Fiap.Carbono.Services.Interfaces;

namespace Web.Fiap.Carbono.Services.Implementations;

public class FornecedorCarbonoService: IFornecedorCarbonoService
{
    private readonly IFornecedorCarbonoRepository _repository;

    public FornecedorCarbonoService(IFornecedorCarbonoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FornecedorModel>> GetFornecedoresComEmissoesPagedAsync(int pageNumber, int pageSize)
    {
        ValidatePagination(pageNumber, pageSize);

        return await _repository.GetFornecedoresComEmissoesPagedAsync(pageNumber, pageSize);
    }

    public async Task<int> CountFornecedoresComEmissoesAsync()
    {
        return await _repository.CountFornecedoresComEmissoesAsync();
    }

    private static void ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new ArgumentException("O número da página deve ser maior ou igual a 1.");

        if (pageSize < 1)
            throw new ArgumentException("O tamanho da página deve ser maior ou igual a 1.");

        if (pageSize > 50)
            throw new ArgumentException("O tamanho da página não pode ser maior que 50.");
    }
}