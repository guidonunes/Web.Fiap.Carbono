using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Web.Fiap.Carbono.Tests.Config;

// Common API host configuration. Persistence is supplied by MongoApiFixture;
// this factory deliberately does not register EF Core/InMemory test services.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Program reads JWT settings before Build(), so supply these through
        // host configuration before the application entry point executes.
        builder.ConfigureHostConfiguration(configurationBuilder =>
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "fiap-carbono-tests-only-signing-key-2026",
                ["Jwt:Issuer"] = "Web.Fiap.Carbono.Tests",
                ["Jwt:Audience"] = "Web.Fiap.Carbono.Tests.Client",
                ["Jwt:ExpirationMinutes"] = "60"
            }));

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDb:ConnectionString"] = "mongodb://localhost:27017",
                ["MongoDb:DatabaseName"] = "fiap_carbono_test"
            });
        });
    }
}
