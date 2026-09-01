using MongoDB.Bson;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Models.Documents.Embedded;
using Web.Fiap.Carbono.ViewModel.MongoDb;

namespace Web.Fiap.Carbono.Mapping;

public static class MongoDbDocumentMapper
{
    public static EmpresaMongoResponseViewModel ToResponse(this EmpresaDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new EmpresaMongoResponseViewModel
        {
            Id = document.Id.ToString(), LegacyId = document.LegacyId, Codigo = document.Codigo,
            RazaoSocial = document.RazaoSocial, NomeFantasia = document.NomeFantasia, Cnpj = document.Cnpj,
            Setor = document.Setor, Ativa = document.Ativa,
            MetasReducao = document.MetasReducao.Select(ToResponse).ToList(),
            Governanca = document.Governanca is null ? null : ToResponse(document.Governanca),
            CriadoEm = document.CriadoEm, AtualizadoEm = document.AtualizadoEm, SchemaVersion = document.SchemaVersion
        };
    }

    public static ProdutoMongoResponseViewModel ToResponse(this ProdutoDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new ProdutoMongoResponseViewModel
        {
            Id = document.Id.ToString(), LegacyId = document.LegacyId, EmpresaId = document.EmpresaId.ToString(),
            Codigo = document.Codigo, Nome = document.Nome, Categoria = document.Categoria,
            UnidadeFuncional = document.UnidadeFuncional,
            Ativo = document.Ativo,
            AtributosAmbientais = document.AtributosAmbientais is null ? null : ToResponse(document.AtributosAmbientais),
            CriadoEm = document.CriadoEm, AtualizadoEm = document.AtualizadoEm, SchemaVersion = document.SchemaVersion
        };
    }

    public static FornecedorMongoResponseViewModel ToResponse(this FornecedorDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new FornecedorMongoResponseViewModel
        {
            Id = document.Id.ToString(), LegacyId = document.LegacyId, Codigo = document.Codigo,
            RazaoSocial = document.RazaoSocial, NomeFantasia = document.NomeFantasia, Cnpj = document.Cnpj,
            Ativo = document.Ativo, CategoriasAtuacao = document.CategoriasAtuacao.ToList(),
            Certificacoes = document.Certificacoes.Select(ToResponse).ToList(),
            IndicadoresSociais = document.IndicadoresSociais is null ? null : ToResponse(document.IndicadoresSociais),
            ConformidadeAmbiental = document.ConformidadeAmbiental is null ? null : ToResponse(document.ConformidadeAmbiental),
            StatusAuditoria = document.StatusAuditoria, NivelRiscoEsg = document.NivelRiscoEsg,
            CriadoEm = document.CriadoEm, AtualizadoEm = document.AtualizadoEm, SchemaVersion = document.SchemaVersion
        };
    }

    public static FatorEmissaoMongoResponseViewModel ToResponse(this FatorEmissaoDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new FatorEmissaoMongoResponseViewModel
        {
            Id = document.Id.ToString(), LegacyId = document.LegacyId, Codigo = document.Codigo, Nome = document.Nome,
            Categoria = document.Categoria, Valor = document.Valor, UnidadeBase = document.UnidadeBase,
            Escopo = document.Escopo, Versao = document.Versao, FonteReferencia = document.FonteReferencia,
            Metodologia = document.Metodologia, ValidoDe = document.ValidoDe, ValidoAte = document.ValidoAte,
            Ativo = document.Ativo, CriadoEm = document.CriadoEm, AtualizadoEm = document.AtualizadoEm,
            SchemaVersion = document.SchemaVersion
        };
    }

    public static EmissaoCarbonoMongoResponseViewModel ToResponse(this EmissaoCarbonoDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        return new EmissaoCarbonoMongoResponseViewModel
        {
            Id = document.Id.ToString(), LegacyId = document.LegacyId, Codigo = document.Codigo,
            EmpresaId = document.EmpresaId.ToString(), ProdutoId = document.ProdutoId.ToString(),
            FornecedorId = document.FornecedorId.ToString(), FatorEmissaoId = document.FatorEmissaoId.ToString(),
            Lote = ToResponse(document.Lote), Etapa = ToResponse(document.Etapa),
            QuantidadeAtividade = document.QuantidadeAtividade, DadosAtividade = ToValues(document.DadosAtividade),
            FatorAplicado = ToResponse(document.FatorAplicado), QuantidadeEmitidaKgCO2e = document.QuantidadeEmitidaKgCO2e,
            MetodoCalculo = document.MetodoCalculo, FonteEmissao = document.FonteEmissao, Observacao = document.Observacao,
            CalculadoPor = document.CalculadoPor, DataEmissao = document.DataEmissao, RevisadoEm = document.RevisadoEm,
            RevisadoPor = document.RevisadoPor, CriadoEm = document.CriadoEm, AtualizadoEm = document.AtualizadoEm,
            SchemaVersion = document.SchemaVersion
        };
    }

