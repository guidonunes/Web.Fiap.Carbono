namespace Web.Fiap.Carbono.Dtos.MongoDb.Analytics;

public sealed class DashboardEmpresaMongoResponse
{
    public string IdEmpresa { get; init; } = string.Empty;

    public string NomeEmpresa { get; init; } = string.Empty;

    public decimal TotalCo2e { get; init; }

    public string Unidade { get; init; } = "kgCO2e";

    public int QuantidadeProdutos { get; init; }

    public long QuantidadeEmissoes { get; init; }

    public decimal MediaEmissaoPorProduto { get; init; }

    public string? ProdutoMaisEmissor { get; init; }

    public string? FornecedorMaisEmissor { get; init; }

    public IReadOnlyList<EmissaoPorMesMongoResponse>
        EmissoesPorMes { get; init; } =
        Array.Empty<EmissaoPorMesMongoResponse>();

    public IReadOnlyList<EmissaoPorEscopoMongoResponse>
        EmissoesPorEscopo { get; init; } =
        Array.Empty<EmissaoPorEscopoMongoResponse>();
}

public sealed class EmissaoPorMesMongoResponse
{
    public int Ano { get; init; }

    public int Mes { get; init; }

    public string Periodo => $"{Ano:D4}-{Mes:D2}";

    public decimal TotalCo2e { get; init; }
}

public sealed class EmissaoPorEscopoMongoResponse
{
    public string Escopo { get; init; } = string.Empty;

    public decimal TotalCo2e { get; init; }
}