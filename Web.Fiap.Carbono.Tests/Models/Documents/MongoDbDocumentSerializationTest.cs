using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Web.Fiap.Carbono.Mapping;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Models.Documents.Embedded;

namespace Web.Fiap.Carbono.Tests.Models.Documents;

public sealed class MongoDbDocumentSerializationTest
{
    private static readonly DateTime UtcDate = new(2026, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void EmpresaDocument_ShouldRoundTripBsonWithDecimalAndUtcDates()
    {
        var original = CreateEmpresa();

        var bson = original.ToBsonDocument();
        var result = BsonSerializer.Deserialize<EmpresaDocument>(bson);

        Assert.IsType<BsonObjectId>(bson["_id"]);
        Assert.IsType<BsonDecimal128>(bson["metasReducao"].AsBsonArray[0].AsBsonDocument["percentualReducao"]);
        Assert.Equal(original.Id, result.Id);
        Assert.Equal(30m, result.MetasReducao[0].PercentualReducao);
        Assert.Equal(DateTimeKind.Utc, result.CriadoEm.Kind);
        Assert.Equal("ANUAL", result.Governanca!.FrequenciaAuditoria);
    }

    [Fact]
    public void ProdutoDocument_ShouldRoundTripNestedMaterialsAndFlexibleAttributes()
    {
        var original = CreateProduto();

        var bson = original.ToBsonDocument();
        var result = BsonSerializer.Deserialize<ProdutoDocument>(bson);

        var attributes = bson["atributosAmbientais"].AsBsonDocument;
        Assert.False(bson.Contains("materiais"));
        Assert.IsType<BsonDecimal128>(attributes["percentualReciclavel"]);
        Assert.Equal("EMBALAGEM", result.Categoria);
        Assert.Equal("ALUMINIO", result.AtributosAmbientais!.Materiais[0].Nome);
        Assert.Equal(85m, result.AtributosAmbientais.Materiais[0].PercentualComposicao);
        Assert.Equal(45m, Decimal128.ToDecimal(result.AtributosAmbientais.CamposAdicionais!["percentualFrotaEletrificada"].AsDecimal128));
        Assert.Equal(DateTimeKind.Utc, result.AtualizadoEm.Kind);
    }

    [Fact]
    public void FornecedorDocument_ShouldRoundTripAuditFieldsAndCertificationValidity()
    {
        var original = CreateFornecedor();

        var bson = original.ToBsonDocument();
        var result = BsonSerializer.Deserialize<FornecedorDocument>(bson);

        Assert.Equal("APROVADO", bson["statusAuditoria"].AsString);
        Assert.False(bson.Contains("auditoriaEsg"));
        Assert.Equal("BAIXO", result.NivelRiscoEsg);
        Assert.Equal(UtcDate.AddYears(2), result.Certificacoes[0].ValidaAte);
        Assert.Equal(UtcDate, result.ConformidadeAmbiental!.DataUltimaAuditoria);
        Assert.Equal(85m, result.ConformidadeAmbiental.PercentualMaterialReciclado);
        Assert.True(result.ConformidadeAmbiental.CamposAdicionais!["controlaEfluentes"].AsBoolean);
    }

    [Fact]
    public void FatorEmissaoDocument_ShouldRoundTripDecimalAndNullableValidityEnd()
    {
        var original = CreateFatorEmissao();

        var bson = original.ToBsonDocument();
        var result = BsonSerializer.Deserialize<FatorEmissaoDocument>(bson);

        Assert.Equal("TRANSPORTE", bson["categoria"].AsString);
        Assert.IsType<BsonDecimal128>(bson["valor"]);
        Assert.True(bson.Contains("validoDe"));
        Assert.False(bson.Contains("validadeInicio"));
        Assert.Equal(0.184m, result.Valor);
        Assert.Equal(UtcDate.AddYears(1), result.ValidoAte);
        Assert.Equal(DateTimeKind.Utc, result.ValidoDe.Kind);
    }

    [Fact]
    public void EmissaoCarbonoDocument_ShouldRoundTripReferencesSnapshotsAndFlexibleActivity()
    {
        var original = CreateEmissao();

        var bson = original.ToBsonDocument();
        var result = BsonSerializer.Deserialize<EmissaoCarbonoDocument>(bson);

        Assert.IsType<BsonObjectId>(bson["empresaId"]);
        Assert.IsType<BsonDecimal128>(bson["quantidadeEmitidaKgCO2e"]);
        Assert.Equal(original.ProdutoId, result.ProdutoId);
        Assert.Equal("TRANSPORTE", result.Etapa.Categoria);
        Assert.Equal(0.184m, result.FatorAplicado.Valor);
        Assert.Equal("TRANSPORTE", result.DadosAtividade["tipo"].AsString);
        Assert.Equal(420m, Decimal128.ToDecimal(result.DadosAtividade["distanciaKm"].AsDecimal128));
        Assert.Equal(DateTimeKind.Utc, result.DataEmissao.Kind);
    }

    [Fact]
    public void EmissaoCarbonoDocument_ShouldOmitNullOptionalFields()
    {
        var bson = new EmissaoCarbonoDocument().ToBsonDocument();
        var etapa = new EtapaSnapshot
        {
            Nome = "Energia",
            Ordem = 1,
            Categoria = "ENERGIA"
        }.ToBsonDocument();
        var fator = new FatorEmissaoSnapshot
        {
            Codigo = "FE-ENERGIA-001",
            Nome = "Energia",
            Valor = 0.0817m,
            UnidadeBase = "kWh",
            Escopo = "ESCOPO_2",
            Versao = 1
        }.ToBsonDocument();

        Assert.False(etapa.Contains("local"));
        Assert.False(fator.Contains("fonteReferencia"));
        Assert.False(fator.Contains("metodologia"));
        Assert.False(bson.Contains("legacyId"));
        Assert.False(bson.Contains("observacao"));
        Assert.False(bson.Contains("revisadoEm"));
        Assert.False(bson.Contains("revisadoPor"));
    }

    [Fact]
    public void MongoDbDocumentMapper_ShouldReturnDtosWithoutMongoDriverTypes()
    {
        var empresa = CreateEmpresa().ToResponse();
        var produto = CreateProduto().ToResponse();
        var fornecedor = CreateFornecedor().ToResponse();
        var fator = CreateFatorEmissao().ToResponse();
        var emissao = CreateEmissao().ToResponse();

        Assert.Equal("EMP-001", empresa.Codigo);
        Assert.Equal("ALUMINIO", produto.AtributosAmbientais!.Materiais[0].Nome);
        Assert.Equal(45m, produto.AtributosAmbientais.CamposAdicionais["percentualFrotaEletrificada"]);
        Assert.Equal("APROVADO", fornecedor.StatusAuditoria);
        Assert.Equal("ESCOPO_3", fator.Escopo);
        Assert.Equal("TRANSPORTE", emissao.DadosAtividade["tipo"]);
        Assert.Equal(420m, emissao.DadosAtividade["distanciaKm"]);
        Assert.Equal(CreateEmissao().Id.ToString(), emissao.Id);
    }

    private static EmpresaDocument CreateEmpresa() => new()
    {
        Id = ObjectId.Parse("66d000000000000000000001"), LegacyId = 1, Codigo = "EMP-001",
        RazaoSocial = "EcoFoods Brasil S.A.", NomeFantasia = "EcoFoods", Cnpj = "10000000000101",
        Setor = "ALIMENTOS", Ativa = true,
        MetasReducao = [new MetaReducao { Tipo = "EMISSOES_GEE", AnoBase = 2025, AnoMeta = 2030, PercentualReducao = 30m }],
        Governanca = new GovernancaEsg { ResponsavelEsg = "Mariana Silva", ComiteEsg = true, FrequenciaAuditoria = "ANUAL" },
        CriadoEm = UtcDate, AtualizadoEm = UtcDate, SchemaVersion = 1
    };

    private static ProdutoDocument CreateProduto() => new()
    {
        Id = ObjectId.Parse("66d000000000000000000002"), LegacyId = 1,
        EmpresaId = ObjectId.Parse("66d000000000000000000001"), Codigo = "PROD-001", Nome = "Produto circular",
        Categoria = "EMBALAGEM", UnidadeFuncional = "UNIDADE", Ativo = true,
        AtributosAmbientais = new AtributosAmbientais
        {
            PercentualReciclavel = 95m,
            Materiais = [new MaterialProduto { Nome = "ALUMINIO", PercentualComposicao = 85m, OrigemReciclada = true }],
            CamposAdicionais = new BsonDocument("percentualFrotaEletrificada", Decimal("45"))
        },
        CriadoEm = UtcDate, AtualizadoEm = UtcDate, SchemaVersion = 1
    };

    private static FornecedorDocument CreateFornecedor() => new()
    {
        Id = ObjectId.Parse("66d000000000000000000003"), LegacyId = 1, Codigo = "FOR-001",
        RazaoSocial = "Transporte Limpo Ltda.", Cnpj = "22345678000180", Ativo = true,
        CategoriasAtuacao = ["TRANSPORTE", "LOGISTICA"],
        Certificacoes = [new Certificacao { Nome = "ISO 14001", Emissor = "Organismo Certificador", ValidaAte = UtcDate.AddYears(2) }],
        ConformidadeAmbiental = new ConformidadeAmbiental
        {
            PossuiLicenca = true, OcorrenciasUltimos12Meses = 0, DataUltimaAuditoria = UtcDate,
            PercentualMaterialReciclado = 85m, CamposAdicionais = new BsonDocument("controlaEfluentes", true)
        },
        StatusAuditoria = "APROVADO", NivelRiscoEsg = "BAIXO",
        CriadoEm = UtcDate, AtualizadoEm = UtcDate, SchemaVersion = 1
    };

    private static FatorEmissaoDocument CreateFatorEmissao() => new()
    {
        Id = ObjectId.Parse("66d000000000000000000004"), LegacyId = 1, Codigo = "FE-TRANS-DIESEL",
        Nome = "Transporte rodoviario a diesel", Categoria = "TRANSPORTE", Valor = 0.184m,
        UnidadeBase = "TON_KM", Escopo = "ESCOPO_3", Versao = 2, FonteReferencia = "Base tecnica",
        Metodologia = "Atividade multiplicada pelo fator", ValidoDe = UtcDate, ValidoAte = UtcDate.AddYears(1),
        Ativo = true, CriadoEm = UtcDate, AtualizadoEm = UtcDate, SchemaVersion = 1
    };

    private static EmissaoCarbonoDocument CreateEmissao() => new()
    {
        Id = ObjectId.Parse("66d000000000000000000005"), LegacyId = 1, Codigo = "EMI-001",
        EmpresaId = ObjectId.Parse("66d000000000000000000001"), ProdutoId = ObjectId.Parse("66d000000000000000000002"),
        FornecedorId = ObjectId.Parse("66d000000000000000000003"), FatorEmissaoId = ObjectId.Parse("66d000000000000000000004"),
        Lote = new LoteSnapshot { Codigo = "LOTE-001", QuantidadeProduzida = 1500m, Unidade = "UNIDADES", DataProducao = UtcDate },
        Etapa = new EtapaSnapshot { Nome = "Transporte rodoviario", Ordem = 3, Categoria = "TRANSPORTE", Local = "Sao Paulo" },
        QuantidadeAtividade = 504m,
        DadosAtividade = new BsonDocument { ["tipo"] = "TRANSPORTE", ["distanciaKm"] = Decimal("420"), ["cargaToneladas"] = Decimal("1.2") },
        FatorAplicado = new FatorEmissaoSnapshot { Codigo = "FE-TRANS-DIESEL", Nome = "Diesel", Valor = 0.184m, UnidadeBase = "TON_KM", Escopo = "ESCOPO_3", Versao = 2 },
        QuantidadeEmitidaKgCO2e = 92.736m, MetodoCalculo = "QuantidadeAtividade * ValorFatorCo2e",
        FonteEmissao = "Diesel", CalculadoPor = "admin@carbono.com", DataEmissao = UtcDate,
        CriadoEm = UtcDate, AtualizadoEm = UtcDate, SchemaVersion = 1
    };

    private static BsonDecimal128 Decimal(string value) => new(Decimal128.Parse(value));
}
