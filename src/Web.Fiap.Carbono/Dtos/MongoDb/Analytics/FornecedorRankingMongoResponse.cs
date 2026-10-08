namespace Web.Fiap.Carbono.Dtos.MongoDb.Analytics;

public sealed class FornecedorRankingMongoResponse
{
    public string IdFornecedor { get; init; } = string.Empty;

    public string? CodigoFornecedor { get; init; }

    public string NomeFornecedor { get; init; } = string.Empty;

    public decimal TotalCo2e { get; init; }

    public long QuantidadeEmissoes { get; init; }
}