    private static MetaReducaoResponseViewModel ToResponse(MetaReducao value) => new()
    {
        Tipo = value.Tipo, AnoBase = value.AnoBase, AnoMeta = value.AnoMeta, PercentualReducao = value.PercentualReducao
    };

    private static GovernancaEsgResponseViewModel ToResponse(GovernancaEsg value) => new()
    {
        ResponsavelEsg = value.ResponsavelEsg, ComiteEsg = value.ComiteEsg,
        FrequenciaAuditoria = value.FrequenciaAuditoria, RelatorioPublico = value.RelatorioPublico,
        CanalDenuncias = value.CanalDenuncias, ConselhoSupervisao = value.ConselhoSupervisao
    };

    private static AtributosAmbientaisResponseViewModel ToResponse(AtributosAmbientais value) => new()
    {
        Materiais = value.Materiais.Select(ToResponse).ToList(), PercentualReciclavel = value.PercentualReciclavel,
        PercentualMaterialReciclado = value.PercentualMaterialReciclado, Embalagem = value.Embalagem,
        SeloSustentabilidade = value.SeloSustentabilidade, CompensacaoCarbono = value.CompensacaoCarbono,
        OrigemAgriculturaOrganica = value.OrigemAgriculturaOrganica, VidaUtilAnos = value.VidaUtilAnos,
        CamposAdicionais = ToValues(value.CamposAdicionais)
    };

    private static MaterialProdutoResponseViewModel ToResponse(MaterialProduto value) => new()
    {
        Nome = value.Nome, PercentualComposicao = value.PercentualComposicao,
        OrigemRenovavel = value.OrigemRenovavel, OrigemReciclada = value.OrigemReciclada
    };

    private static CertificacaoResponseViewModel ToResponse(Certificacao value) => new()
    {
        Nome = value.Nome, Emissor = value.Emissor, ValidaAte = value.ValidaAte
    };

    private static IndicadoresSociaisResponseViewModel ToResponse(IndicadoresSociais value) => new()
    {
        AcidentesUltimos12Meses = value.AcidentesUltimos12Meses,
        PercentualMulheresLideranca = value.PercentualMulheresLideranca,
        PossuiProgramaDiversidade = value.PossuiProgramaDiversidade
    };

    private static ConformidadeAmbientalResponseViewModel ToResponse(ConformidadeAmbiental value) => new()
    {
        PossuiLicenca = value.PossuiLicenca, OcorrenciasUltimos12Meses = value.OcorrenciasUltimos12Meses,
        DescarteMonitorado = value.DescarteMonitorado, PercentualMaterialReciclado = value.PercentualMaterialReciclado,
        DataUltimaAuditoria = value.DataUltimaAuditoria,
        CamposAdicionais = ToValues(value.CamposAdicionais)
    };

    private static LoteSnapshotResponseViewModel ToResponse(LoteSnapshot value) => new()
    {
        Codigo = value.Codigo, QuantidadeProduzida = value.QuantidadeProduzida,
        Unidade = value.Unidade, DataProducao = value.DataProducao
    };

    private static EtapaSnapshotResponseViewModel ToResponse(EtapaSnapshot value) => new()
    {
        Nome = value.Nome, Ordem = value.Ordem, Categoria = value.Categoria, Local = value.Local
    };

    private static FatorEmissaoSnapshotResponseViewModel ToResponse(FatorEmissaoSnapshot value) => new()
    {
        Codigo = value.Codigo, Nome = value.Nome, Valor = value.Valor, UnidadeBase = value.UnidadeBase,
        Escopo = value.Escopo, Versao = value.Versao, FonteReferencia = value.FonteReferencia,
        Metodologia = value.Metodologia
    };

    private static IReadOnlyDictionary<string, object?> ToValues(BsonDocument? document) =>
        document is null
            ? new Dictionary<string, object?>()
            : document.Elements.ToDictionary(element => element.Name, element => ToValue(element.Value));

    private static object? ToValue(BsonValue value) => value.BsonType switch
    {
        BsonType.Null => null,
        BsonType.String => value.AsString,
        BsonType.Boolean => value.AsBoolean,
        BsonType.Int32 => value.AsInt32,
        BsonType.Int64 => value.AsInt64,
        BsonType.Double => value.AsDouble,
        BsonType.Decimal128 => Decimal128.ToDecimal(value.AsDecimal128),
        BsonType.DateTime => value.ToUniversalTime(),
        BsonType.ObjectId => value.AsObjectId.ToString(),
        BsonType.Document => ToValues(value.AsBsonDocument),
        BsonType.Array => value.AsBsonArray.Select(ToValue).ToList(),
        _ => value.ToString()
    };
}
