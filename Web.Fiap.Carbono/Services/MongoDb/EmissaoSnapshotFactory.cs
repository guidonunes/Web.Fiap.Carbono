using MongoDB.Bson;
using Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Models.Documents.Embedded;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;

namespace Web.Fiap.Carbono.Services.MongoDb;

internal static class EmissaoSnapshotFactory
{
    public static FatorEmissaoSnapshot CreateFactorSnapshot(
        FatorEmissaoDocument factor)
    {
        ArgumentNullException.ThrowIfNull(factor);

        return new FatorEmissaoSnapshot
        {
            Codigo = factor.Codigo,
            Nome = factor.Nome,
            Valor = factor.Valor,
            UnidadeBase = factor.UnidadeBase,
            Escopo = factor.Escopo,
            Versao = factor.Versao,
            FonteReferencia = factor.FonteReferencia,
            Metodologia = factor.Metodologia
        };
    }

    public static LoteSnapshot CreateLoteSnapshot(
        LoteEmissaoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.QuantidadeProduzida <= 0)
        {
            throw new BadRequestException(
                "A quantidade produzida do lote deve ser maior que zero.");
        }

        if (request.DataProducao == default)
        {
            throw new BadRequestException(
                "A data de produção do lote é obrigatória.");
        }

        return new LoteSnapshot
        {
            Codigo = MongoServiceRules.Required(
                request.Codigo,
                "lote.codigo",
                100),
            QuantidadeProduzida = request.QuantidadeProduzida,
            Unidade = MongoServiceRules.Required(
                request.Unidade,
                "lote.unidade",
                50),
            DataProducao = MongoServiceRules.AsUtc(
                request.DataProducao)
        };
    }

    public static EtapaSnapshot CreateEtapaSnapshot(
        EtapaEmissaoRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Ordem < 1)
        {
            throw new BadRequestException(
                "A ordem da etapa deve ser maior ou igual a 1.");
        }

        return new EtapaSnapshot
        {
            Nome = MongoServiceRules.Required(
                request.Nome,
                "etapa.nome",
                150),
            Ordem = request.Ordem,
            Categoria = MongoServiceRules.Required(
                    request.Categoria,
                    "etapa.categoria",
                    50)
                .ToUpperInvariant(),
            Local = MongoServiceRules.Optional(
                request.Local,
                "etapa.local",
                200)
        };
    }

    public static BsonDocument CreateActivityData(
        DadosAtividadeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request switch
        {
            DadosTransporteRequest transporte =>
                CreateTransportActivity(transporte),

            DadosEnergiaRequest energia =>
                CreateEnergyActivity(energia),

            DadosMateriaPrimaRequest materiaPrima =>
                CreateRawMaterialActivity(materiaPrima),

            DadosResiduoRequest residuo =>
                CreateWasteActivity(residuo),

            _ => throw new BadRequestException(
                "O tipo de dados da atividade não é suportado.")
        };
    }

    private static BsonDocument CreateTransportActivity(
        DadosTransporteRequest request)
    {
        var document = new BsonDocument
        {
            ["tipo"] = request.Categoria,
            ["distanciaKm"] = Decimal(request.DistanciaKm),
            ["cargaToneladas"] = Decimal(request.CargaToneladas),
            ["combustivel"] = MongoServiceRules.Required(
                request.Combustivel,
                "dadosAtividade.combustivel",
                100)
        };

        AddOptionalString(document, "modal", request.Modal);

        return document;
    }

    private static BsonDocument CreateEnergyActivity(
        DadosEnergiaRequest request)
    {
        return new BsonDocument
        {
            ["tipo"] = request.Categoria,
            ["consumoKwh"] = Decimal(request.ConsumoKwh),
            ["fonteEnergia"] = MongoServiceRules.Required(
                request.FonteEnergia,
                "dadosAtividade.fonteEnergia",
                100),
            ["percentualRenovavel"] =
                Decimal(request.PercentualRenovavel)
        };
    }

    private static BsonDocument CreateRawMaterialActivity(
        DadosMateriaPrimaRequest request)
    {
        var document = new BsonDocument
        {
            ["tipo"] = request.Categoria,
            ["material"] = MongoServiceRules.Required(
                request.Material,
                "dadosAtividade.material",
                150),
            ["pesoKg"] = Decimal(request.PesoKg),
            ["percentualReciclado"] =
                Decimal(request.PercentualReciclado)
        };

        AddOptionalString(document, "origem", request.Origem);

        return document;
    }

    private static BsonDocument CreateWasteActivity(
        DadosResiduoRequest request)
    {
        var document = new BsonDocument
        {
            ["tipo"] = request.Categoria,
            ["classe"] = MongoServiceRules.Required(
                request.Classe,
                "dadosAtividade.classe",
                100),
            ["pesoKg"] = Decimal(request.PesoKg),
            ["tratamento"] = MongoServiceRules.Required(
                request.Tratamento,
                "dadosAtividade.tratamento",
                150),
            ["distanciaDestinoKm"] =
                Decimal(request.DistanciaDestinoKm)
        };

        AddOptionalString(
            document,
            "tipoResiduo",
            request.TipoResiduo);

        if (request.PercentualReciclavel.HasValue)
        {
            document["percentualReciclavel"] =
                Decimal(request.PercentualReciclavel.Value);
        }

        return document;
    }

    private static BsonDecimal128 Decimal(decimal value)
    {
        return new BsonDecimal128(new Decimal128(value));
    }

    private static void AddOptionalString(
        BsonDocument document,
        string fieldName,
        string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            document[fieldName] = value.Trim();
        }
    }
}
