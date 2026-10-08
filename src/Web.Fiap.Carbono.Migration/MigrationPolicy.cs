namespace Web.Fiap.Carbono.Migration;

public sealed record MigrationPolicy
{
    public string SourceId { get; init; } = "";
    public string OracleTimeZone { get; init; } = "";
    public DateTime MigrationTimestampUtc { get; init; }
    public bool? CompanyActiveDefault { get; init; }
    public DateTime? SupplierCreatedAtUtc { get; init; }
    public bool AcceptReconstructedFactorSnapshots { get; init; }
    public Dictionary<int, FactorPolicy> Factors { get; init; } = [];
    public Dictionary<string, string> StageCategories { get; init; } = [];
}

public sealed record FactorPolicy
{
    public string Category { get; init; } = "";
    public int Version { get; init; }
    public DateTime ValidFromUtc { get; init; }
    public DateTime? ValidUntilUtc { get; init; }
    public string DecisionNote { get; init; } = "";
}

public sealed class MigrationValidationException(IEnumerable<string> errors)
    : Exception("Migration validation failed.")
{
    public IReadOnlyList<string> Errors { get; } = errors.ToArray();
}
