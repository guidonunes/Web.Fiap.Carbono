using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Web.Fiap.Carbono.Config.MongoDb;
using Web.Fiap.Carbono.Data.MongoDb;
using Web.Fiap.Carbono.Data.MongoDb.Repositories;
using Web.Fiap.Carbono.Data.MongoDb.Repositories.Interfaces;

namespace Web.Fiap.Carbono.Tests.Config;

public sealed class MongoDbConfigurationTest(
    CustomWebApplicationFactory factory
) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public void ShouldBindMongoDbSettings()
    {
        var settings = factory.Services
            .GetRequiredService<IOptions<MongoDbSettings>>()
            .Value;

        Assert.Equal(
            "mongodb://localhost:27017",
            settings.ConnectionString
        );
        Assert.Equal("fiap_carbono_test", settings.DatabaseName);
    }

    [Fact]
    public void ShouldReuseMongoDbSingletons()
    {
        var firstClient = factory.Services
            .GetRequiredService<IMongoClient>();
        var secondClient = factory.Services
            .GetRequiredService<IMongoClient>();
        var firstDatabase = factory.Services
            .GetRequiredService<IMongoDatabase>();
        var secondDatabase = factory.Services
            .GetRequiredService<IMongoDatabase>();
        var firstContext = factory.Services
            .GetRequiredService<MongoDbContext>();
        var secondContext = factory.Services
            .GetRequiredService<MongoDbContext>();

        Assert.Same(firstClient, secondClient);
        Assert.Same(firstDatabase, secondDatabase);
        Assert.Same(firstContext, secondContext);
        Assert.Equal(
            "fiap_carbono_test",
            firstDatabase.DatabaseNamespace.DatabaseName
        );
    }

    [Fact]
    public void ShouldResolveExactlyTheFiveApprovedCollections()
    {
        var context = factory.Services
            .GetRequiredService<MongoDbContext>();

        var collectionNames = new[]
        {
            context.Empresas.CollectionNamespace.CollectionName,
            context.Produtos.CollectionNamespace.CollectionName,
            context.Fornecedores.CollectionNamespace.CollectionName,
            context.FatoresEmissao.CollectionNamespace.CollectionName,
            context.EmissoesCarbono.CollectionNamespace.CollectionName
        };

        Assert.Equal(
            new[]
            {
                "empresas",
                "produtos",
                "fornecedores",
                "fatores_emissao",
                "emissoes_carbono"
            },
            collectionNames
        );
        Assert.Equal(5, collectionNames.Distinct().Count());
    }

    [Fact]
    public void ShouldResolveAllFiveMongoDbRepositories()
    {
        using var scope = factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        Assert.IsType<MongoEmpresaRepository>(
            services.GetRequiredService<IMongoEmpresaRepository>()
        );
        Assert.IsType<MongoProdutoRepository>(
            services.GetRequiredService<IMongoProdutoRepository>()
        );
        Assert.IsType<MongoFornecedorRepository>(
            services.GetRequiredService<IMongoFornecedorRepository>()
        );
        Assert.IsType<MongoFatorEmissaoRepository>(
            services.GetRequiredService<IMongoFatorEmissaoRepository>()
        );
        Assert.IsType<MongoEmissaoCarbonoRepository>(
            services.GetRequiredService<IMongoEmissaoCarbonoRepository>()
        );
    }

    [Theory]
    [InlineData(
        "MongoDb:ConnectionString",
        "MongoDb:ConnectionString is required."
    )]
    [InlineData(
        "MongoDb:DatabaseName",
        "MongoDb:DatabaseName is required."
    )]
    public void ShouldFailStartupWhenRequiredSettingIsMissing(
        string missingSetting,
        string expectedMessage
    )
    {
        using var invalidFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration(
                    (_, configurationBuilder) =>
                    {
                        var settings = new Dictionary<string, string?>
                        {
                            ["MongoDb:ConnectionString"] =
                                "mongodb://localhost:27017",
                            ["MongoDb:DatabaseName"] =
                                "fiap_carbono_test",
                            [missingSetting] = string.Empty
                        };

                        configurationBuilder.AddInMemoryCollection(
                            settings
                        );
                    }
                );
            });

        var exception = Assert.ThrowsAny<Exception>(
            () => _ = invalidFactory.Services
        );

        Assert.Contains(expectedMessage, exception.ToString());
    }
}
