using Web.Fiap.Carbono.Dtos.MongoDb.Emissoes;
using Web.Fiap.Carbono.Models.Documents;
using Web.Fiap.Carbono.Services.MongoDb;
using Web.Fiap.Carbono.Services.MongoDb.Exceptions;

namespace Web.Fiap.Carbono.Tests.Services.MongoDb;

public sealed class EmissaoCalculationRulesTest
{
    [Fact]
    public void ValidateActivity_WithCompatibleUnitAndCategory_DoesNotThrow()
    {
        var request = CreateRequest();
        var factor = CreateFactor();

        var exception = Record.Exception(
            () => EmissaoCalculationRules.ValidateActivity(request, factor));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateActivity_WithNonPositiveQuantity_ThrowsBadRequest()
    {
        var request = CreateRequest(quantidadeAtividade: 0m);

        var exception = Assert.Throws<BadRequestException>(
            () => EmissaoCalculationRules.ValidateActivity(
                request,
                CreateFactor()));

        Assert.Contains("maior que zero", exception.Message);
    }

    [Fact]
    public void ValidateActivity_WithIncompatibleUnit_ThrowsUnprocessableEntity()
    {
        var request = CreateRequest(unidadeAtividade: "litro");

        var exception = Assert.Throws<UnprocessableEntityException>(
            () => EmissaoCalculationRules.ValidateActivity(
                request,
                CreateFactor()));

        Assert.Contains("não é compatível", exception.Message);
    }

    [Fact]
    public void ValidateActivity_WithMismatchedCategory_ThrowsUnprocessableEntity()
    {
        var request = CreateRequest(etapaCategoria: "TRANSPORTE");

        var exception = Assert.Throws<UnprocessableEntityException>(
            () => EmissaoCalculationRules.ValidateActivity(
                request,
                CreateFactor()));

        Assert.Contains("não corresponde", exception.Message);
    }

    [Fact]
    public void ValidateFactorAvailability_WithCurrentActiveFactor_DoesNotThrow()
    {
        var timestamp = Utc(2026, 9, 2);
        var factor = CreateFactor(
            active: true,
            validFrom: timestamp.AddDays(-1),
            validUntil: timestamp.AddDays(1));

        var exception = Record.Exception(
            () => EmissaoCalculationRules.ValidateFactorAvailability(
                factor,
                timestamp));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateFactorAvailability_WithInactiveFactor_ThrowsUnprocessableEntity()
    {
        var timestamp = Utc(2026, 9, 2);
        var factor = CreateFactor(
            active: false,
            validFrom: timestamp.AddDays(-1),
            validUntil: timestamp.AddDays(1));

        var exception = Assert.Throws<UnprocessableEntityException>(
            () => EmissaoCalculationRules.ValidateFactorAvailability(
                factor,
                timestamp));

        Assert.Contains("inativo", exception.Message);
    }

    [Fact]
    public void ValidateFactorAvailability_BeforeValidFrom_ThrowsUnprocessableEntity()
    {
        var timestamp = Utc(2026, 9, 2);
        var factor = CreateFactor(
            validFrom: timestamp.AddTicks(1));

        var exception = Assert.Throws<UnprocessableEntityException>(
            () => EmissaoCalculationRules.ValidateFactorAvailability(
                factor,
                timestamp));

        Assert.Contains("válido a partir", exception.Message);
    }

    [Fact]
    public void ValidateFactorAvailability_AfterValidUntil_ThrowsUnprocessableEntity()
    {
        var timestamp = Utc(2026, 9, 2);
        var factor = CreateFactor(
            validFrom: timestamp.AddDays(-1),
            validUntil: timestamp.AddTicks(-1));

        var exception = Assert.Throws<UnprocessableEntityException>(
            () => EmissaoCalculationRules.ValidateFactorAvailability(
                factor,
                timestamp));

        Assert.Contains("expirou", exception.Message);
    }

    [Fact]
    public void ValidateFactorAvailability_AtValidityBoundaries_DoesNotThrow()
    {
        var timestamp = Utc(2026, 9, 2);
        var factor = CreateFactor(
            validFrom: timestamp,
            validUntil: timestamp);

        var exception = Record.Exception(
            () => EmissaoCalculationRules.ValidateFactorAvailability(
                factor,
                timestamp));

        Assert.Null(exception);
    }

    [Fact]
    public void ValidateFactorAvailability_WithoutValidUntil_DoesNotExpire()
    {
        var timestamp = Utc(2026, 9, 2);
        var factor = CreateFactor(
            validFrom: timestamp.AddYears(-10),
            validUntil: null);

        var exception = Record.Exception(
            () => EmissaoCalculationRules.ValidateFactorAvailability(
                factor,
                timestamp));

        Assert.Null(exception);
    }

    [Fact]
    public void CalculateKgCO2e_ReturnsExactDecimalResult()
    {
        var result = EmissaoCalculationRules.CalculateKgCO2e(
            1500m,
            0.0817m);

        Assert.Equal(122.5500m, result);
    }

    [Fact]
    public void CalculateKgCO2e_PreservesDecimalArithmetic()
    {
        var result = EmissaoCalculationRules.CalculateKgCO2e(
            0.1m,
            0.2m);

        Assert.Equal(0.02m, result);
    }

    [Fact]
    public void CalculateKgCO2e_WithZeroFactor_ReturnsZero()
    {
        var result = EmissaoCalculationRules.CalculateKgCO2e(
            1500m,
            0m);

        Assert.Equal(0m, result);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void CalculateKgCO2e_WithNonPositiveQuantity_ThrowsBadRequest(
        string quantityText)
    {
        var quantity = decimal.Parse(
            quantityText,
            System.Globalization.CultureInfo.InvariantCulture);

        Assert.Throws<BadRequestException>(
            () => EmissaoCalculationRules.CalculateKgCO2e(
                quantity,
                0.0817m));
    }

    [Fact]
    public void CalculateKgCO2e_WithNegativeFactor_ThrowsUnprocessableEntity()
    {
        Assert.Throws<UnprocessableEntityException>(
            () => EmissaoCalculationRules.CalculateKgCO2e(
                1500m,
                -0.0817m));
    }

    [Fact]
    public void CalculateKgCO2e_WhenResultOverflows_ThrowsUnprocessableEntity()
    {
        var exception = Assert.Throws<UnprocessableEntityException>(
            () => EmissaoCalculationRules.CalculateKgCO2e(
                decimal.MaxValue,
                2m));

        Assert.Contains("excede", exception.Message);
    }

    [Fact]
    public void ValidateCalculatedBy_WithAuthenticatedEmail_ReturnsNormalizedValue()
    {
        var result = EmissaoCalculationRules.ValidateCalculatedBy(
            "  analista@carbono.com  ");

        Assert.Equal("analista@carbono.com", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateCalculatedBy_WithMissingIdentity_ThrowsBadRequest(
        string? calculatedBy)
    {
        var exception = Assert.Throws<BadRequestException>(
            () => EmissaoCalculationRules.ValidateCalculatedBy(
                calculatedBy));

        Assert.Contains("calculadoPor", exception.Message);
    }

    [Fact]
    public void ValidateCalculatedBy_WithOversizedIdentity_ThrowsBadRequest()
    {
        var calculatedBy = new string('a', 201);

        var exception = Assert.Throws<BadRequestException>(
            () => EmissaoCalculationRules.ValidateCalculatedBy(
                calculatedBy));

        Assert.Contains("200", exception.Message);
    }

    private static CalcularEmissaoMongoRequest CreateRequest(
        decimal quantidadeAtividade = 1500m,
        string unidadeAtividade = "kWh",
        string etapaCategoria = "ENERGIA")
    {
        return new CalcularEmissaoMongoRequest
        {
            ProdutoId = "68c000000000000000000001",
            FornecedorId = "68c000000000000000000002",
            FatorEmissaoId = "68c000000000000000000003",
            QuantidadeAtividade = quantidadeAtividade,
            UnidadeAtividade = unidadeAtividade,
            Lote = new LoteEmissaoRequest
            {
                Codigo = "LOTE-001"
            },
            Etapa = new EtapaEmissaoRequest
            {
                Nome = "Consumo de energia",
                Ordem = 1,
                Categoria = etapaCategoria
            },
            DadosAtividade = new DadosEnergiaRequest
            {
                ConsumoKwh = quantidadeAtividade,
                FonteEnergia = "Rede elétrica",
                PercentualRenovavel = 20m
            }
        };
    }

    private static FatorEmissaoDocument CreateFactor(
        bool active = true,
        DateTime? validFrom = null,
        DateTime? validUntil = null)
    {
        return new FatorEmissaoDocument
        {
            UnidadeBase = "kWh",
            Ativo = active,
            ValidoDe = validFrom ?? Utc(2026, 1, 1),
            ValidoAte = validUntil
        };
    }

    private static DateTime Utc(int year, int month, int day)
    {
        return new DateTime(
            year,
            month,
            day,
            0,
            0,
            0,
            DateTimeKind.Utc);
    }
}
