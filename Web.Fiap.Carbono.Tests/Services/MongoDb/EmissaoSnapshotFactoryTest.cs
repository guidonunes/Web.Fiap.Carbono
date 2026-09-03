using MongoDB.Bson;
using Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Services.MongoDb;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;

namespace Web.Fiap.Carbono.Tests.Services.MongoDb;

public sealed class EmissaoSnapshotFactoryTest
{
    [Fact]
    public void CreateFactorSnapshot_CopiesCompleteCalculationContext()
    {
        var factor = CreateFactor();

        var snapshot =
            EmissaoSnapshotFactory.CreateFactorSnapshot(factor);

        Assert.Equal(factor.Codigo, snapshot.Codigo);
        Assert.Equal(factor.Nome, snapshot.Nome);
        Assert.Equal(factor.Valor, snapshot.Valor);
        Assert.Equal(factor.UnidadeBase, snapshot.UnidadeBase);
        Assert.Equal(factor.Escopo, snapshot.Escopo);
        Assert.Equal(factor.Versao, snapshot.Versao);
        Assert.Equal(
            factor.FonteReferencia,
            snapshot.FonteReferencia);
        Assert.Equal(
            factor.Metodologia,
            snapshot.Metodologia);
    }

    [Fact]
    public void CreateFactorSnapshot_CreatesIndependentHistoricalValue()
    {
        var original = CreateFactor();

        var snapshot =
            EmissaoSnapshotFactory.CreateFactorSnapshot(original);

        var revisedFactor = new FatorEmissaoDocument
        {
            Id = original.Id,
            Codigo = original.Codigo,
            Nome = "Fator revisado",
            Categoria = original.Categoria,
            Valor = 0.2500m,
            UnidadeBase = original.UnidadeBase,
            Escopo = original.Escopo,
            Versao = 2,
            FonteReferencia = "Nova referência",
            Metodologia = "Nova metodologia",
            ValidoDe = original.ValidoDe,
            Ativo = true
        };

        Assert.NotSame(original, snapshot);
        Assert.Equal("Fator de energia", snapshot.Nome);
        Assert.Equal(0.0817m, snapshot.Valor);
        Assert.Equal(1, snapshot.Versao);
        Assert.NotEqual(revisedFactor.Valor, snapshot.Valor);
        Assert.NotEqual(revisedFactor.Versao, snapshot.Versao);
    }

