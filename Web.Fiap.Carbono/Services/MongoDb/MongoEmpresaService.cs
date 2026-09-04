using MongoDB.Bson;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Exceptions;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;
using Web.Fiap.Carbono.Dtos.MongoDb.Analytics;
using Web.Fiap.Carbono.Dtos.MongoDb.Empresas;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Models.Documents.Embedded;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;
using Web.Fiap.Carbono.Services.MongoDb.Interfaces;

namespace Web.Fiap.Carbono.Services.MongoDb;

public sealed class MongoEmpresaService : IMongoEmpresaService
{
    private readonly IMongoEmpresaRepository _empresaRepository;
    private readonly IMongoProdutoRepository _produtoRepository;
    private readonly IMongoFornecedorRepository _fornecedorRepository;
    private readonly IMongoEmissaoCarbonoRepository _emissaoRepository;

    public MongoEmpresaService(
        IMongoEmpresaRepository empresaRepository,
        IMongoProdutoRepository produtoRepository,
        IMongoFornecedorRepository fornecedorRepository,
        IMongoEmissaoCarbonoRepository emissaoRepository)
    {
        _empresaRepository = empresaRepository;
        _produtoRepository = produtoRepository;
        _fornecedorRepository = fornecedorRepository;
        _emissaoRepository = emissaoRepository;
    }

    public async Task<EmpresaDocument> CreateAsync(
        CreateEmpresaRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var now = DateTime.UtcNow;

        var document = new EmpresaDocument
        {
            Id = ObjectId.GenerateNewId(),
            Codigo = MongoServiceRules.Required(request.Codigo, "codigo", 50),
            RazaoSocial = MongoServiceRules.Required(
                request.RazaoSocial, "razaoSocial"),
            NomeFantasia = request.NomeFantasia?.Trim(),
            Cnpj = MongoServiceRules.ValidateCnpj(request.Cnpj),
            Setor = MongoServiceRules.Required(request.Setor, "setor", 100),
            Ativa = request.Ativa,
            MetasReducao = request.MetasReducao?
                .Select(meta => new MetaReducao
                {
                    Tipo = MongoServiceRules.Required(
                        meta.Tipo,
                        "tipoMetaReducao",
                        50),
                    AnoBase = meta.AnoBase,
                    AnoMeta = meta.AnoMeta,
                    PercentualReducao = MongoServiceRules.Percentage(
                        meta.PercentualReducao,
                        "percentualReducao")
                })
                .ToList() ?? [],
            Governanca = request.Governanca is null
                ? null
                : new GovernancaEsg
                {
                    ResponsavelEsg = MongoServiceRules.Required(
                        request.Governanca.ResponsavelEsg,
                        "responsavelEsg",
                        150),
                    ComiteEsg = request.Governanca.ComiteEsg,
                    FrequenciaAuditoria =
                        MongoServiceRules.Required(
                            request.Governanca.FrequenciaAuditoria,
                            "frequenciaAuditoria",
                            30),
                    RelatorioPublico = request.Governanca.RelatorioPublico,
                    CanalDenuncias = request.Governanca.CanalDenuncias,
                    ConselhoSupervisao =
                        request.Governanca.ConselhoSupervisao
                },
            CriadoEm = now,
            AtualizadoEm = now,
            SchemaVersion = 1
        };

        ValidateCompany(document);

        try
        {
            await _empresaRepository.CreateAsync(document, cancellationToken);
            return document;
        }
        catch (MongoDuplicateKeyException exception)
        {
            throw new ConflictException(
                "Já existe uma empresa com o CNPJ informado.",
                exception);
        }
    }

    public async Task<EmpresaDocument> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var objectId = MongoServiceRules.ParseObjectId(id);

