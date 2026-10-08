namespace Web.Fiap.Carbono.Dtos.MongoDb.Analytics;

public sealed class ProdutoPegadaMongoResponse
{
    public string IdProduto { get; init; } = string.Empty;

    public string NomeProduto { get; init; } = string.Empty;

    public string NomeEmpresa { get; init; } = string.Empty;

    public decimal TotalCo2e { get; init; }

    public string Unidade { get; init; } = "kgCO2e";

    public IReadOnlyList<EmissaoPorEtapaMongoResponse>
        EmissoesPorEtapa { get; init; } =
        Array.Empty<EmissaoPorEtapaMongoResponse>();
}

public sealed class EmissaoPorEtapaMongoResponse
{
    public string TipoEtapa { get; init; } = string.Empty;

    public decimal TotalCo2e { get; init; }
}