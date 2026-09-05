using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Web.Fiap.Carbono.Migration;
using Web.Fiap.Carbono.Models;
using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Tests.Migration;

public sealed class MigrationPlannerTest
{
    [Fact]
    public void MapsReferencesDecimalsAndHistoricalContextWithoutInventingActivity()
    {
        var plan = MigrationPlanner.Build(MigrationTestData.Source(), MigrationTestData.Policy);
        Assert.Equal(5, plan.Count);
        var emission = Assert.Single(plan["emissoes_carbono"]);
        Assert.Equal(plan["empresas"][0]["_id"], emission["empresaId"]);
        Assert.Equal(plan["produtos"][0]["_id"], emission["produtoId"]);
        Assert.Equal(plan["fornecedores"][0]["_id"], emission["fornecedorId"]);
        Assert.Equal(plan["fatores_emissao"][0]["_id"], emission["fatorEmissaoId"]);
        Assert.Equal(BsonType.Decimal128, emission["quantidadeEmitidaKgCO2e"].BsonType);
        Assert.Equal(122.55m, Decimal128.ToDecimal(emission["quantidadeEmitidaKgCO2e"].AsDecimal128));
        Assert.Equal("TRANSPORTE", emission["etapa"]["categoria"]);
        Assert.False(emission["dadosAtividade"].AsBsonDocument.Contains("distanciaKm"));
        Assert.False(emission["etapa"].AsBsonDocument.Contains("local"));
        Assert.True(emission["migracao"]["snapshotReconstruido"].AsBoolean);
        plan["fatores_emissao"][0]["valor"] = new BsonDecimal128(9m);
        Assert.Equal(0.0817m, Decimal128.ToDecimal(emission["fatorAplicado"]["valor"].AsDecimal128));
        Assert.Equal(41, BsonSerializer.Deserialize<EmissaoCarbonoDocument>(emission).LegacyId);
        Assert.Equal(1, BsonSerializer.Deserialize<EmpresaDocument>(plan["empresas"][0]).LegacyId);
        Assert.Equal(1, BsonSerializer.Deserialize<ProdutoDocument>(plan["produtos"][0]).LegacyId);
        Assert.Equal(1, BsonSerializer.Deserialize<FornecedorDocument>(plan["fornecedores"][0]).LegacyId);
        Assert.Equal(2, BsonSerializer.Deserialize<FatorEmissaoDocument>(plan["fatores_emissao"][0]).LegacyId);
    }

    [Theory]
    [InlineData("company")]
    [InlineData("product")]
    [InlineData("supplier")]
    [InlineData("factor")]
    [InlineData("batch")]
    [InlineData("stage")]
    public void ReportsMissingReferences(string missing)
    {
        var source = MigrationTestData.Source();
        switch (missing)
        {
            case "company": source.Empresas.Clear(); break;
            case "product": source.Produtos.Clear(); break;
            case "supplier": source.Fornecedores.Clear(); break;
            case "factor": source.Fatores.Clear(); break;
            case "batch": source.Lotes.Clear(); break;
            case "stage": source.Etapas.Clear(); break;
        }
        var error = Assert.Throws<MigrationValidationException>(() => MigrationPlanner.Build(source, MigrationTestData.Policy));
        Assert.Contains(error.Errors, x => x.Contains($"Missing {missing}", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void DetectsOrphanBatchWithoutEmissions()
    {
        var source = MigrationTestData.Source();
        source.Lotes.Add(new LoteProducaoModel { IdLote = 9, IdProduto = 999 });
        var error = Assert.Throws<MigrationValidationException>(() => MigrationPlanner.Build(source, MigrationTestData.Policy));
        Assert.Contains(error.Errors, x => x.Contains("legacyId=9") && x.Contains("missing product 999"));
    }

    [Fact]
    public void RequiresPolicyAndValidOrderAndReportsAllRowErrors()
    {
        var source = MigrationTestData.Source();
        source.Etapas[0].OrdemEtapa = "unknown";
        var error = Assert.Throws<MigrationValidationException>(() => MigrationPlanner.Build(source,
            MigrationTestData.Policy with { CompanyActiveDefault = null, SupplierCreatedAtUtc = null }));
        Assert.Contains(error.Errors, x => x.Contains("CompanyActiveDefault"));
        Assert.Contains(error.Errors, x => x.Contains("SupplierCreatedAtUtc"));
        Assert.Contains(error.Errors, x => x.Contains("legacyId=41") && x.Contains("Stage order"));
        var factorError = Assert.Throws<MigrationValidationException>(() => MigrationPlanner.Build(source,
            MigrationTestData.Policy with { Factors = [] }));
        Assert.Contains(factorError.Errors, x => x.Contains("legacyId=2") && x.Contains("Missing factor policy"));
    }

    [Fact]
    public void PreservesRoundedOracleResultAndRejectsUnexplainedDifferences()
    {
        var source = MigrationTestData.Source();
        source.Emissoes[0].QuantidadeAtividade = 1m;
        source.Emissoes[0].QuantidadeEmitida = 0.082m;
        var plan = MigrationPlanner.Build(source, MigrationTestData.Policy);
        Assert.Equal(0.082m, BsonSerializer.Deserialize<EmissaoCarbonoDocument>(plan["emissoes_carbono"][0]).QuantidadeEmitidaKgCO2e);
        source.Emissoes[0].QuantidadeEmitida = 1m;
        Assert.Throws<MigrationValidationException>(() => MigrationPlanner.Build(source, MigrationTestData.Policy));
    }

    [Fact]
    public void HandlesDuplicatesAndNormalizedBusinessKeys()
    {
        var source = MigrationTestData.Source();
        source.Empresas.Add(new EmpresaModel { IdEmpresa = 2, NomeEmpresa = "Duplicada", Cnpj = "12345678000190", DataCadastro = MigrationTestData.Date });
        var error = Assert.Throws<MigrationValidationException>(() => MigrationPlanner.Build(source, MigrationTestData.Policy));
        Assert.Contains(error.Errors, x => x.Contains("duplicate normalized CNPJ"));
        source.Empresas[1].IdEmpresa = 1;
        Assert.Throws<MigrationValidationException>(() => MigrationPlanner.Build(source, MigrationTestData.Policy));
    }

    [Fact]
    public void StableMappingAndUtcConversionDoNotDependOnMachineTimezone()
    {
        var source = MigrationTestData.Source();
        var policy = MigrationTestData.Policy with { OracleTimeZone = "America/Sao_Paulo" };
        var first = MigrationPlanner.Build(source, policy);
        var second = MigrationPlanner.Build(source, policy);
        Assert.Equal(first["empresas"][0], second["empresas"][0]);
        Assert.Equal(MigrationTestData.Date.AddHours(3), first["empresas"][0]["criadoEm"].ToUniversalTime());
        Assert.NotEqual(MigrationPlanner.StableId("a", "empresas", 1), MigrationPlanner.StableId("b", "empresas", 1));
        Assert.Throws<MigrationValidationException>(() => MigrationPlanner.Build(source, policy with { OracleTimeZone = "" }));
    }
}
