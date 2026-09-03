using Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;

namespace Web.Fiap.Carbono.Services.MongoDb;

internal static class EmissaoCalculationRules
{
    public static void ValidateActivity(
        CalcularEmissaoMongoRequest request,
        FatorEmissaoDocument factor)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(factor);

        if (request.QuantidadeAtividade <= 0)
        {
            throw new BadRequestException(
                "A quantidade da atividade deve ser maior que zero.");
        }

        var unidadeInformada = MongoServiceRules.Required(
            request.UnidadeAtividade,
            "unidadeAtividade",
            50);

        if (!string.Equals(
                unidadeInformada,
                factor.UnidadeBase,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new UnprocessableEntityException(
                $"A unidade '{unidadeInformada}' não é compatível com " +
                $"a unidade base '{factor.UnidadeBase}' do fator de emissão.");
        }

        if (!string.Equals(
                request.Etapa.Categoria,
                request.DadosAtividade.Categoria,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new UnprocessableEntityException(
                $"A categoria da etapa '{request.Etapa.Categoria}' não corresponde " +
                $"ao tipo de atividade '{request.DadosAtividade.Categoria}'.");
        }
    }

    public static void ValidateFactorAvailability(
        FatorEmissaoDocument factor,
        DateTime calculationTimestampUtc)
    {
        ArgumentNullException.ThrowIfNull(factor);

        var calculationTimestamp =
            MongoServiceRules.AsUtc(calculationTimestampUtc);

        var validFrom = MongoServiceRules.AsUtc(factor.ValidoDe);

        var validUntil = factor.ValidoAte.HasValue
            ? MongoServiceRules.AsUtc(factor.ValidoAte.Value)
            : (DateTime?)null;

        if (!factor.Ativo)
        {
            throw new UnprocessableEntityException(
                "O fator de emissão informado está inativo.");
        }

        if (calculationTimestamp < validFrom)
        {
            throw new UnprocessableEntityException(
                $"O fator de emissão somente é válido a partir de " +
                $"{validFrom:yyyy-MM-ddTHH:mm:ssZ}.");
        }

        if (validUntil.HasValue &&
            calculationTimestamp > validUntil.Value)
        {
            throw new UnprocessableEntityException(
                $"O fator de emissão expirou em " +
                $"{validUntil.Value:yyyy-MM-ddTHH:mm:ssZ}.");
        }
    }

    public static decimal CalculateKgCO2e(
        decimal activityQuantity,
        decimal factorValue)
    {
        if (activityQuantity <= 0)
        {
            throw new BadRequestException(
                "A quantidade da atividade deve ser maior que zero.");
        }

        // Zero is a valid factor value for activities with no measured
        // emissions, but a negative emission factor is invalid.
        if (factorValue < 0)
        {
            throw new UnprocessableEntityException(
                "O valor do fator de emissão não pode ser negativo.");
        }

        try
        {
            return checked(activityQuantity * factorValue);
        }
        catch (OverflowException)
        {
            throw new UnprocessableEntityException(
                "O resultado da emissão excede o intervalo decimal suportado.");
        }
    }

    public static string ValidateCalculatedBy(string? calculatedBy)
    {
        return MongoServiceRules.Required(
            calculatedBy,
            "calculadoPor",
            200);
    }
}