    [Fact]
    public void CreateFactorSnapshot_WithNullFactor_ThrowsArgumentNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => EmissaoSnapshotFactory.CreateFactorSnapshot(null!));
    }

    [Fact]
    public void CreateLoteSnapshot_NormalizesValuesAndStoresUtcDate()
    {
        var request = new LoteEmissaoRequest
        {
            Codigo = "  LOTE-001  ",
            QuantidadeProduzida = 1500.25m,
            Unidade = "  unidades  ",
            DataProducao = new DateTime(
                2026,
                8,
                10,
                12,
                30,
                0,
                DateTimeKind.Unspecified)
        };

        var snapshot =
            EmissaoSnapshotFactory.CreateLoteSnapshot(request);

        Assert.Equal("LOTE-001", snapshot.Codigo);
        Assert.Equal(1500.25m, snapshot.QuantidadeProduzida);
        Assert.Equal("unidades", snapshot.Unidade);
        Assert.Equal(DateTimeKind.Utc, snapshot.DataProducao.Kind);
    }

    [Fact]
    public void CreateLoteSnapshot_WithMissingDate_ThrowsBadRequest()
    {
        var request = new LoteEmissaoRequest
        {
            Codigo = "LOTE-001",
            QuantidadeProduzida = 10m,
            Unidade = "unidades"
        };

        Assert.Throws<BadRequestException>(
            () => EmissaoSnapshotFactory.CreateLoteSnapshot(request));
    }

    [Fact]
    public void CreateEtapaSnapshot_NormalizesCategoryAndOptionalLocal()
    {
        var request = new EtapaEmissaoRequest
        {
            Nome = "  Transporte  ",
            Ordem = 2,
            Categoria = "transporte",
            Local = "  São Paulo  "
        };

        var snapshot =
            EmissaoSnapshotFactory.CreateEtapaSnapshot(request);

        Assert.Equal("Transporte", snapshot.Nome);
        Assert.Equal(2, snapshot.Ordem);
        Assert.Equal("TRANSPORTE", snapshot.Categoria);
        Assert.Equal("São Paulo", snapshot.Local);
    }

    [Fact]
    public void CreateActivityData_MapsTransportAsDecimal128()
    {
        var document = EmissaoSnapshotFactory.CreateActivityData(
            new DadosTransporteRequest
            {
                DistanciaKm = 350m,
                CargaToneladas = 1.2m,
                Combustivel = "DIESEL",
                Modal = "  RODOVIARIO  "
            });

        Assert.Equal("TRANSPORTE", document["tipo"].AsString);
        AssertDecimal(350m, document["distanciaKm"]);
        AssertDecimal(1.2m, document["cargaToneladas"]);
        Assert.Equal("DIESEL", document["combustivel"].AsString);
        Assert.Equal("RODOVIARIO", document["modal"].AsString);
    }

    [Fact]
    public void CreateActivityData_MapsEnergyAsDecimal128()
    {
        var document = EmissaoSnapshotFactory.CreateActivityData(
            new DadosEnergiaRequest
            {
                ConsumoKwh = 950m,
                FonteEnergia = "SOLAR",
                PercentualRenovavel = 100m
            });

        Assert.Equal("ENERGIA", document["tipo"].AsString);
        AssertDecimal(950m, document["consumoKwh"]);
        AssertDecimal(100m, document["percentualRenovavel"]);
        Assert.Equal("SOLAR", document["fonteEnergia"].AsString);
    }

    [Fact]
    public void CreateActivityData_MapsRawMaterialAndOmitsEmptyOrigin()
    {
        var document = EmissaoSnapshotFactory.CreateActivityData(
            new DadosMateriaPrimaRequest
            {
                Material = "ALUMINIO",
                PesoKg = 80m,
                PercentualReciclado = 45m,
                Origem = " "
            });

        Assert.Equal("MATERIA_PRIMA", document["tipo"].AsString);
        AssertDecimal(80m, document["pesoKg"]);
        AssertDecimal(45m, document["percentualReciclado"]);
        Assert.False(document.Contains("origem"));
    }

    [Fact]
    public void CreateActivityData_MapsWasteAndOptionalValues()
    {
        var document = EmissaoSnapshotFactory.CreateActivityData(
            new DadosResiduoRequest
            {
                Classe = "CLASSE_II",
                PesoKg = 120m,
                TipoResiduo = "ORGANICO",
                Tratamento = "COMPOSTAGEM",
                DistanciaDestinoKm = 18m,
                PercentualReciclavel = 75m
            });

        Assert.Equal("RESIDUO", document["tipo"].AsString);
        AssertDecimal(120m, document["pesoKg"]);
        AssertDecimal(18m, document["distanciaDestinoKm"]);
        AssertDecimal(75m, document["percentualReciclavel"]);
        Assert.Equal("ORGANICO", document["tipoResiduo"].AsString);
        Assert.Equal("COMPOSTAGEM", document["tratamento"].AsString);
    }

    private static FatorEmissaoDocument CreateFactor()
    {
        return new FatorEmissaoDocument
        {
            Id = ObjectId.GenerateNewId(),
            Codigo = "FE-ENERGIA-001",
            Nome = "Fator de energia",
            Categoria = "ENERGIA",
            Valor = 0.0817m,
            UnidadeBase = "kWh",
            Escopo = "ESCOPO_2",
            Versao = 1,
            FonteReferencia = "Fonte técnica",
            Metodologia = "Consumo multiplicado pelo fator",
            ValidoDe = new DateTime(
                2026,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc),
            Ativo = true
        };
    }

    private static void AssertDecimal(
        decimal expected,
        BsonValue actual)
    {
        Assert.IsType<BsonDecimal128>(actual);
        Assert.Equal(
            expected,
            Decimal128.ToDecimal(actual.AsDecimal128));
    }
}
