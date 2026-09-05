using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Migration;

namespace Web.Fiap.Carbono.Tests.Migration;

[Collection(MigrationCollection.Name)]
public sealed class MigrationReconcilerTest(MigrationMongoFixture fixture)
{
    [Fact]
    public async Task ReconcilesCountsTotalsDatesReferencesAndLegacyIds()
    {
        var database = await fixture.CreateDatabaseAsync();
        var plan = MigrationPlanner.Build(MigrationTestData.Source(3), MigrationTestData.Policy);
        await new MongoMigrationWriter(database).RunAsync(plan, true, true);

        var report = await new MigrationReconciler(database).RunAsync(plan);

        Assert.True(report.Passed, string.Join(Environment.NewLine,
            report.Failures.Select(x => $"{x.Requirement}: expected {x.Expected}, actual {x.Actual}")));
        Assert.Contains(report.Checks, x => x.Requirement == "total.kgCO2e" && x.Expected == "367.65");
        Assert.Contains(report.Checks, x => x.Requirement == "date.earliest" && x.Passed);
        Assert.Contains(report.Checks, x => x.Requirement == "orphan.emissao.fatorEmissaoId" && x.Passed);
        Assert.Contains(report.Checks, x => x.Requirement == "legacyId.emissoes_carbono" && x.Passed);
    }

    [Fact]
    public async Task ReportsDecimalAndGroupingDifferences()
    {
        var database = await fixture.CreateDatabaseAsync();
        var plan = MigrationPlanner.Build(MigrationTestData.Source(), MigrationTestData.Policy);
        await new MongoMigrationWriter(database).RunAsync(plan, true, true);
        await database.GetCollection<BsonDocument>("emissoes_carbono").UpdateOneAsync(
            FilterDefinition<BsonDocument>.Empty,
            Builders<BsonDocument>.Update.Set("quantidadeEmitidaKgCO2e", new Decimal128(1m)));

        var report = await new MigrationReconciler(database).RunAsync(plan);

        Assert.False(report.Passed);
        Assert.Contains(report.Failures, x => x.Requirement == "total.kgCO2e" && x.Actual == "1");
        Assert.Contains(report.Failures, x => x.Requirement == "totals.perCompany");
        Assert.Contains(report.Failures, x => x.Requirement == "totals.perProduct");
        Assert.Contains(report.Failures, x => x.Requirement == "totals.perSupplier");
    }

    [Fact]
    public async Task ReportsOrphanReferenceAndMissingTraceability()
    {
        var database = await fixture.CreateDatabaseAsync();
        var plan = MigrationPlanner.Build(MigrationTestData.Source(), MigrationTestData.Policy);
        await new MongoMigrationWriter(database).RunAsync(plan, true, true);
        await database.GetCollection<BsonDocument>("emissoes_carbono").UpdateOneAsync(
            FilterDefinition<BsonDocument>.Empty,
            Builders<BsonDocument>.Update.Set("fatorEmissaoId", ObjectId.GenerateNewId()));
        await database.GetCollection<BsonDocument>("empresas").UpdateOneAsync(
            FilterDefinition<BsonDocument>.Empty,
            Builders<BsonDocument>.Update.Unset("legacyId"));

        var report = await new MigrationReconciler(database).RunAsync(plan);

        Assert.False(report.Passed);
        Assert.Contains(report.Failures, x => x.Requirement == "orphan.emissao.fatorEmissaoId" && x.Actual == "1");
        Assert.Contains(report.Failures, x => x.Requirement == "legacyId.empresas");
        Assert.Contains(report.Failures, x => x.Requirement == "legacyId.unique.empresas");
    }
}
