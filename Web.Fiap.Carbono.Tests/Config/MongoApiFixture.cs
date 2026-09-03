using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Tests.Config;

public sealed class MongoApiFixture : IAsyncLifetime
{
    private const int MongoPort = 27019;
    private const string DatabaseName = "fiap_carbono_api_tests";

    private readonly IContainer _container = new ContainerBuilder(
        "mongo:8.0.29-noble"
    )
        .WithCreateParameterModifier(parameters =>
        {
            parameters.HostConfig!.NetworkMode = "host";
        })
        .WithCommand(
            "mongod",
            "--bind_ip",
            "127.0.0.1",
            "--port",
            MongoPort.ToString(),
            "--noauth"
        )
        .WithWaitStrategy(
            Wait.ForUnixContainer().UntilMessageIsLogged(
                "Waiting for connections"
            )
        )
        .WithCleanUp(false)
        .WithAutoRemove(true)
        .Build();

    private MongoApiApplicationFactory _factory = null!;

    public IMongoDatabase Database { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var connectionString =
            $"mongodb://127.0.0.1:{MongoPort}/?directConnection=true";
        var settings = MongoClientSettings.FromConnectionString(
            connectionString
        );
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);

        var client = new MongoClient(settings);
        await client
            .GetDatabase("admin")
            .RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));

        Database = client.GetDatabase(DatabaseName);
        await CreateCollectionsAsync();
        await CreateUniqueIndexesAsync();

        _factory = new MongoApiApplicationFactory(
            connectionString,
            DatabaseName
        );
    }

    public HttpClient CreateClient()
    {
        return _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            }
        );
    }

    public async Task ClearAsync()
    {
        await Database
            .GetCollection<BsonDocument>(
                MongoDbContext.EmissoesCarbonoCollectionName
            )
            .DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
        await Database
            .GetCollection<BsonDocument>(
                MongoDbContext.ProdutosCollectionName
            )
            .DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
        await Database
            .GetCollection<BsonDocument>(
                MongoDbContext.FornecedoresCollectionName
            )
            .DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
        await Database
            .GetCollection<BsonDocument>(
                MongoDbContext.FatoresEmissaoCollectionName
            )
            .DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
        await Database
            .GetCollection<BsonDocument>(
                MongoDbContext.EmpresasCollectionName
            )
            .DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
    }

    public async Task DisposeAsync()
    {
        _factory.Dispose();
        await _container.DisposeAsync();
    }

    private async Task CreateCollectionsAsync()
    {
        foreach (var name in new[]
                 {
                     MongoDbContext.EmpresasCollectionName,
                     MongoDbContext.ProdutosCollectionName,
                     MongoDbContext.FornecedoresCollectionName,
                     MongoDbContext.FatoresEmissaoCollectionName,
                     MongoDbContext.EmissoesCarbonoCollectionName
                 })
        {
            await Database.CreateCollectionAsync(name);
        }

        await ApplyEmissionNullabilityValidatorAsync();
    }

    private Task ApplyEmissionNullabilityValidatorAsync()
    {
        var stringRule = new BsonDocument("bsonType", "string");

        var schema = new BsonDocument
        {
            ["bsonType"] = "object",
            ["properties"] = new BsonDocument
            {
                ["etapa"] = new BsonDocument
                {
                    ["bsonType"] = "object",
                    ["properties"] = new BsonDocument
                    {
                        ["local"] = stringRule
                    }
                },
                ["fatorAplicado"] = new BsonDocument
                {
                    ["bsonType"] = "object",
                    ["properties"] = new BsonDocument
                    {
                        ["fonteReferencia"] = stringRule,
                        ["metodologia"] = stringRule
                    }
                },
                ["fonteEmissao"] = stringRule,
                ["observacao"] = stringRule
            }
        };

        return Database.RunCommandAsync<BsonDocument>(
            new BsonDocument
            {
                ["collMod"] =
                    MongoDbContext.EmissoesCarbonoCollectionName,
                ["validator"] =
                    new BsonDocument("$jsonSchema", schema),
                ["validationLevel"] = "strict",
                ["validationAction"] = "error"
            });
    }

    private async Task CreateUniqueIndexesAsync()
    {
        var empresas = Database.GetCollection<EmpresaDocument>(
            MongoDbContext.EmpresasCollectionName
        );
        await empresas.Indexes.CreateOneAsync(
            new CreateIndexModel<EmpresaDocument>(
                Builders<EmpresaDocument>.IndexKeys.Ascending(
                    empresa => empresa.Cnpj
                ),
                new CreateIndexOptions { Unique = true }
            )
        );

        var produtos = Database.GetCollection<ProdutoDocument>(
            MongoDbContext.ProdutosCollectionName
        );
        await produtos.Indexes.CreateOneAsync(
            new CreateIndexModel<ProdutoDocument>(
                Builders<ProdutoDocument>.IndexKeys.Combine(
                    Builders<ProdutoDocument>.IndexKeys.Ascending(
                        produto => produto.EmpresaId
                    ),
                    Builders<ProdutoDocument>.IndexKeys.Ascending(
                        produto => produto.Codigo
                    )
                ),
                new CreateIndexOptions { Unique = true }
            )
        );

        var fornecedores = Database.GetCollection<FornecedorDocument>(
            MongoDbContext.FornecedoresCollectionName
        );
        await fornecedores.Indexes.CreateOneAsync(
            new CreateIndexModel<FornecedorDocument>(
                Builders<FornecedorDocument>.IndexKeys.Ascending(
                    fornecedor => fornecedor.Cnpj
                ),
                new CreateIndexOptions { Unique = true }
            )
        );

        var fatores = Database.GetCollection<FatorEmissaoDocument>(
            MongoDbContext.FatoresEmissaoCollectionName
        );
        await fatores.Indexes.CreateOneAsync(
            new CreateIndexModel<FatorEmissaoDocument>(
                Builders<FatorEmissaoDocument>.IndexKeys.Combine(
                    Builders<FatorEmissaoDocument>.IndexKeys.Ascending(
                        fator => fator.Codigo
                    ),
                    Builders<FatorEmissaoDocument>.IndexKeys.Ascending(
                        fator => fator.Versao
                    )
                ),
                new CreateIndexOptions { Unique = true }
            )
        );
    }

    private sealed class MongoApiApplicationFactory(
        string connectionString,
        string databaseName
    ) : CustomWebApplicationFactory
    {
        protected override void ConfigureWebHost(
            IWebHostBuilder builder
        )
        {
            base.ConfigureWebHost(builder);

            builder.ConfigureAppConfiguration(
                (_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["MongoDb:ConnectionString"] =
                                connectionString,
                            ["MongoDb:DatabaseName"] = databaseName
                        }
                    );
                }
            );
        }
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class MongoApiCollection
    : ICollectionFixture<MongoApiFixture>
{
    public const string Name = "MongoDB API integration";
}
