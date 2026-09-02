using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using MongoDB.Bson;
using MongoDB.Driver;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Models.Documents;

namespace Web.Fiap.Carbono.Tests.Data.MongoDb;

public sealed class MongoRepositoryFixture : IAsyncLifetime
{
    private const int MongoPort = 27018;

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
    private MongoClient _client = null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        var settings = MongoClientSettings.FromConnectionString(
            $"mongodb://127.0.0.1:{MongoPort}/?directConnection=true"
        );
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
        _client = new MongoClient(settings);
        await PingAsync(_client);
    }

    public async Task<MongoDbContext> CreateContextAsync()
    {
        var database = _client.GetDatabase(
            $"fiap_carbono_tests_{Guid.NewGuid():N}"
        );

        await CreateCollectionsAsync(database);
        await CreateUniqueIndexesAsync(database);

        return new MongoDbContext(database);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    private static async Task PingAsync(MongoClient client)
    {
        await client
            .GetDatabase("admin")
            .RunCommandAsync<BsonDocument>(
                new BsonDocument("ping", 1)
            );
    }

    private static async Task CreateCollectionsAsync(
        IMongoDatabase database
    )
    {
        var names = new[]
        {
            MongoDbContext.EmpresasCollectionName,
            MongoDbContext.ProdutosCollectionName,
            MongoDbContext.FornecedoresCollectionName,
            MongoDbContext.FatoresEmissaoCollectionName,
            MongoDbContext.EmissoesCarbonoCollectionName
        };

        foreach (var name in names)
        {
            await database.CreateCollectionAsync(name);
        }
    }

    private static async Task CreateUniqueIndexesAsync(
        IMongoDatabase database
    )
    {
        var empresas = database.GetCollection<EmpresaDocument>(
            MongoDbContext.EmpresasCollectionName
        );
        await empresas.Indexes.CreateOneAsync(
            new CreateIndexModel<EmpresaDocument>(
                Builders<EmpresaDocument>.IndexKeys.Ascending(
                    empresa => empresa.Cnpj
                ),
                new CreateIndexOptions
                {
                    Name = "ux_empresas_cnpj",
                    Unique = true
                }
            )
        );

        var produtos = database.GetCollection<ProdutoDocument>(
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
                new CreateIndexOptions
                {
                    Name = "ux_produtos_empresa_codigo",
                    Unique = true
                }
            )
        );

        var fornecedores = database.GetCollection<FornecedorDocument>(
            MongoDbContext.FornecedoresCollectionName
        );
        await fornecedores.Indexes.CreateOneAsync(
            new CreateIndexModel<FornecedorDocument>(
                Builders<FornecedorDocument>.IndexKeys.Ascending(
                    fornecedor => fornecedor.Cnpj
                ),
                new CreateIndexOptions
                {
                    Name = "ux_fornecedores_cnpj",
                    Unique = true
                }
            )
        );

        var fatores = database.GetCollection<FatorEmissaoDocument>(
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
                new CreateIndexOptions
                {
                    Name = "ux_fatores_codigo_versao",
                    Unique = true
                }
            )
        );

        var emissoes = database.GetCollection<EmissaoCarbonoDocument>(
            MongoDbContext.EmissoesCarbonoCollectionName
        );
        await emissoes.Indexes.CreateOneAsync(
            new CreateIndexModel<EmissaoCarbonoDocument>(
                Builders<EmissaoCarbonoDocument>.IndexKeys.Ascending(
                    emissao => emissao.Codigo
                ),
                new CreateIndexOptions
                {
                    Name = "ux_emissoes_codigo_seed",
                    Unique = true
                }
            )
        );
    }
}

[CollectionDefinition(
    Name,
    DisableParallelization = true
)]
public sealed class MongoRepositoryCollection
    : ICollectionFixture<MongoRepositoryFixture>
{
    public const string Name = "MongoDB repository integration";
}
