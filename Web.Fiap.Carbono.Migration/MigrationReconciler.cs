using System.Globalization;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Web.Fiap.Carbono.Migration;

public sealed record ReconciliationCheck(string Requirement, string Expected, string Actual)
{
    public bool Passed => Expected == Actual;
}

public sealed record ReconciliationReport(IReadOnlyList<ReconciliationCheck> Checks)
{
    public bool Passed => Checks.All(x => x.Passed);
    public IReadOnlyList<ReconciliationCheck> Failures => Checks.Where(x => !x.Passed).ToArray();
}

public sealed class MigrationReconciler(IMongoDatabase database)
{
    public async Task<ReconciliationReport> RunAsync(
        Dictionary<string, List<BsonDocument>> sourcePlan,
        CancellationToken token = default)
    {
        MigrationPlanner.Check(sourcePlan.Keys.Order().SequenceEqual(MigrationPlanner.Collections.Order()),
            "Invalid reconciliation collection set.");

        using var collectionCursor = await database.ListCollectionsAsync(cancellationToken: token);
        var collectionNames = (await collectionCursor.ToListAsync(token))
            .Select(x => x["name"].AsString)
            .Where(x => !x.StartsWith("system.", StringComparison.Ordinal))
            .Order()
            .ToArray();
        MigrationPlanner.Check(collectionNames.SequenceEqual(MigrationPlanner.Collections.Order()),
            "Target must contain exactly the five initialized collections.");

        var target = new Dictionary<string, List<BsonDocument>>();
        foreach (var name in MigrationPlanner.Collections)
            target[name] = await database.GetCollection<BsonDocument>(name)
                .Find(FilterDefinition<BsonDocument>.Empty).ToListAsync(token);

        var checks = new List<ReconciliationCheck>();
        void Check(string requirement, object? expected, object? actual) => checks.Add(new(
            requirement, Format(expected), Format(actual)));

        foreach (var name in MigrationPlanner.Collections)
            Check($"count.{name}", sourcePlan[name].Count, target[name].Count);

        var sourceEmissions = sourcePlan["emissoes_carbono"];
        var targetEmissions = target["emissoes_carbono"];
        Check("total.kgCO2e", Sum(sourceEmissions), Sum(targetEmissions));
        Check("totals.perCompany", Totals(sourceEmissions, "empresaId", sourcePlan["empresas"]),
            Totals(targetEmissions, "empresaId", target["empresas"]));
        Check("totals.perProduct", Totals(sourceEmissions, "produtoId", sourcePlan["produtos"]),
            Totals(targetEmissions, "produtoId", target["produtos"]));
        Check("totals.perSupplier", Totals(sourceEmissions, "fornecedorId", sourcePlan["fornecedores"]),
            Totals(targetEmissions, "fornecedorId", target["fornecedores"]));
        Check("date.earliest", Date(sourceEmissions, earliest: true), Date(targetEmissions, earliest: true));
        Check("date.latest", Date(sourceEmissions, earliest: false), Date(targetEmissions, earliest: false));

        var companyIds = Ids(target["empresas"]);
        var productIds = Ids(target["produtos"]);
        var supplierIds = Ids(target["fornecedores"]);
        var factorIds = Ids(target["fatores_emissao"]);
        Check("orphan.produto.empresaId", 0, Orphans(target["produtos"], "empresaId", companyIds));
        Check("orphan.emissao.empresaId", 0, Orphans(targetEmissions, "empresaId", companyIds));
        Check("orphan.emissao.produtoId", 0, Orphans(targetEmissions, "produtoId", productIds));
        Check("orphan.emissao.fornecedorId", 0, Orphans(targetEmissions, "fornecedorId", supplierIds));
        Check("orphan.emissao.fatorEmissaoId", 0, Orphans(targetEmissions, "fatorEmissaoId", factorIds));

        foreach (var name in MigrationPlanner.Collections)
        {
            var expectedLegacyIds = LegacyIds(sourcePlan[name]);
            var actualLegacyIds = LegacyIds(target[name]);
            Check($"legacyId.{name}", expectedLegacyIds, actualLegacyIds);
            Check($"legacyId.unique.{name}", target[name].Count,
                target[name].Select(LegacyId).Where(x => x.HasValue).Distinct().Count());
            Check($"sourcePlan.documentMatch.{name}", sourcePlan[name].Count,
                sourcePlan[name].Count(expected => target[name].Any(actual => actual.Equals(expected))));
        }

        return new ReconciliationReport(checks);
    }

    private static decimal Sum(IEnumerable<BsonDocument> documents) => documents.Sum(x =>
        Decimal128.ToDecimal(x["quantidadeEmitidaKgCO2e"].AsDecimal128));

    private static SortedDictionary<string, decimal> Totals(
        IEnumerable<BsonDocument> documents, string key, IEnumerable<BsonDocument> referencedDocuments)
    {
        var legacyIds = referencedDocuments
            .Where(x => x.TryGetValue("_id", out var id) && id.IsObjectId && LegacyId(x).HasValue)
            .ToDictionary(x => x["_id"].AsObjectId, x => LegacyId(x)!.Value.ToString(CultureInfo.InvariantCulture));
        return documents.GroupBy(x => x.TryGetValue(key, out var id) && id.IsObjectId &&
                legacyIds.TryGetValue(id.AsObjectId, out var legacyId) ? legacyId : "ORPHAN")
            .ToSortedDictionary(x => x.Key, x => x.Sum(y =>
                Decimal128.ToDecimal(y["quantidadeEmitidaKgCO2e"].AsDecimal128)));
    }

    private static DateTime? Date(IEnumerable<BsonDocument> documents, bool earliest)
    {
        var dates = documents.Select(x => x["dataEmissao"].ToUniversalTime()).ToArray();
        return dates.Length == 0 ? null : earliest ? dates.Min() : dates.Max();
    }

    private static HashSet<ObjectId> Ids(IEnumerable<BsonDocument> documents) => documents
        .Where(x => x.TryGetValue("_id", out var value) && value.IsObjectId)
        .Select(x => x["_id"].AsObjectId).ToHashSet();

    private static int Orphans(
        IEnumerable<BsonDocument> documents, string field, HashSet<ObjectId> referencedIds) => documents.Count(x =>
        !x.TryGetValue(field, out var value) || !value.IsObjectId || !referencedIds.Contains(value.AsObjectId));

    private static int? LegacyId(BsonDocument document) =>
        document.TryGetValue("legacyId", out var value) && value.IsInt32 ? value.AsInt32 : null;

    private static int[] LegacyIds(IEnumerable<BsonDocument> documents) => documents
        .Select(LegacyId).Where(x => x.HasValue).Select(x => x!.Value).Order().ToArray();

    private static string Format(object? value) => value switch
    {
        null => "null",
        decimal number => number.ToString("0.############################", CultureInfo.InvariantCulture),
        DateTime date => date.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
        int[] values => JsonSerializer.Serialize(values),
        SortedDictionary<string, decimal> totals => JsonSerializer.Serialize(totals),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? "null"
    };
}

internal static class EnumerableExtensions
{
    internal static SortedDictionary<TKey, TValue> ToSortedDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> source, Func<TSource, TKey> key, Func<TSource, TValue> value)
        where TKey : notnull => new(source.ToDictionary(key, value));
}
