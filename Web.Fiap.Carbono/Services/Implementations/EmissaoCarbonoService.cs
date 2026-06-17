using Web.Fiap.Carbono.Exceptions;
using Web.Fiap.Carbono.Models;
using Web.Fiap.Carbono.Data.Repository.Interfaces;
using Web.Fiap.Carbono.Services.Interfaces;
using Web.Fiap.Carbono.ViewModel;

namespace Web.Fiap.Carbono.Services.Implementations;

public class EmissaoCarbonoService : IEmissaoCarbonoService
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
            throw new DomainValidationException("O ID da emissão deve ser maior que zero.");

        return await _repository.GetByIdAsync(idEmissao);
    }

    public async Task<EmissaoCarbonoModel> CalcularEmissaoAsync(CalcularEmissaoCarbonoViewModel viewModel)
    {
        if (viewModel.IdEtapa <= 0)
            throw new DomainValidationException("O ID da etapa deve ser maior que zero.");

        if (viewModel.IdFator <= 0)
            throw new DomainValidationException("O ID do fator de emissão deve ser maior que zero.");

        if (viewModel.QuantidadeAtividade <= 0)
            throw new DomainValidationException("A quantidade da atividade deve ser maior que zero.");

        var etapa = await _repository.GetEtapaByIdAsync(viewModel.IdEtapa);

        if (etapa is null)
            throw new NotFoundException("Etapa da cadeia não encontrada.");

        var fator = await _repository.GetFatorByIdAsync(viewModel.IdFator);

        if (fator is null)
            throw new NotFoundException("Fator de emissão não encontrado.");

        if (fator.Ativo != "S")
            throw new BusinessRuleException("O fator de emissão informado está inativo.");

        var quantidadeEmitida = viewModel.QuantidadeAtividade * fator.ValorFatorCo2e;

        var emissaoCarbono = new EmissaoCarbonoModel
        {
            IdEtapa = viewModel.IdEtapa,
            IdFator = viewModel.IdFator,
            FonteEmissao = viewModel.FonteEmissao,
            QuantidadeAtividade = viewModel.QuantidadeAtividade,
            QuantidadeEmitida = quantidadeEmitida,
            Unidade = "kgCO2e",
            MetodoCalculo = "QuantidadeAtividade * ValorFatorCo2e",
            Observacao = viewModel.Observacao,
            DataRegistro = DateTime.Now
        };

        var emissaoCriada = await _repository.CreateAsync(emissaoCarbono);

        emissaoCriada.EtapaCadeia = etapa;
        emissaoCriada.FatorEmissao = fator;

        return emissaoCriada;
    }

    private static void ValidatePagination(int pageNumber, int pageSize)
    {
        if (pageNumber < 1)
            throw new DomainValidationException("O número da página deve ser maior ou igual a 1.");

        if (pageSize < 1)
            throw new DomainValidationException("O tamanho da página deve ser maior ou igual a 1.");

        if (pageSize > 50)
            throw new DomainValidationException("O tamanho da página não pode ser maior que 50.");
    }
}