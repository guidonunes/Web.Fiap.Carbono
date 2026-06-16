using Web.Fiap.Carbono.Data.Repository;
using Web.Fiap.Carbono.Models;
using Web.Fiap.Carbono.Services.Interfaces;

namespace Web.Fiap.Carbono.Services.Implementations;

public class EmissaoCarbonoService: IEmissaoCarbonoService
{
    private readonly IEmissaoCarbonoRepository _repository;

    public EmissaoCarbonoService(IEmissaoCarbonoRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<EmissaoCarbonoModel>> GetPagedAsync(int pageNumber, int pageSize)
    {
        ValidatePagination(pageNumber, pageSize);

        return await _repository.GetPagedAsync(pageNumber, pageSize);
    }

    public async Task<int> CountAsync()
    {
        return await _repository.CountAsync();
    }

    public async Task<EmissaoCarbonoModel?> GetByIdAsync(int idEmissao)
    {
        if (idEmissao <= 0)
            throw new ArgumentException("O ID da emissão deve ser maior que zero.");

        return await _repository.GetByIdAsync(idEmissao);
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