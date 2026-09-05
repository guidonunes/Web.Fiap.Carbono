using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Web.Fiap.Carbono.Tests.Config;

// Common API host configuration. Persistence is supplied by MongoApiFixture;
// this factory deliberately does not register EF Core/InMemory test services.
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
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
