using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Migration;

namespace Web.Fiap.Carbono.Tests.Migration;

[Collection(MigrationCollection.Name)]
public sealed class MongoMigrationWriterTest(MigrationMongoFixture fixture)
{
    [Fact]
    public async Task DryRunWritesNothingApplyPreservesDocumentsAndRetryIsIdempotent()
    {
        var database = await fixture.CreateDatabaseAsync();
        var plan = MigrationPlanner.Build(MigrationTestData.Source(3), MigrationTestData.Policy);
        var writer = new MongoMigrationWriter(database);
        var pending = await writer.RunAsync(plan, false, false);
        Assert.Equal(3, pending["emissoes_carbono"]);
        await AssertEmpty(database);
        await writer.RunAsync(plan, true, true, batchSize: 2);
        foreach (var (name, expected) in plan)
        {
            var actual = await database.GetCollection<BsonDocument>(name).Find(FilterDefinition<BsonDocument>.Empty).ToListAsync();
            Assert.Equal(expected.Count, actual.Count);
            foreach (var document in expected) Assert.Contains(document, actual);
        }
        var retried = await writer.RunAsync(MigrationPlanner.Build(MigrationTestData.Source(3), MigrationTestData.Policy), true, true);
        Assert.All(retried.Values, count => Assert.Equal(0, count));
    }

    [Fact]
    public async Task ValidatorFailureInLastCollectionPreventsAllWrites()
    {
        var database = await fixture.CreateDatabaseAsync();
        var plan = MigrationPlanner.Build(MigrationTestData.Source(), MigrationTestData.Policy);
        plan["emissoes_carbono"][0]["etapa"]["local"] = BsonNull.Value;
        var error = await Assert.ThrowsAsync<MigrationValidationException>(() =>
            new MongoMigrationWriter(database).RunAsync(plan, true, true));
        Assert.Contains(error.Errors, x => x.Contains("validator rejected legacy IDs 41"));
        await AssertEmpty(database);
    }

    [Fact]
    public async Task UnacknowledgedSnapshotsAndUnrelatedTargetAreRejected()
    {
        var database = await fixture.CreateDatabaseAsync();
        var plan = MigrationPlanner.Build(MigrationTestData.Source(), MigrationTestData.Policy);
        var writer = new MongoMigrationWriter(database);
        await Assert.ThrowsAsync<MigrationValidationException>(() => writer.RunAsync(plan, true, false));
        await AssertEmpty(database);
        var unrelated = plan["empresas"][0].DeepClone().AsBsonDocument;
        unrelated["_id"] = ObjectId.GenerateNewId();
        await database.GetCollection<BsonDocument>("empresas").InsertOneAsync(unrelated);
        await Assert.ThrowsAsync<MigrationValidationException>(() => writer.RunAsync(plan, true, true));
        Assert.Equal(0, await database.GetCollection<BsonDocument>("produtos").CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
    }

    [Fact]
    public async Task ConflictingExistingDocumentIsNeverOverwritten()
    {
        var database = await fixture.CreateDatabaseAsync();
        var plan = MigrationPlanner.Build(MigrationTestData.Source(), MigrationTestData.Policy);
        var altered = plan["empresas"][0].DeepClone().AsBsonDocument;
        altered["razaoSocial"] = "Changed after import";
        var companies = database.GetCollection<BsonDocument>("empresas");
        await companies.InsertOneAsync(altered);
        await Assert.ThrowsAsync<MigrationValidationException>(() => new MongoMigrationWriter(database).RunAsync(plan, true, true));
        Assert.Equal(altered, await companies.Find(FilterDefinition<BsonDocument>.Empty).SingleAsync());
    }

    [Fact]
    public async Task PartialBatchFailureReportsIdAndResumesCompletedInserts()
    {
        var database = await fixture.CreateDatabaseAsync();
        var plan = MigrationPlanner.Build(MigrationTestData.Source(3), MigrationTestData.Policy);
        var emissions = database.GetCollection<BsonDocument>("emissoes_carbono");
        // Additional test-only constraint produces a real mid-batch server failure.
        await emissions.Indexes.CreateOneAsync(new CreateIndexModel<BsonDocument>(
            new BsonDocument("metodoCalculo", 1), new CreateIndexOptions { Unique = true, Name = "test_failure" }));
        var writer = new MongoMigrationWriter(database);
        var error = await Assert.ThrowsAsync<MigrationValidationException>(() => writer.RunAsync(plan, true, true));
        Assert.Contains(error.Errors, x => x.Contains("legacyId=42") && x.Contains("11000"));
        Assert.Equal(1, await emissions.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
        await emissions.Indexes.DropOneAsync("test_failure");
        var resumed = await writer.RunAsync(plan, true, true);
        Assert.Equal(2, resumed["emissoes_carbono"]);
        Assert.Equal(0, resumed["empresas"]);
        Assert.Equal(3, await emissions.CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
    }

    private static async Task AssertEmpty(IMongoDatabase database)
    {
        foreach (var name in MigrationPlanner.Collections)
            Assert.Equal(0, await database.GetCollection<BsonDocument>(name).CountDocumentsAsync(FilterDefinition<BsonDocument>.Empty));
    }
}
