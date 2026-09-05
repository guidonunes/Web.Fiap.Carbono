using System.Text.Json;
using MongoDB.Driver;

namespace Web.Fiap.Carbono.Migration;

public static class MigrationCommand
{
    public static async Task<int> RunAsync(string[] args)
    {
        using var cancellation = new CancellationTokenSource();
        ConsoleCancelEventHandler handler = (_, e) => { e.Cancel = true; cancellation.Cancel(); };
        Console.CancelKeyPress += handler;
        try
        {
            var arguments = args.ToList();
            string? oracleConfig = null;
            var configIndex = arguments.IndexOf("--oracle-config");
            if (configIndex >= 0)
            {
                MigrationPlanner.Check(configIndex + 1 < arguments.Count, "--oracle-config requires a local JSON path.");
                oracleConfig = arguments[configIndex + 1];
                arguments.RemoveRange(configIndex, 2);
            }
            var inventory = arguments.SequenceEqual(["--inventory"]);
            var apply = arguments.Count == 2 && arguments[1] == "--apply";
            var reconcile = arguments.Count == 2 && arguments[1] == "--reconcile";
            MigrationPlanner.Check(inventory || (arguments.Count == 1 && !arguments[0].StartsWith('-')) || apply || reconcile,
                "Usage: migration <policy.json> [--apply|--reconcile] [--oracle-config <local.json>] OR --inventory [--oracle-config <local.json>]");
            MigrationPolicy? policy = null;
            if (!inventory)
            {
                policy = JsonSerializer.Deserialize<MigrationPolicy>(
                    await File.ReadAllTextAsync(arguments[0], cancellation.Token),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                MigrationPlanner.Check(policy is not null, "Invalid policy file.");
                // Check policy before connecting when possible; data validation follows source read.
                MigrationPlanner.Build(new OracleSource(), policy!);
            }
            var connection = Environment.GetEnvironmentVariable("ConnectionStrings__OracleConnection");
            if (string.IsNullOrWhiteSpace(connection) && oracleConfig is not null)
            {
                using var config = JsonDocument.Parse(await File.ReadAllTextAsync(oracleConfig, cancellation.Token));
                connection = config.RootElement.GetProperty("ConnectionStrings").GetProperty("OracleConnection").GetString();
            }
            MigrationPlanner.Check(!string.IsNullOrWhiteSpace(connection),
                "Set ConnectionStrings__OracleConnection or supply --oracle-config with an ignored local file.");
            var source = await OracleSource.ReadAsync(connection!, cancellation.Token);
            Console.WriteLine(JsonSerializer.Serialize(new
            {
                source = "Oracle EC_*", capturedAtUtc = DateTime.UtcNow,
                counts = new { empresas = source.Empresas.Count, produtos = source.Produtos.Count,
                    fornecedores = source.Fornecedores.Count, fatores = source.Fatores.Count,
                    lotes = source.Lotes.Count, etapas = source.Etapas.Count, emissoes = source.Emissoes.Count },
                totalKgCO2e = source.Emissoes.Sum(x => x.QuantidadeEmitida),
                earliestOracleDate = source.Emissoes.Select(x => (DateTime?)x.DataRegistro).Min(),
                latestOracleDate = source.Emissoes.Select(x => (DateTime?)x.DataRegistro).Max(),
                policyInputs = new
                {
                    executingMachineTimeZone = TimeZoneInfo.Local.Id,
                    factors = source.Fatores.Select(x => new
                    {
                        x.IdFator, x.Fonte, x.Escopo, x.UnidadeBase, x.ValorFatorCo2e,
                        x.Referencia, x.Ativo, x.DataCadastro
                    }),
                    stages = source.Etapas.Select(x => new
                    {
                        x.IdEtapa, x.TipoEtapa, x.OrdemEtapa, x.DataInicio, x.DataFim
                    })
                }
            }));
            if (inventory) return 0;

            var plan = MigrationPlanner.Build(source, policy!);
            // Fixed isolated endpoint prevents accidentally using the application's seeded DB.
            var mongo = new MongoClient(new MongoClientSettings
            {
                Server = new MongoServerAddress("127.0.0.1", 27018),
                ServerSelectionTimeout = TimeSpan.FromSeconds(10)
            });
            var database = mongo.GetDatabase("fiap_carbono");
            if (reconcile)
            {
                var report = await new MigrationReconciler(database).RunAsync(plan, cancellation.Token);
                Console.WriteLine(JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true }));
                MigrationPlanner.Check(report.Passed,
                    $"Reconciliation failed: {string.Join(", ", report.Failures.Select(x => x.Requirement))}.");
                Console.WriteLine("Reconciliation passed: counts, Decimal128 totals, dates, references, and legacy IDs match Oracle.");
                return 0;
            }
            var counts = await new MongoMigrationWriter(database)
                .RunAsync(plan, apply, policy!.AcceptReconstructedFactorSnapshots, token: cancellation.Token);
            foreach (var (name, count) in counts)
                Console.WriteLine($"{name}: source={plan[name].Count}, {(apply ? "inserted" : "pending")}={count}.");
            Console.WriteLine(apply
                ? "Migration inserts completed. Run --reconcile against the frozen source and policy."
                : "Dry run passed. No documents inserted.");
            return 0;
        }
        catch (MigrationValidationException e)
        {
            foreach (var error in e.Errors) Console.Error.WriteLine(error);
            return 2;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Cancelled. Target may contain completed batches; retry with the same source and policy.");
            return 130;
        }
        catch (Exception e)
        {
            // Driver exceptions may embed credentials or complete documents.
            Console.Error.WriteLine($"Migration stopped ({e.GetType().Name}). Inspect locally; partial inserts may remain.");
            return 1;
        }
        finally { Console.CancelKeyPress -= handler; }
    }
}
