using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MongoDB.Driver;

namespace Web.Fiap.Carbono.Tests.Migration;

public sealed class MigrationMongoFixture : IAsyncLifetime
{
    private readonly IContainer _container = new ContainerBuilder("mongo:8.0.29-noble")
        .WithPortBinding(27017, true)
        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Waiting for connections"))
        .WithCleanUp(false).WithAutoRemove(true).Build();

    public Task InitializeAsync() => _container.StartAsync();
    public Task DisposeAsync() => _container.DisposeAsync().AsTask();

    public async Task<IMongoDatabase> CreateDatabaseAsync()
    {
        var name = $"migration_test_{Guid.NewGuid():N}";
        var root = new DirectoryInfo(AppContext.BaseDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "Web.Fiap.Carbono.sln"))) root = root.Parent;
        Assert.NotNull(root);
        foreach (var script in new[] { "01-create-collections.js", "02-create-indexes.js" })
        {
            var code = await File.ReadAllTextAsync(Path.Combine(root!.FullName, "database", "mongodb", script));
            // Execute the real validators/indexes in a unique disposable test database.
            code = code.Replace("db.getSiblingDB(\"fiap_carbono\")", $"db.getSiblingDB(\"{name}\")");
            var result = await _container.ExecAsync(["mongosh", "--quiet", "--eval", code]);
            Assert.True(result.ExitCode == 0, result.Stderr);
        }
        var settings = MongoClientSettings.FromConnectionString(
            $"mongodb://{_container.Hostname}:{_container.GetMappedPublicPort(27017)}");
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
        return new MongoClient(settings).GetDatabase(name);
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MigrationCollection : ICollectionFixture<MigrationMongoFixture>
{
    public const string Name = "Phase 12 migration integration";
}
