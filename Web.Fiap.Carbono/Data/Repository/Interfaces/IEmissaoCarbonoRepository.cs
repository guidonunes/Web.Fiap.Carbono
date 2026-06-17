using Web.Fiap.Carbono.Models;

namespace Web.Fiap.Carbono.Data.Repository.Interfaces;

public interface IEmissaoCarbonoRepository
{
    Task<IReadOnlyList<EmissaoCarbonoModel>> GetPagedAsync(int pageNumber, int pageSize);

    Task<int> CountAsync();

    Task<EmissaoCarbonoModel?> GetByIdAsync(int idEmissao);

    Task<EtapaCadeiaModel?> GetEtapaByIdAsync(int idEtapa);

    Task<FatorEmissaoModel?> GetFatorByIdAsync(int idFator);

    Task<EmissaoCarbonoModel> CreateAsync(EmissaoCarbonoModel emissaoCarbono);
}