        return await _empresaRepository.GetByIdAsync(
                   objectId.ToString(),
                   cancellationToken)
               ?? throw new NotFoundException("Empresa não encontrada.");
    }

    public Task<IReadOnlyList<EmpresaDocument>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return _empresaRepository.GetAllAsync(cancellationToken);
    }

    public async Task<EmpresaDocument> UpdateAsync(
        string id,
        UpdateEmpresaRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var objectId = MongoServiceRules.ParseObjectId(id);
        var current = await GetByIdAsync(id, cancellationToken);

        var updated = new EmpresaDocument
        {
            Id = current.Id,
            LegacyId = current.LegacyId,
            Codigo = MongoServiceRules.Required(request.Codigo, "codigo", 50),
            RazaoSocial = MongoServiceRules.Required(
                request.RazaoSocial, "razaoSocial"),
            NomeFantasia = request.NomeFantasia?.Trim(),
            Cnpj = MongoServiceRules.ValidateCnpj(request.Cnpj),
            Setor = MongoServiceRules.Required(request.Setor, "setor", 100),
            Ativa = request.Ativa,
            MetasReducao = request.MetasReducao?
                .Select(meta => new MetaReducao
                {
                    Tipo = MongoServiceRules.Required(
                        meta.Tipo,
                        "tipoMetaReducao",
                        50),
                    AnoBase = meta.AnoBase,
                    AnoMeta = meta.AnoMeta,
                    PercentualReducao = MongoServiceRules.Percentage(
                        meta.PercentualReducao,
                        "percentualReducao")
                })
                .ToList() ?? [],
            Governanca = request.Governanca is null
                ? null
                : new GovernancaEsg
                {
                    ResponsavelEsg = MongoServiceRules.Required(
                        request.Governanca.ResponsavelEsg,
                        "responsavelEsg",
                        150),
                    ComiteEsg = request.Governanca.ComiteEsg,
                    FrequenciaAuditoria =
                        MongoServiceRules.Required(
                            request.Governanca.FrequenciaAuditoria,
                            "frequenciaAuditoria",
                            30),
                    RelatorioPublico = request.Governanca.RelatorioPublico,
                    CanalDenuncias = request.Governanca.CanalDenuncias,
                    ConselhoSupervisao =
                        request.Governanca.ConselhoSupervisao
                },
            CriadoEm = current.CriadoEm,
            AtualizadoEm = DateTime.UtcNow,
            SchemaVersion = current.SchemaVersion
        };

        ValidateCompany(updated);

        try
        {
            var changed = await _empresaRepository.UpdateAsync(
                objectId.ToString(),
                updated,
                cancellationToken);

            if (!changed)
            {
                throw new NotFoundException("Empresa não encontrada.");
            }

            return updated;
        }
        catch (MongoDuplicateKeyException exception)
        {
            throw new ConflictException(
                "Já existe uma empresa com o CNPJ informado.",
                exception);
        }
    }

    public async Task DeleteAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var document = await GetByIdAsync(id, cancellationToken);

        if (await _produtoRepository.ExistsByEmpresaIdAsync(
                document.Id.ToString(),
                cancellationToken) ||
            await _emissaoRepository.ExistsByEmpresaIdAsync(
                document.Id.ToString(),
                cancellationToken))
        {
            throw new ConflictException(
                "A empresa não pode ser excluída porque possui referências.");
        }

        if (MongoServiceRules.IsTemporaryRecord(document.Codigo))
        {
            var deleted = await _empresaRepository.DeleteAsync(
                document.Id.ToString(),
                cancellationToken);

            if (!deleted)
            {
                throw new NotFoundException("Empresa não encontrada.");
            }

            return;
        }

        var deactivated = new EmpresaDocument
        {
            Id = document.Id,
            LegacyId = document.LegacyId,
            Codigo = document.Codigo,
            RazaoSocial = document.RazaoSocial,
            NomeFantasia = document.NomeFantasia,
            Cnpj = document.Cnpj,
            Setor = document.Setor,
            Ativa = false,
            MetasReducao = document.MetasReducao,
            Governanca = document.Governanca,
            CriadoEm = document.CriadoEm,
            AtualizadoEm = DateTime.UtcNow,
            SchemaVersion = document.SchemaVersion
        };

        var updated = await _empresaRepository.UpdateAsync(
            document.Id.ToString(),
            deactivated,
            cancellationToken);

        if (!updated)
        {
            throw new NotFoundException("Empresa não encontrada.");
        }
    }

    public async Task<DashboardEmpresaMongoResponse> GetDashboardAsync(
      string empresaId,
      CancellationToken cancellationToken = default)
  {
      var empresa = await GetByIdAsync(
          empresaId,
          cancellationToken);

      var dashboard =
          await _emissaoRepository.GetCompanyDashboardAsync(
              empresa.Id.ToString(),
              cancellationToken);

      if (dashboard is null)
      {
          return new DashboardEmpresaMongoResponse
          {
              IdEmpresa = empresa.Id.ToString(),
              NomeEmpresa =
                  empresa.NomeFantasia ?? empresa.RazaoSocial,
              Unidade = "kgCO2e"
          };
      }

      var produtoMaisEmissor =
          dashboard.PorProduto.FirstOrDefault();

      var fornecedorMaisEmissor =
          dashboard.PorFornecedor.FirstOrDefault();

      var produtoTask = produtoMaisEmissor is null
          ? Task.FromResult<ProdutoDocument?>(null)
          : _produtoRepository.GetByIdAsync(
              produtoMaisEmissor.ProdutoId.ToString(),
              cancellationToken);

      var fornecedorTask = fornecedorMaisEmissor is null
          ? Task.FromResult<FornecedorDocument?>(null)
          : _fornecedorRepository.GetByIdAsync(
              fornecedorMaisEmissor.FornecedorId.ToString(),
              cancellationToken);

      await Task.WhenAll(produtoTask, fornecedorTask);

      var produto = await produtoTask;
      var fornecedor = await fornecedorTask;

      var quantidadeProdutos = dashboard.PorProduto.Count;

      return new DashboardEmpresaMongoResponse
      {
          IdEmpresa = empresa.Id.ToString(),
          NomeEmpresa =
              empresa.NomeFantasia ?? empresa.RazaoSocial,
          TotalCo2e = dashboard.TotalKgCO2e,
          Unidade = "kgCO2e",
          QuantidadeProdutos = quantidadeProdutos,
          QuantidadeEmissoes =
              dashboard.QuantidadeEmissoes,
          MediaEmissaoPorProduto =
              quantidadeProdutos == 0
                  ? 0m
                  : dashboard.TotalKgCO2e / quantidadeProdutos,
          ProdutoMaisEmissor = produto?.Nome,
          FornecedorMaisEmissor =
              fornecedor?.NomeFantasia
              ?? fornecedor?.RazaoSocial,
          EmissoesPorMes = dashboard.PorMes
              .Select(item => new EmissaoPorMesMongoResponse
              {
                  Ano = item.Ano,
                  Mes = item.Mes,
                  TotalCo2e = item.TotalKgCO2e
              })
              .ToList(),
          EmissoesPorEscopo = dashboard.PorEscopo
              .Select(item => new EmissaoPorEscopoMongoResponse
              {
                  Escopo = item.Escopo,
                  TotalCo2e = item.TotalKgCO2e
              })
              .ToList()
      };
  }

    private static void ValidateCompany(EmpresaDocument document)
    {
        foreach (var meta in document.MetasReducao)
        {
            if (meta.AnoMeta <= meta.AnoBase)
            {
                throw new BadRequestException(
                    "O ano-meta deve ser posterior ao ano-base.");
            }
        }
    }
}
